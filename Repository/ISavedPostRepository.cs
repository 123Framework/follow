using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ISavedPostRepository
    {
        Task<IEnumerable<PostModel>> GetSavedPostsByUserAsync(int userId);
        Task SavePostAsync(int postId, int userId);
        Task UnsavePostAsync(int postId, int userId);
        Task<bool> IsPostSavedAsync(int postId, int userId);
    }
}
