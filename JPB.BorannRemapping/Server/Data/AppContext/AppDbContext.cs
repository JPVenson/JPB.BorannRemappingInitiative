using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<DeviceCodes> DeviceCodes { get; set; }
        public virtual DbSet<FrontierOauthToken> FrontierOauthToken { get; set; }
        public virtual DbSet<PersistedGrants> PersistedGrants { get; set; }
        public virtual DbSet<System> System { get; set; }
        public virtual DbSet<SystemBody> SystemBody { get; set; }
        public virtual DbSet<SystemBodyRing> SystemBodyRing { get; set; }
        public virtual DbSet<SystemEval> SystemEval { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\V17;Database=JPB.BorannRemapping.Database;Trusted_Connection=True;MultipleActiveResultSets=true", x => x.UseNetTopologySuite());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");
            });

            modelBuilder.Entity<DeviceCodes>(entity =>
            {
                entity.HasIndex(e => e.DeviceCode)
                    .IsUnique();

                entity.HasIndex(e => e.Expiration);
            });

            modelBuilder.Entity<FrontierOauthToken>(entity =>
            {
                entity.HasKey(e => e.FrontierId)
                    .HasName("PK_FrontierId");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.FrontierOauthToken)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FrontierOAuthToken_AspNetUsers");
            });

            modelBuilder.Entity<PersistedGrants>(entity =>
            {
                entity.HasIndex(e => e.Expiration);

                entity.HasIndex(e => new { e.SubjectId, e.ClientId, e.Type });
            });

            modelBuilder.Entity<SystemBody>(entity =>
            {
                entity.HasOne(d => d.IdSystemNavigation)
                    .WithMany(p => p.SystemBody)
                    .HasForeignKey(d => d.IdSystem)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemBody_System");
            });

            modelBuilder.Entity<SystemBodyRing>(entity =>
            {
                entity.HasOne(d => d.IdSystemBodyNavigation)
                    .WithMany(p => p.SystemBodyRing)
                    .HasForeignKey(d => d.IdSystemBody)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemBodyRing_SystemBody");
            });

            modelBuilder.Entity<SystemEval>(entity =>
            {
                entity.HasKey(e => e.SubmissionId)
                    .HasName("PK_SystemEvalId");

                entity.Property(e => e.IsReviewed).HasDefaultValueSql("(CONVERT([bit],(0)))");

                entity.HasOne(d => d.IdSubmittingUserNavigation)
                    .WithMany(p => p.SystemEval)
                    .HasForeignKey(d => d.IdSubmittingUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemEval_AspNetUsers");

                entity.HasOne(d => d.IdSystemBodyNavigation)
                    .WithMany(p => p.SystemEval)
                    .HasForeignKey(d => d.IdSystemBody)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemEval_SystemBody");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
