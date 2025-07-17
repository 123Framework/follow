namespace TweeterApp.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int PostId {  get; set; }
        
        public PostModel Post {  get; set; } 

        public ICollection<CommentLikeModel> Likes { get; set; } = new List<CommentLikeModel>();
        
        public bool IsLikedByCurrentUser { get; set; }


        public int? ParentCommentId { get; set; }
        public CommentModel ParentComment { get; set; }
        public ICollection<CommentModel> Replies{ get; set; } = new List<CommentModel>();
    }

}
