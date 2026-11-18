using System;
using System.Collections.Generic;
using System.Linq;
using BookApp.Data;
using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Services;

/// <summary>
/// Computes reading analytics with LINQ. Each query is an IQueryable that EF Core
/// translates to SQL (WHERE / GROUP BY / COUNT / AVG / ORDER BY / LIMIT), so the
/// aggregation runs in the database rather than in the browser.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    /// <summary>Genres need at least this many books to be ranked by average rating.</summary>
    public const int MinBooksForGenreRanking = 3;

    private readonly BookLyticsDbContext _db;

    public AnalyticsService(BookLyticsDbContext db) => _db = db;

    public LibraryAnalytics GetForUser(int userId)
    {
        var books = _db.Books.AsNoTracking().Where(b => b.OwnerId == userId);

        var total = books.Count();
        if (total == 0)
            return new LibraryAnalytics();

        // One aggregate row: SELECT COUNT(*), AVG(Rating), SUM(Liked) ... GROUP BY constant
        var summary = books
            .GroupBy(_ => 1)
            .Select(g => new
            {
                AvgRating = g.Average(b => (double)b.Rating),
                Liked = g.Count(b => b.Liked),
                AvgLiked = g.Where(b => b.Liked).Average(b => (double?)b.Rating),
                AvgNotLiked = g.Where(b => !b.Liked).Average(b => (double?)b.Rating)
            })
            .Single();

        // Per-genre breakdown: GROUP BY Genre
        var genres = books
            .GroupBy(b => b.Genre)
            .Select(g => new
            {
                Genre = g.Key,
                Count = g.Count(),
                AvgRating = g.Average(b => (double)b.Rating),
                Liked = g.Count(b => b.Liked)
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Genre)
            .AsEnumerable()
            .Select(g => new GenreStat(g.Genre, g.Count, Round(g.AvgRating), Percent(g.Liked, g.Count)))
            .ToList();

        // Highest-rated genres, ignoring genres with too few books to be meaningful
        var topRatedGenres = genres
            .Where(g => g.Count >= MinBooksForGenreRanking)
            .OrderByDescending(g => g.AverageRating)
            .ThenByDescending(g => g.Count)
            .Take(5)
            .ToList();

        // Top 5 authors: GROUP BY Author ORDER BY COUNT(*) DESC LIMIT 5
        var topAuthors = books
            .GroupBy(b => b.Author)
            .Select(g => new
            {
                Author = g.Key,
                Count = g.Count(),
                AvgRating = g.Average(b => (double)b.Rating)
            })
            .OrderByDescending(a => a.Count)
            .ThenByDescending(a => a.AvgRating)
            .ThenBy(a => a.Author)
            .Take(5)
            .AsEnumerable()
            .Select(a => new AuthorStat(a.Author, a.Count, Round(a.AvgRating)))
            .ToList();

        // Rating distribution: GROUP BY Rating, then fill missing star values with 0
        var ratingCounts = books
            .GroupBy(b => b.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionary(r => r.Rating, r => r.Count);
        var distribution = Enumerable.Range(1, 5)
            .Select(star => ratingCounts.GetValueOrDefault(star))
            .ToArray();

        return new LibraryAnalytics
        {
            TotalBooks = total,
            AverageRating = Round(summary.AvgRating),
            LikedPercent = Percent(summary.Liked, total),
            GenreCount = genres.Count,
            AuthorCount = books.Select(b => b.Author).Distinct().Count(),
            AvgRatingLiked = summary.AvgLiked is double l ? Round(l) : null,
            AvgRatingNotLiked = summary.AvgNotLiked is double n ? Round(n) : null,
            Genres = genres,
            TopRatedGenres = topRatedGenres,
            TopAuthors = topAuthors,
            RatingDistribution = distribution
        };
    }

    private static double Round(double value) => Math.Round(value, 2);

    private static double Percent(int part, int whole) =>
        whole == 0 ? 0 : Math.Round(100.0 * part / whole, 1);
}
