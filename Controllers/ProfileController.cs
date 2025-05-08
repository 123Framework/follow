using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;
using TweeterApp.Views.Profile;

namespace TweeterApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        public readonly FollowRepository _followRepository;
        

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env, FollowRepository followRepository)
        {
            _userManager = userManager;
            _env = env;
            _followRepository = followRepository;
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
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var maxFileSizeInBytes = 2 * 1024 * 1024; //2 MB

                var extension = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Avatar", "only .jpg, .jpeg, .png, .gif files are allowed");
                    return View(model);
                }

                if (model.Avatar.Length > maxFileSizeInBytes) {
                    ModelState.AddModelError("Avatar", "file size must be less 2MB");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(user.AvatarPath) && !user.AvatarPath.Contains("default"))
                { 
                    var oldPath = Path.Combine(_env.WebRootPath, user.AvatarPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

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
            var followers = await _followRepository.GetFollowersAsync(user.Id);
            var following = await _followRepository.GetFollowingAsync(user.Id);

            
            if (user == null) return RedirectToAction("Login", "Account");
            var model = new MyProfileViewModel
            {
                User = user,
                Followers = followers.ToList(),
                Following = following.ToList(),
            };
            return View(user);
        }


    }
}
