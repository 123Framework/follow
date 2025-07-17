using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public class SavedPostRepository : ISavedPostRepository
    {
        private readonly ApplicationDbContext _context;
        public SavedPostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostModel>> GetSavedPostsByUserAsync(int userId)
        {
            return await _context.SavedPosts.Where(sp => sp.UserId == userId).Include(sp => sp.Post)
                 .ThenInclude(p => p.User)
                 .Select(sp => sp.Post)
                 .ToListAsync();
        }

        public async Task<bool> IsPostSavedAsync(int postId, int userId)
        {
            return await _context.SavedPosts.AnyAsync(sp => sp.PostId == postId && sp.UserId == userId);

        }

        public async Task SavePostAsync(int postId, int userId)
        {
            var exists = await _context.SavedPosts
                .AnyAsync(sp => sp.PostId == postId && sp.UserId == userId);
            if (!exists)
            {
                _context.SavedPosts.Add(new SavedPostModel { PostId = postId, UserId = userId });
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnsavePostAsync(int postId, int userId)
        {
           var saved = await _context.SavedPosts.FirstOrDefaultAsync(sp => sp.PostId==postId && sp.UserId == userId);

            if (saved != null)
            {
                _context.SavedPosts.Remove(saved);
                await _context.SaveChangesAsync();
            }
        }
    }
}
