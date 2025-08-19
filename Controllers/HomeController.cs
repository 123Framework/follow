using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Data;
using TweeterApp.Models;
using TweeterApp.Models.ViewModels;

public class StartChatViewModel
{
    public string? SelectedEmail { get; set; }
    public List<string> Users { get; set; } = new();


}

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        /* var posts = _context.Posts
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


         return View(posts);*/
        var me = User?.Identity?.Name ?? "";
        var emails = _userManager.Users
            .Select(u => u.Email!)
            .Where(e => e!= null && e != me)
            .OrderBy(e => e)
            .ToList();
        var vm = new StartChatViewModel { Users = emails };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult StartChat(StartChatViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SelectedEmail))
        {
            ModelState.AddModelError(nameof(model.SelectedEmail), "Выберите получателя");
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("with", "Chat", new { username = model.SelectedEmail});
    }

}