using becore.api.Scheme.Packs;
using becore.api.Models;
using becore.api.Scheme.Workgroup;
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
    public DbSet<AdditionPage> AddonPages { get; set; }
    public DbSet<ResourcePage> ResourcePages { get; set; }
    public DbSet<BehaviourPage> BehaviourPages { get; set; }
    public DbSet<ResourceFile> ResourceFiles { get; set; }
    public DbSet<BehaviourFile> BehaviourFiles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<ContentMaker> ContentMakers { get; set; }

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

        modelBuilder.Entity<ResourceFile>(file =>
        {
            file.HasKey(x => new { x.Id, x.ResourcePageId });
            file.HasOne(x => x.ResourcePage)
                .WithMany(page => page.Files)
                .HasForeignKey(x => x.ResourcePageId);
        });
        
        modelBuilder.Entity<BehaviourFile>(file =>
        {
            file.HasKey(x => new { x.Id, x.BehaviourPageId });
            file.HasOne(x => x.BehaviourPage)
                .WithMany(page => page.Files)
                .HasForeignKey(x => x.BehaviourPageId);
        });

        modelBuilder.Entity<ContentMaker>(maker =>
        {
            maker.HasOne(x => x.User)
                .WithOne(user => user.ContentMaker)
                .HasForeignKey<ContentMaker>(x => x.UserId);
        });

        modelBuilder.Entity<Team>(team =>
        {
            team.HasMany(x => x.Members)
                .WithOne(member => member.Team)
                .HasForeignKey(x => x.TeamId);
        });

        modelBuilder.Entity<TeamMember>(member =>
        {
            member.HasKey(x => x.Id);
            member.HasOne(x => x.ContentMaker)
                .WithOne(maker => maker.TeamMember)
                .HasForeignKey<TeamMember>(x => x.Id);
        });

        modelBuilder.Entity<AdditionPage>(page =>
        {
            page.HasOne(x => x.Team)
                .WithMany(team => team.Addons)
                .HasForeignKey(x => x.TeamId);
            page.HasOne(x => x.Owner)
                .WithMany(maker => maker.Addons)
                .HasForeignKey(x => x.OwnerId);
        });

        modelBuilder.Entity<BehaviourPage>(page =>
        {
            page.HasKey(x => x.Id);
            page.HasOne(x => x.AdditionPage)
                .WithOne(addon => addon.BehaviourPage)
                .HasForeignKey<BehaviourPage>(x => x.Id);
        });

        modelBuilder.Entity<ResourcePage>(page =>
        {
            page.HasKey(x => x.Id);
            page.HasOne(x => x.AdditionPage)
                .WithOne(addon => addon.ResourcePage)
                .HasForeignKey<ResourcePage>(x => x.Id);
        });
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
}