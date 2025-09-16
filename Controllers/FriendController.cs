using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("friends")]
    public class FriendController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public FriendController(ApplicationDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var me = User.Identity!.Name!;
            var friends = await _db.Friendships
                .Where(f => f.Status == Models.FriendshipStatus.Accepted && (f.RequesterUserName == me || f.AddresseeUserName == me))
                .Select(f => f.RequesterUserName == me ? f.AddresseeUserName : f.RequesterUserName)
                .OrderBy(s => s)
                .ToListAsync();
            return Ok(friends);
        }


        [HttpGet("requests")]
        public async Task<IActionResult> Requests()
        {
            var me = User.Identity!.Name!;
            var incoming = await _db.Friendships
                .Where(f => f.Status == Models.FriendshipStatus.Pending && f.AddresseeUserName == me)
                .Select(f => f.RequesterUserName)
                .ToListAsync();
            var outgoing = await _db.Friendships
                .Where(f => f.Status == Models.FriendshipStatus.Pending && f.RequesterUserName == me)
                .Select(f => f.AddresseeUserName)
                .ToListAsync();

            return Ok(new { incoming, outgoing });

        }
        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] string toUserName)
        {
            var me = User.Identity!.Name!;
            toUserName = toUserName?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(toUserName) || toUserName == me) return BadRequest();


            var exists = await _db.Friendships.AnyAsync(f => (f.RequesterUserName == me && f.AddresseeUserName == toUserName) || (f.RequesterUserName == toUserName && f.AddresseeUserName == me));
            if (!exists) return Conflict("Already exists");
            _db.Friendships.Add(new FriendModel
            {
                RequesterUserName = me,
                AddresseeUserName = toUserName,
                Status = FriendshipStatus.Pending,
            });
            await _db.SaveChangesAsync();
            await _hub.Clients.User(toUserName).SendAsync("FriendReuquestIncoming", me);
            return Ok();
        }
        public class RespondDto
        {
            public string FromUserName { get; set; } = "";
            public bool Accept { get; set; }
        }
        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondDto dto)
        {
            var me = User.Identity!.Name!;
            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterUserName == dto.FromUserName &&
            f.AddresseeUserName == me &&
            f.Status == FriendshipStatus.Pending);

            if (fr == null) return NotFound();

            fr.Status = dto.Accept ? FriendshipStatus.Accepted : FriendshipStatus.Declined;
            fr.RespondedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            
            await _hub.Clients.User(dto.FromUserName).SendAsync("FriendRequestResult", me, dto.Accept ? "accepted" : "declined");

            await _hub.Clients.User(me).SendAsync("FriendListUpdated");

            return Ok();
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] string other)
        {
            var me = User.Identity!.Name;


            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>(f.RequesterUserName == me && f.AddresseeUserName == other) || 
            (f.RequesterUserName == other && f.AddresseeUserName == me));

        if (fr == null) return NotFound();
        _db.Friendships.Remove(fr);
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, other).SendAsync("FriendListUpdated");
            return Ok();
        }

        
    }
}
