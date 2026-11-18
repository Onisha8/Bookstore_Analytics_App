using BookApp.Models;

namespace BookApp.Services;

public interface IAnalyticsService
{
    LibraryAnalytics GetForUser(int userId);
}
