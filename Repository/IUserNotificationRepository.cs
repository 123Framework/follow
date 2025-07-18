﻿using TweeterApp.Models;

namespace TweeterApp.Repository
{
    public interface IUserNotificationRepository
    {
        Task<IEnumerable<NotificationModel>> GetUserNotificationAsync(int userId);
        Task AddNotificationAsync(NotificationModel notification);
        Task MarkAsReadAsync(int notificationId);
    }
}
