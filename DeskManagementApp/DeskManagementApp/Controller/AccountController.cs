using DeskManagementApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static DeskManagementApp.Controller.DeskController;
using NuGet.Common;
using DeskManagementApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DeskManagementApp.Controller
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender = new EmailSender();
        private readonly IConfiguration _configuration;
        private readonly IHubContext<EmployeeHub> _employeeHubContext;

        public AccountController(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            IHubContext<EmployeeHub> employeeHubContext)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _employeeHubContext = employeeHubContext;
        }

        [AllowAnonymous]
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid user ID or token.");
            }

            string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                var request = HttpContext.Request;
                var rootAddress = GetRootAddress(request);

                var userDto = new UserDTO();
                userDto.Name = user.UserName;
                userDto.Id = user.Id;

                bool admin = await _userManager.IsInRoleAsync(user, "Admin");
                bool deskAdmin = await _userManager.IsInRoleAsync(user, "DeskAdmin");

                if (admin)
                {
                    userDto.Role = "Admin";
                }
                else if (deskAdmin)
                {
                    userDto.Role = "DeskAdmin";
                }
                userDto.Role = "User";
                var message = new ActionDTO
                {
                    Action = "create",
                    UserDTO = userDto
                };

                await _employeeHubContext.Clients.All.SendAsync("EmployeeNotify", message);

                return Redirect($"{rootAddress}/login");
            }
            else
            {
                return BadRequest("Email confirmation failed.");
            }
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RegisterRequestDTO dto)
        {

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Email = dto.Email;
                user.UserName = dto.UserName;
                var result = await _userManager.CreateAsync(user, dto.Password);

                await _userStore.SetUserNameAsync(user, dto.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, dto.Email, CancellationToken.None);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var request = HttpContext.Request;
                    var rootAddress = GetRootAddress(request);
                    string callbackUrl = $"{rootAddress}/api/account/confirm?userId={user.Id}&token={code}";

                    await _emailSender.SendEmailAsync(dto.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return Ok();
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(errors);
                }
            }
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequestDTO dto)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(dto.UserName);
                if (user != null)
                {
                    var request = HttpContext.Request;
                    var rootAddress = GetRootAddress(request);
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string callbackUrl = $"{rootAddress}/reset?userId={user.Id}&token={code}";

                    await _emailSender.SendEmailAsync(user.Email, "Reset your password",
                        $"Once you click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>the link</a>, you'll be directed to a page where you can securely set a new password. ");

                    return Ok(true);
                }
                else
                {
                    return BadRequest(false);
                }
            }
            return BadRequest(false);
        }

        [AllowAnonymous]
        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromBody] ResetPasswordRequestDTO dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);

                string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.Password);

                if (result.Succeeded)
                {
                    return Ok(true);
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(errors);
                }
            }
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(dto.UserName);
                if (user == null)
                {
                    return NotFound();
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, dto.RememberMe);
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var secretKey = _configuration["SecretKey"];
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(secretKey);
                    //var key = new byte[256 / 8];
                    //Array.Copy(keyBytes, key, Math.Min(keyBytes.Length, key.Length));
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, dto.UserName),
                            new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id)
                        }),
                        Expires = DateTime.UtcNow.AddDays(7), 
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    return Ok(new
                    {
                        Token = tokenString,
                        User = new
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Role = roles.FirstOrDefault()
                        }
                    });
                }
                else if (result.RequiresTwoFactor)
                {
                    return StatusCode(201, "Requires two-factor authentication.");
                }
                else if (result.IsLockedOut)
                {
                    return StatusCode(403, "User account is locked.");
                }
                else
                {
                    return BadRequest("Invalid login attempt.");
                }
            }
            else
            {
                return BadRequest("Invalid model state.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsernameById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { username = user.UserName });
        }

        private string GetRootAddress(HttpRequest request)
        {
            if (request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                request.Host.Port == 5067)
            {
                return "http://localhost:5067/";
            }
            else if (request.Host.Host.Equals("desks.infaloom.com", StringComparison.OrdinalIgnoreCase))
            {
                return "https://desks.infaloom.com/";
            }
            else
            {
                return "";
            }
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }

    public class RegisterRequestDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginRequestDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordRequestDTO
    {
        [Required]
        public string UserName { get; set; }
    }

    public class ResetPasswordRequestDTO
    {
        [Required]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class ActionDTO
    {
        [Required]
        public UserDTO UserDTO { get; set; }
        [Required]
        public string Action { get; set; }
    }
}
