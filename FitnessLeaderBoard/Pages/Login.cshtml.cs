using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FitnessLeaderBoard.Data.EntityClasses;
using FitnessLeaderBoard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace FitnessLeaderBoard.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<FlbUser> _signInManager;
        private readonly UserManager<FlbUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly StepDataService _stepDataService;

        [TempData]
        public string ErrorMessage { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public LoginModel(
            SignInManager<FlbUser> signInManager,
            UserManager<FlbUser> userManager,
            ILogger<LoginModel> logger,
            StepDataService stepDataService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _stepDataService = stepDataService;
        }

        public IActionResult OnGetAsync(string provider, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(provider))
                return RedirectToPage("./Login");

            // Request a redirect to the external login provider.
            var redirectUrl
                = Url.Page("/Login", pageHandler: "Callback", 
                values: new { returnUrl });
            var properties
                = _signInManager.ConfigureExternalAuthenticationProperties(
                    provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(
            string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                // Update the users image link
                await UpdateUsersImageLink(info);

                return LocalRedirect(returnUrl);
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new FlbUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Get the users image link
            await UpdateUsersImageLink(info);

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }

        protected async Task UpdateUsersImageLink(ExternalLoginInfo info)
        {
            var user
                = await _userManager.FindByEmailAsync(
                    info.Principal.FindFirstValue(ClaimTypes.Email));

            // Update the picture link
            switch (info.LoginProvider)
            {
                case "Google":
                    user.ImageLink
                        = info.Principal.FindFirstValue("urn:google:image");
                    break;
                case "Facebook":
                    var identifier = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    user.ImageLink
                        = !string.IsNullOrEmpty(identifier)
                        ? string.Format("https://graph.facebook.com/{0}/picture", identifier)
                        : string.Empty;
                    break;
            }

            // Update the user info with the user's profile image
            await _userManager.UpdateAsync(user);

            // Update the user's profile image in the leaderboard
            await _stepDataService.UpdateUserInfoInLeaderboard(user);
        }
    }
}
