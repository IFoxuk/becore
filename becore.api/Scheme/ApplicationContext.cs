using becore.api.Scheme.Packs;
using Microsoft.EntityFrameworkCore;

namespace becore.api.Scheme;

public class ApplicationContext : DbContext
{
    public DbSet<AddonPack> AddonPacks { get; set; }
    public DbSet<DataPack> DataPacks { get; set; }
    public DbSet<ResourcesPack> ResourcesPacks { get; set; }
    public DbSet<ScriptPack> ScriptPacks { get; set; }
    public DbSet<Page> Pages { get; set; }
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}