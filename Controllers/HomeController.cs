using Microsoft.AspNetCore.Mvc;
using TweeterApp.Data;
using TweeterApp.Models.ViewModels;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var posts = _context.Posts
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new PostViewModel
            {
                Post = p,
                Comments = _context.Comments
                    .Where(c => c.PostId == p.Id)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToList(),
                LikeCount = 0, // сюда вставь реальное значение лайков, если считаешь
                IsLikedByCurrentUser = false // тоже можно обновить, если у тебя есть лайки
            })
            .ToList();

        return View(posts);
    }
}