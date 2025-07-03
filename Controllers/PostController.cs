using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        public readonly IPostRepository _postRepository;
        public readonly UserManager<ApplicationUser> _userManager;
        private ILogger<AccountController> _logger;
        public readonly ILikeRepository _likeRepository;
        public readonly ICommentRepository _commentRepository;
        public PostController(IPostRepository postRepository, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger, ILikeRepository likeRepository, ICommentRepository commentRepository)
            {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
            _userManager = userManager;
            _commentRepository = commentRepository;
            _logger = logger;

        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllAsync();
            var user = await _userManager.GetUserAsync(User);
            var result = new List<PostViewModel>();
            


            foreach (var post in posts) {
                var isLiked = await _likeRepository.IsLikedAsync(user.Id, post.Id);
                var likeCount = await _likeRepository.GetLikeCountAsync(post.Id);


                var comments = await _commentRepository.GetByPostIdAsync(post.Id);
                post.Comments = comments.ToList();

                result.Add(new PostViewModel
                {
                    Post = post,
                    IsLikedByCurrentUser = isLiked,
                    LikeCount = likeCount,
                    Comments = comments.Take(3)
                });
            }
            return View(result);
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostModel Post, IFormFile imageFile)
        {
            if ( imageFile != null && imageFile.Length > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(extension)) {
                    ModelState.AddModelError("Image", "Only .jpg, .jpeg, .png, .gif are allowed");
                }
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                Post.ImagePath = "/uploads/" + fileName;
            }
            //if (ModelState.IsValid)

            var user = await _userManager.GetUserAsync(User);
            Post.UserId = user.Id;
            Post.CreatedDate = DateTime.UtcNow;
            await _postRepository.AddAsync(Post);
            return RedirectToAction("Index");

            //return View(Post);
        }

        



        public async Task<IActionResult> Details(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var comments = await _commentRepository.GetByPostIdAsync(id);
            var isLiked = await _likeRepository.IsLikedAsync(user.Id, post.Id);
            var LikeCount = await _likeRepository.GetLikeCountAsync(post.Id);



            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                IsLikedByCurrentUser = isLiked,
                LikeCount = LikeCount,
                Comments = comments.ToList(),
            };
            return View(viewModel);
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
