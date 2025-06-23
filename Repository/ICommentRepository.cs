using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface ICommentRepository 
    {
        Task<IEnumerable<CommentModel>> GetByPostIdAsync(int postId);
        Task<CommentModel> GetByIdAsync(int id);
        Task AddAsync(CommentModel comment);
        Task UpdateAsync(CommentModel comment);
        Task DeleteAsync(int id);

        Task<IEnumerable<CommentModel>> GetRecentCommentsByPostIdAsync(int postId, int count = 3);

        Task<bool> ToggleLikeAsync(int commentId, string userId);
        Task<int> GetLikeCountAsync(int commentId);

        Task<bool> IsLikedByCurrentUser(int commentId, string userId);
        Task<IEnumerable<CommentModel>> GetCommentsFoPostAsync(int PostId, string currentUserId = null);
    }
}
