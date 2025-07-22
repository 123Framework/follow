using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class SavedPostController : Controller
    {
        private readonly ISavedPostRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public SavedPostController(ISavedPostRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> Toggle(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            var IsSaved = await _repository.IsPostSavedAsync(postId, user.Id);

            if (IsSaved) {
                await _repository.UnsavePostAsync(postId, user.Id);
            }
            else
                await _repository.SavePostAsync(postId, user.Id);
                
            
        return RedirectToAction("Index","Post");
        }
        public async Task<IActionResult> Saved()
        {
            var user = await _userManager.GetUserAsync(User);
            var savedPosts = await _repository.GetSavedPostsByUserAsync(user.Id);
            return View(savedPosts);
        }
    }
}
