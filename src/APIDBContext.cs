using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

using KeyMan.Models;

namespace KeyMan
{
    public partial class APIKeyDBContext : DbContext
    {
        public APIKeyDBContext()
        {
        }

        public APIKeyDBContext(DbContextOptions<APIKeyDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApiKeyModel> ApiKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(
                $"Host={Environment.GetEnvironmentVariable("HOSTNAME")};Database={Environment.GetEnvironmentVariable("DATABASE")};Username={Environment.GetEnvironmentVariable("USERNAME")};Password={Environment.GetEnvironmentVariable("PASSWORD")}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiKeyModel>(entity =>
            {
                entity.HasKey(e => e.Key).HasName("apikeys_pkey");

                entity.ToTable("apikeys");

                entity.Property(e => e.Key)
                    .HasMaxLength(88)
                    .IsFixedLength()
                    .HasColumnName("key");
                entity.Property(e => e.Creationtime)
                    .HasMaxLength(22)
                    .HasColumnName("creationtime");
                entity.Property(e => e.Expirytime)
                    .HasMaxLength(22)
                    .HasColumnName("expirytime");
                entity.Property(e => e.Islimitless).HasColumnName("islimitless");
                entity.Property(e => e.Permissions)
                    .HasMaxLength(1024)
                    .HasColumnName("permissions");
                entity.Property(e => e.Userid)
                    .HasMaxLength(64)
                    .IsFixedLength()
                    .HasColumnName("userid");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}