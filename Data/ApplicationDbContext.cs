using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TweeterApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace TweeterApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<SavedPostModel> SavedPosts { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<CommentLikeModel> CommentLikes { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<FollowModel> Follows { get; set; }
        public DbSet<LikeModel> Likes { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<MessageModel> Messages{ get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FollowModel>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FollowModel>()
                .HasOne(f => f.Followee)
                .WithMany()
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LikeModel>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LikeModel>()
                .HasOne(f => f.Post)
                .WithMany()
                .HasForeignKey(f => f.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentModel>()
                 .HasOne(c => c.Post)
                 .WithMany(p => p.Comments)
                 .HasForeignKey(c => c.PostId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.User)
                 .WithMany(p => p.Comments)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostModel>()
                .HasMany(p => p.Comments)
                 .WithOne(p => p.Post)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedPostModel>()
                .HasOne(sp => sp.Post)
                .WithMany()
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<SavedPostModel>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageModel>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m =>  m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<MessageModel>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m =>  m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
