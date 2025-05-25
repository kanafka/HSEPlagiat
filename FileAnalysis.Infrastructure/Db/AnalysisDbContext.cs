namespace FileAnalisysService;
using Microsoft.EntityFrameworkCore;
public class AnalysisDbContext : DbContext
{
    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options) { }

    public DbSet<AnalyzedFile> Files => Set<AnalyzedFile>();
    public DbSet<WordCloudImage> WordCloudImages => Set<WordCloudImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalyzedFile>()
            .OwnsOne(f => f.WordAnalysis);

        modelBuilder.Entity<WordCloudImage>()
            .Property(w => w.Location)
            .IsRequired();
    }
}