namespace File_Storing_Service;

using Microsoft.EntityFrameworkCore;

public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) {}

    public DbSet<FileMetadata> Files => Set<FileMetadata>();
}
