using Microsoft.EntityFrameworkCore;
using VideoProcessingApp.Models;

namespace VideoProcessingApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Bu satır, PostgreSQL'de "AnalysisResults" adında bir tablo oluştur demek.
    public DbSet<AnalysisResult> AnalysisResults { get; set; }
}