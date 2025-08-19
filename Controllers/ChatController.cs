using Microsoft.AspNetCore.Mvc;
using TweeterApp.Data;

namespace TweeterApp.Controllers
{

    public class ChatController : Controller
    {
        [HttpGet("/chat/with/{**username}")]
        public IActionResult With(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return NotFound();
            ViewData["OtherUser"] = username;
            return View("index");
        }
        /*
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var messages = await _context.Messages
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string username, string text)
        {
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(text) {

            }
        }
        */
        public IActionResult Index()
        {
            return View();
        }
    }
}
