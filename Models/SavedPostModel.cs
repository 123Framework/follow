namespace TweeterApp.Models
{
    public class SavedPostModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }

        public PostModel Post { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;


    }
}
