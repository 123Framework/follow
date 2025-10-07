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
    [Route("friends/api")]
    public class FriendController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public FriendController(ApplicationDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var me = User.Identity!.Name!;
            var friends = await _db.Friendships
                .Where(f => f.Status == Models.FriendshipStatus.Accepted && (f.RequesterUserName == me || f.AddresseeUserName == me))
                .Select(f => f.RequesterUserName == me ? f.AddresseeUserName : f.RequesterUserName)
                .OrderBy(s => s)
                .ToListAsync();
            return Ok(new { friends });
        }

        // GET /friends/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var me = User.Identity!.Name!;

            var incoming = await _db.Friendships
                .Where(f => f.Status == FriendshipStatus.Pending && f.AddresseeUserName == me)
                .Select(f => f.RequesterUserName)
                .ToListAsync();

            var outgoing = await _db.Friendships
                .Where(f => f.Status == FriendshipStatus.Pending && f.RequesterUserName == me)
                .Select(f => f.AddresseeUserName)
                .ToListAsync();

            return Ok(new { incoming, outgoing });
        }
        //[HttpGet("requests")]
        /*public async Task<IActionResult> Requests()
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

        }*/
        // [HttpPost("request")]
        /*public async Task<IActionResult> SendRequest([FromBody] string toUserName)
        {
            var me = User.Identity!.Name!;
            toUserName = toUserName?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(toUserName) || toUserName == me) return BadRequest();


            var exists = await _db.Friendships.AnyAsync(f => (f.RequesterUserName == me && f.AddresseeUserName == toUserName) || (f.RequesterUserName == toUserName && f.AddresseeUserName == me));
            if (exists) return Conflict("Already exists");
            _db.Friendships.Add(new FriendModel
            {
                RequesterUserName = me,
                AddresseeUserName = toUserName,
                Status = FriendshipStatus.Pending,
            });
            await _db.SaveChangesAsync();
            await _hub.Clients.User(toUserName).SendAsync("FriendRequestIncoming", me);
            return Ok();
        }*/
        public class RequestDto { public string? ToUserName { get; set; } }
        public class RespondDto { public string? WithUserName { get; set; } }



        // POST /friends/request

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] RequestDto dto)
        {
            var me = User.Identity!.Name!;
            var toUserName = (dto?.ToUserName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(toUserName))
                return BadRequest("ToUserName is required.");
            var existing = await _db.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterUserName == me && f.AddresseeUserName == toUserName) ||
                (f.RequesterUserName == toUserName && f.AddresseeUserName == me));

            if (existing != null)
            {
                if (existing.Status == FriendshipStatus.Pending)
                    return Conflict("A pending request already exists.");
                if (existing.Status == FriendshipStatus.Accepted)
                    return Conflict("You are already friends.");
                _db.Friendships.Remove(existing);
            }
            _db.Friendships.Add(new FriendModel
            {
                RequesterUserName = me,
                AddresseeUserName = toUserName,
                Status = FriendshipStatus.Pending
            });
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, toUserName).SendAsync("FriendListUpdated");
            return Ok();
        }



        /* public class RespondDto
          {
              public string FromUserName { get; set; } = "";
              public bool Accept { get; set; }
          */
        //[HttpPost("respond")]
        /*  public async Task<IActionResult> Respond([FromBody] RespondDto dto)
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
          }*/


        [HttpGet]
        public Task<IActionResult> ListRoot() => List();


        // POST /friends/accept

        [HttpPost("accept")]
        public async Task<IActionResult> Accept([FromBody] RespondDto dto)
        {
            var me = User.Identity!.Name!;
            var other = (dto?.WithUserName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(other)) return BadRequest("WithUserName is required.");

            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
                f.RequesterUserName == other &&
                f.AddresseeUserName == me &&
                f.Status == FriendshipStatus.Pending);

            if (fr == null) return NotFound("No pending request found.");

            fr.Status = FriendshipStatus.Accepted;
            fr.RespondedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, other).SendAsync("FriendListUpdated");
            return Ok();
        }


        // POST /friends/decline
        [HttpPost("decline")]
        public async Task<IActionResult> Decline([FromBody] RespondDto dto)
        {
            var me = User.Identity!.Name!;
            var other = (dto?.WithUserName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(other)) return BadRequest("WithUserName is required.");

            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
                f.RequesterUserName == other &&
                f.AddresseeUserName == me &&
                f.Status == FriendshipStatus.Pending);

            if (fr == null) return NotFound("No pending request found.");

            fr.Status = FriendshipStatus.Declined;
            fr.RespondedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, other).SendAsync("FriendListUpdated");
            return Ok();
        }


        [HttpGet("/friends")]
        public IActionResult Index() { return View("Index"); }



        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] string otherUserName)
        {
            var me = User.Identity!.Name;
            var other = (otherUserName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(other)) return BadRequest("Username is required");
            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
            (f.RequesterUserName == me && f.AddresseeUserName == other) ||
            (f.RequesterUserName == other && f.AddresseeUserName == me) &&
            f.Status == FriendshipStatus.Accepted);

            if (fr == null) return NotFound();
            _db.Friendships.Remove(fr);
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, other).SendAsync("FriendListUpdated");
            return Ok();
        }
        [HttpGet("status/{otherUserName}")]
        public async Task<IActionResult> Status(string otherUserName)
        {
            var me = User.Identity!.Name!;
            var other = (otherUserName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(other)) return BadRequest();
            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
           (f.RequesterUserName == me && f.AddresseeUserName == other) ||
            (f.RequesterUserName == other && f.AddresseeUserName == me) &&
            f.Status == FriendshipStatus.Accepted);

            if (fr == null) return Ok(new { status = "none" });

            var status = fr.Status.ToString().ToLowerInvariant();
            var direction = fr.RequesterUserName == me ? "outgoing" :
                fr.AddresseeUserName == me ? "incoming" : "unknown";
            return Ok(new { status, direction });

        }
        // POST /friends/cancel (cancel my outgoing pending request)
        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] RespondDto dto)
        {
            var me = User.Identity!.Name!;
            var other = (dto?.WithUserName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(other)) return BadRequest("WithUserName is required.");

            var fr = await _db.Friendships.FirstOrDefaultAsync(f =>
                f.RequesterUserName == me &&
                f.AddresseeUserName == other &&
                f.Status == FriendshipStatus.Pending);

            if (fr == null) return NotFound("No outgoing pending request found.");

            _db.Friendships.Remove(fr);
            await _db.SaveChangesAsync();

            await _hub.Clients.Users(me, other).SendAsync("FriendListUpdated");
            return Ok();
        }





    }
}
