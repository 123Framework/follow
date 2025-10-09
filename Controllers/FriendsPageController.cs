using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TweeterApp.Controllers
{
    [Authorize]
    [Route("friends")]
    public class FriendsPageController : Controller
    {
        [HttpGet("/friends")]
        public IActionResult Index() { return View("~/Views/Friend/Index.cshtml"); }

    }
}
