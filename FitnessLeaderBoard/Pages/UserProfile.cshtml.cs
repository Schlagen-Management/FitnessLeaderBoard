using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FitnessLeaderBoard.Data.EntityClasses;
using FitnessLeaderBoard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitnessLeaderBoard.Pages
{
    public partial class UserProfileModel : PageModel
    {
        private readonly UserManager<FlbUser> _userManager;
        private readonly SignInManager<FlbUser> _signInManager;
        private readonly StepDataService _stepDataService;

        public UserProfileModel(
            UserManager<FlbUser> userManager,
            SignInManager<FlbUser> signInManager,
            StepDataService stepDataService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _stepDataService = stepDataService;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }

            [Display(Name = "Profile Image")]
            public string ImageLink { get; set; }
        }

        private async Task LoadAsync(FlbUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                DisplayName = user.DisplayName,
                ImageLink = user.ImageLink
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string isCancel)
        {
            if (isCancel == "true")
                return LocalRedirect("~/");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.FullName != user.FullName)
                user.FullName = Input.FullName;

            if (Input.DisplayName != user.DisplayName)
                user.DisplayName = Input.DisplayName;

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            // Update the user's info in the leaderboard
            await _stepDataService.UpdateUserInfoInLeaderboard(user);

            return LocalRedirect("~/");
        }
    }
}
