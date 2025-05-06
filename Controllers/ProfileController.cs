using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;

namespace TweeterApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            

            var model = new ProfileEditViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                CurrentAvatarPath = user.AvatarPath
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            if (model.Avatar != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Avatar.FileName);
                var filepath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }

             
                
                user.AvatarPath = "/uploads/" + fileName;
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction("MyProfile");
        }
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            return View(user);
        }
    }
}
