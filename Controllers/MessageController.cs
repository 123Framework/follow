using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var user =  await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }
        public async Task<IActionResult> Outbox()
        {
            var user =  await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var messages = await _context.Messages
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }

        public IActionResult Send()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Send(string recieverEmail, string subject, string body)
        {

            var sender = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(recieverEmail))
            {

                ModelState.AddModelError("", "Receiver email is required");
                return View();
            }

            var receiver = await _userManager.FindByEmailAsync(recieverEmail);
            if (receiver == null) {
                ModelState.AddModelError("", "User not found");
                return View();
            }
            var Message = new MessageModel
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
            };
            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();
            return RedirectToAction("Outbox");
        }

    }
}
