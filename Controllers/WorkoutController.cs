using FitLog.Domain.Entities;
using FitLog.Infrastructure.Data;
using FitLog.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitLog.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]


    public class WorkoutController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public WorkoutController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetWotkouts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var workouts = await _db.Workouts
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
            return Ok(workouts);

        }
        [HttpPost]
        public async Task<IActionResult> AddWorkout([FromBody] WorkoutDto dto)
        {
            var user = await _userManager.GetUserAsync (User);
            if (user == null) return Unauthorized();

            var workout = new Workout
            {
                UserId = user.Id,
                Title = dto.Title,
                Date = dto.Date == default ? DateTime.UtcNow : dto.Date,
                DurationMin = dto.DurationMin,
                CaloriesBurned = dto.CaloriesBurned
            };
            _db.Workouts.Add(workout);
            await _db.SaveChangesAsync();
            return Ok(workout);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var workout = await _db.Workouts.FirstOrDefaultAsync (w => w.Id == id && w.UserId == user.Id);

            if (workout == null) return NotFound();
            _db.Workouts.Remove(workout);
            await _db.SaveChangesAsync();
            return Ok(new {message = "Workout deleted"});
        }
    }
}
