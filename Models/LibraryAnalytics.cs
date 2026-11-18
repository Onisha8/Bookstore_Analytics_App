using System.Collections.Generic;

namespace BookApp.Models;

/// <summary>Everything the Analytics page shows, computed server-side.</summary>
public class LibraryAnalytics
{
    public int TotalBooks { get; init; }
    public double AverageRating { get; init; }
    public double LikedPercent { get; init; }
    public int GenreCount { get; init; }
    public int AuthorCount { get; init; }

    /// <summary>Average rating of liked vs. not-liked books (null when a group is empty).</summary>
    public double? AvgRatingLiked { get; init; }
    public double? AvgRatingNotLiked { get; init; }

    public IReadOnlyList<GenreStat> Genres { get; init; } = new List<GenreStat>();
    public IReadOnlyList<GenreStat> TopRatedGenres { get; init; } = new List<GenreStat>();
    public IReadOnlyList<AuthorStat> TopAuthors { get; init; } = new List<AuthorStat>();

    /// <summary>Index 0 = 1 star ... index 4 = 5 stars. Always 5 entries.</summary>
    public IReadOnlyList<int> RatingDistribution { get; init; } = new int[5];

    public bool HasData => TotalBooks > 0;
}

public record GenreStat(string Genre, int Count, double AverageRating, double LikedPercent);

public record AuthorStat(string Author, int Count, double AverageRating);
