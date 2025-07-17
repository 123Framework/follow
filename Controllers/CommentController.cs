using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;
using TweeterApp.Repository;


namespace TweeterApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPostRepository _postRepository;

        public CommentController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _postRepository = postRepository;
        }

        public async Task<IActionResult> GetComments(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var comments = await _commentRepository.GetByPostIdAsync(postId);
            return PartialView("_CommentsPartial", comments);
        }


        // POST: CommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCommentViewModel model)
        {
            var comment = await _commentRepository.GetByIdAsync(model.CommentId);
            if (comment == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (comment.UserId != currentUser.Id)
            {
                return Forbid();
            }
            comment.Content = model.Content;
            await _commentRepository.UpdateAsync(comment);
            return RedirectToAction("Index", "Post", new { postId = model.PostId });
        }


        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (User == null || comment.UserId != user.Id)
            {
                return Forbid();
            }
            var model = new EditCommentViewModel
            {
                CommentId = comment.Id,
                Content = comment.Content,
                PostId = comment.PostId,
            };
            return View(model);
        }
        // GET: CommentController/Delete/5
        [HttpPost]
        public async Task<IActionResult> Add(int postId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Comment cannot be empty");
                return BadRequest(ModelState);
            }
            var user = await _userManager.GetUserAsync(User);
            var comment = new CommentModel
            {
                PostId = postId,
                Content = content,
                CreatedDate = DateTime.UtcNow,
                UserId = user.Id,
                ParentCommentId = parentCommentId
            };
            await _commentRepository.AddAsync(comment);
            return RedirectToAction("Details", "Post", new { id = postId });
        }



        // POST: CommentController/Delete/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentid)
        {
            var comment = await _commentRepository.GetByIdAsync(commentid);

            if (comment == null)
            {
                return NotFound();

            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || comment.UserId != user.Id)
            {
                return Forbid();
            }

            await _commentRepository.DeleteAsync(commentid);
            return RedirectToAction("Details", "Post", new { Id = comment.PostId });
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (comment.UserId != user.Id || user == null)
            {
                return Forbid();
            }
            return View(comment);


        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int commentId, int postId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Forbid();
            }


            bool liked = await _commentRepository.ToggleLikeAsync(commentId, postId);
            if (liked)
            {
                TempData["Notification"] = "liked";
            }
            else {
                TempData["Notification"] = "like removed";
            }

            await _commentRepository.ToggleLikeAsync(commentId, user.Id);
            return RedirectToAction("Details", "Post", new { id = postId });
        }



    }
}
