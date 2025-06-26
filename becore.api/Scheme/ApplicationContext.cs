using becore.api.Scheme.Packs;
using becore.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using File = becore.api.Scheme.System.File;

namespace becore.api.Scheme;

public class ApplicationContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<AddonPack> AddonPacks { get; set; }
    public DbSet<DataPack> DataPacks { get; set; }
    public DbSet<ResourcesPack> ResourcesPacks { get; set; }
    public DbSet<ScriptPack> ScriptPacks { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<PageTag> PageTags { get; set; }
    public DbSet<File> Files { get; set; }
    // Identity Users are managed by IdentityDbContext

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Page>()
            .HasMany(p => p.PageTags)
            .WithOne(pt => pt.Page)
            .HasForeignKey(pt => pt.PageId);

        modelBuilder.Entity<File>(file =>
        {
            file.Navigation(x => x.User).AutoInclude();
        });
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}