using Microsoft.EntityFrameworkCore;
using Haven.Models;

namespace Haven.Data;

public class HavenDbContext : DbContext
{
    public HavenDbContext(DbContextOptions<HavenDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ChildProfile> ChildProfiles => Set<ChildProfile>();
    public DbSet<ProfessionalProfile> ProfessionalProfiles => Set<ProfessionalProfile>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> CourseModules => Set<CourseModule>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<SupportSession> SupportSessions => Set<SupportSession>();
    public DbSet<EmergencyResource> EmergencyResources => Set<EmergencyResource>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CrisisAlert> CrisisAlerts => Set<CrisisAlert>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ArticleBookmark> ArticleBookmarks => Set<ArticleBookmark>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<CommunityComment> CommunityComments => Set<CommunityComment>();
    public DbSet<PostReport> PostReports => Set<PostReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Identity & Access
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("User");
            entity.Property(u => u.UserType).HasMaxLength(50).HasDefaultValue("Individual");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<ChildProfile>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.ParentUser)
                  .WithMany()
                  .HasForeignKey(c => c.ParentUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfessionalProfile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Content & Learning
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.Author)
                  .WithMany()
                  .HasForeignKey(c => c.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CourseModule>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasOne(m => m.Course)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(m => m.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointments & Care
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Professional)
                  .WithMany()
                  .HasForeignKey(a => a.ProfessionalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Author)
                  .WithMany()
                  .HasForeignKey(a => a.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CrisisAlert>(entity =>
        {
            entity.HasKey(ca => ca.Id);
            entity.HasOne(ca => ca.User)
                  .WithMany()
                  .HasForeignKey(ca => ca.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.HasOne(log => log.AdminUser)
                  .WithMany()
                  .HasForeignKey(log => log.AdminUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
