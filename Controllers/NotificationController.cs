using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TweeterApp.Models;
using TweeterApp.Repository;

namespace TweeterApp.Controllers
{
    public class NotificationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserNotificationRepository _notificationRepository;
        public NotificationController(UserManager<ApplicationUser> userManager, IUserNotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _notificationRepository.GetUserNotificationAsync(user.Id);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationRepository.MarkAsReadAsync(id);
            return RedirectToAction("Index");
        }


    }
}
