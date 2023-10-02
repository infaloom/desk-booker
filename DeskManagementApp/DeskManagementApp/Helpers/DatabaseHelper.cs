using DeskManagementApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using DeskManagementApp.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DeskManagementApp.Helpers
{
    public class DatabaseHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IUserStore<IdentityUser> _userStore;
        public DatabaseHelper(IConfiguration configuration, IUserStore<IdentityUser> userStore) 
        {
            _configuration = configuration;
            _userStore = userStore;
        }

        public async Task CreateInitialUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var emailStore = GetEmailStore(serviceProvider);
            var emailSender = new EmailSender(_configuration);

            var user = await userManager.FindByNameAsync("Admin");

            if(user == null)
            {
                user = CreateUser();
                user.Email = "dimitrije@infaloom.com";
                user.UserName = "Admin";
                var result = await userManager.CreateAsync(user, "Test!234");

                await _userStore.SetUserNameAsync(user, "Admin", CancellationToken.None);
                await emailStore.SetEmailAsync(user, "dimitrije@infaloom.com", CancellationToken.None);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string backendUrl = _configuration["BackendUrl"];
                    string callbackUrl = $"{backendUrl}/api/account/confirm?userId={user.Id}&token={code}";

                    await emailSender.SendEmailAsync("dimitrije@infaloom.com", "Confirm your email",
                         $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.\nYou need to reset your password in order to start using the app!");
                }
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

        private IUserEmailStore<IdentityUser> GetEmailStore(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
