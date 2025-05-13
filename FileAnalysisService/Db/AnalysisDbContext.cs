namespace FileAnalisysService;
using Microsoft.EntityFrameworkCore;
public class AnalysisDbContext : DbContext
{
    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options) { }

    public DbSet<AnalyzedFile> Files => Set<AnalyzedFile>();
}