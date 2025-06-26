namespace TweeterApp.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public PostModel Post { get; set; }
        public IEnumerable<CommentModel> Comments { get; set; }

        
    }

}
