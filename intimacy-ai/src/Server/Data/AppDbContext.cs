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
        public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
        public DbSet<AnalysisHistory> AnalysisHistories { get; set; } = null!;
        public DbSet<CoachingSession> CoachingSessions { get; set; } = null!;

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

            modelBuilder.Entity<UserPreferences>(entity =>
            {
                entity.ToTable("user_preferences");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PreferencesJsonEncrypted).HasColumnName("preferences_json");
                entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at");
                entity.HasIndex(e => e.UserId).IsUnique(false);
            });

            modelBuilder.Entity<AnalysisHistory>(entity =>
            {
                entity.ToTable("analysis_history");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).HasColumnName("session_id");
                entity.Property(e => e.AnalysisType).HasColumnName("analysis_type");
                entity.Property(e => e.ScoresJsonEncrypted).HasColumnName("scores_json");
                entity.Property(e => e.MetadataJsonEncrypted).HasColumnName("metadata_json");
                entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at");
            });

            modelBuilder.Entity<CoachingSession>(entity =>
            {
                entity.ToTable("coaching_sessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).HasColumnName("session_id");
                entity.Property(e => e.SuggestionsJsonEncrypted).HasColumnName("suggestions_json");
                entity.Property(e => e.FeedbackJsonEncrypted).HasColumnName("feedback_json");
                entity.Property(e => e.CreatedAtUtc).HasColumnName("created_at");
            });
        }
    }
}

