using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        public readonly IPostRepository _postRepository;
        public readonly UserManager<ApplicationUser> _userManager;
        private ILogger<AccountController> _logger;

        public PostController(IPostRepository postRepository, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _logger = logger;

        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllAsync();
            return View(posts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostModel Post)
        {
            //if (ModelState.IsValid)

            var user = await _userManager.GetUserAsync(User);
            Post.UserId = user.Id;
            Post.CreatedDate = DateTime.UtcNow;
            await _postRepository.AddAsync(Post);
            return RedirectToAction("Index");

            //return View(Post);
        }

        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (post == null || post.UserId != user.Id)
            {
                return Forbid();
            }
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostModel post)
        {
            if (id != post.Id) return NotFound();
            var user = await _userManager.GetUserAsync(User);

            _logger.LogInformation("post.UserId = {postUserId}, user.Id = {User.Id}", post.UserId, user.Id);
            if (post.UserId != user.Id)
            {
                _logger.LogWarning("User {UserId} tried to edit post {PostId} without permission", user.Id, post.Id);
                return Forbid();
            }
                       
                
            await _postRepository.UpdateAsync(post);
            return RedirectToAction("Index");
            //if (ModelState.IsValid)
            //{

            // }
            // return View(post);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (post == null || post.UserId != user.Id)
            {
                return Forbid();
            }
            return View(post);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post.UserId != int.Parse(_userManager.GetUserId(User))) return Forbid(); // used to be UserModel

            await _postRepository.DeleteAsync(id);
            return RedirectToAction("Index");

        }
    }
}
