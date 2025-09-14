using Microsoft.EntityFrameworkCore;
using IntimacyAI.Server.Models;

namespace IntimacyAI.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UsageAnalytics> UsageAnalytics { get; set; } = null!;
        public DbSet<ModelPerformance> ModelPerformances { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsageAnalytics>(entity =>
            {
                entity.ToTable("usage_analytics");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnonymousUserId).HasColumnName("anonymous_user_id");
                entity.Property(e => e.FeatureUsed).HasColumnName("feature_used");
                entity.Property(e => e.UsageDurationSeconds).HasColumnName("usage_duration");
                entity.Property(e => e.Platform).HasColumnName("platform");
                entity.Property(e => e.AppVersion).HasColumnName("app_version");
                entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at");
            });

            modelBuilder.Entity<ModelPerformance>(entity =>
            {
                entity.ToTable("model_performance");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ModelVersion).HasColumnName("model_version");
                entity.Property(e => e.AccuracyMetricsJson).HasColumnName("accuracy_metrics");
                entity.Property(e => e.PerformanceMetricsJson).HasColumnName("performance_metrics");
                entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at");
            });
        }
    }
}

