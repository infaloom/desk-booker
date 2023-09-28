using DeskManagementApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace DeskManagementApp.Controller
{
    [Route("config")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender = new EmailSender();
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public ConfigurationController(

            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            bool systemConfigured = _cache.Get<bool>("SystemConfigured");

            if (systemConfigured != null && systemConfigured)
            {
                return Ok(systemConfigured);
            }
            else
            {
                var users = await _userManager.Users.ToListAsync();

                var user = users.FirstOrDefault(u => u.EmailConfirmed && _userManager.IsInRoleAsync(u, "Admin").Result);

                if (user != null)
                {
                    _cache.Set("SystemConfigured", true);
                    return Ok(true);
                }
                else
                {
                    _cache.Set("SystemConfigured", false);
                    return Ok(false);
                }
            }

        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ConfigureRequestDTO dto)
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
                    await _userManager.AddToRoleAsync(user, "Admin");
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


    public class ConfigureRequestDTO
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
}
