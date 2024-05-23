using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Infrastructures.DatabaseContext
{
    public partial class ThinkTankContext : DbContext
    {
        public ThinkTankContext()
        {
        }

        public ThinkTankContext(DbContextOptions<ThinkTankContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<AccountIn1vs1> AccountIn1vs1s { get; set; } = null!;
        public virtual DbSet<AccountInContest> AccountInContests { get; set; } = null!;
        public virtual DbSet<AccountInRoom> AccountInRooms { get; set; } = null!;
        public virtual DbSet<Achievement> Achievements { get; set; } = null!;
        public virtual DbSet<Asset> Assets { get; set; } = null!;
        public virtual DbSet<AssetOfContest> AssetOfContests { get; set; } = null!;
        public virtual DbSet<Badge> Badges { get; set; } = null!;
        public virtual DbSet<Challenge> Challenges { get; set; } = null!;
        public virtual DbSet<Contest> Contests { get; set; } = null!;
        public virtual DbSet<Friend> Friends { get; set; } = null!;
        public virtual DbSet<Game> Games { get; set; } = null!;
        public virtual DbSet<Icon> Icons { get; set; } = null!;
        public virtual DbSet<IconOfAccount> IconOfAccounts { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<Topic> Topics { get; set; } = null!;
        public virtual DbSet<TypeOfAsset> TypeOfAssets { get; set; } = null!;
        public virtual DbSet<TypeOfAssetInContest> TypeOfAssetInContests { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }
        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var strConn = config["ConnectionStrings:DefaultSQLConnection"];
            return strConn;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasMaxLength(254)
                    .IsUnicode(false);

                entity.Property(e => e.Fcm).HasColumnName("FCM");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.Gender)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.GoogleId).IsUnicode(false);

                entity.Property(e => e.RefreshToken).IsUnicode(false);

                entity.Property(e => e.RegistrationDate).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Version)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<AccountIn1vs1>(entity =>
            {
                entity.ToTable("AccountIn1vs1");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.RoomOfAccountIn1vs1Id)
                   .HasMaxLength(8)
                   .IsUnicode(false);
                entity.HasOne(d => d.AccountId1Navigation)
                    .WithMany(p => p.AccountIn1vs1AccountId1Navigations)
                    .HasForeignKey(d => d.AccountId1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__Accou__208CD6FA");

                entity.HasOne(d => d.AccountId2Navigation)
                    .WithMany(p => p.AccountIn1vs1AccountId2Navigations)
                    .HasForeignKey(d => d.AccountId2)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__Accou__2180FB33");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.AccountIn1vs1s)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__GameI__278EDA44");
            });

            modelBuilder.Entity<AccountInContest>(entity =>
            {
                entity.ToTable("AccountInContest");

                entity.Property(e => e.CompletedTime).HasColumnType("datetime");

                entity.Property(e => e.Duration).HasColumnType("decimal(18, 1)");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountInContests)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__Accou__0D7A0286");

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.AccountInContests)
                    .HasForeignKey(d => d.ContestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__Conte__0E6E26BF");
            });

            modelBuilder.Entity<AccountInRoom>(entity =>
            {
                entity.ToTable("AccountInRoom");

                entity.Property(e => e.CompletedTime).HasColumnType("datetime");

                entity.Property(e => e.Duration).HasColumnType("decimal(18, 1)");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountInRooms)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__Accou__09A971A2");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.AccountInRooms)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__AccountIn__RoomI__0A9D95DB");
            });

            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.ToTable("Achievement");

                entity.Property(e => e.CompletedTime).HasColumnType("datetime");

                entity.Property(e => e.Duration).HasColumnType("decimal(18, 1)");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Achievements)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Achieveme__Accou__01142BA1");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Achievements)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Achieveme__GameI__7E8CC4B1");
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Asset");

                entity.Property(e => e.Value).IsUnicode(false);

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK__Asset__TopicId__7EF6D905");

                entity.HasOne(d => d.TypeOfAsset)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.TypeOfAssetId)
                    .HasConstraintName("FK__Asset__TypeOfAss__7FEAFD3E");
            });

            modelBuilder.Entity<AssetOfContest>(entity =>
            {
                entity.ToTable("AssetOfContest");

                entity.Property(e => e.Value).IsUnicode(false);

                entity.HasOne(d => d.Contest)
                    .WithMany(p => p.AssetOfContests)
                    .HasForeignKey(d => d.ContestId)
                    .HasConstraintName("FK__AssetOfCo__Conte__7B264821");

                entity.HasOne(d => d.TypeOfAsset)
                    .WithMany(p => p.AssetOfContests)
                    .HasForeignKey(d => d.TypeOfAssetId)
                    .HasConstraintName("FK__AssetOfCo__TypeO__7C1A6C5A");
            });

            modelBuilder.Entity<Badge>(entity =>
            {
                entity.ToTable("Badge");

                entity.Property(e => e.CompletedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Badges)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Badge__AccountId__60A75C0F");

                entity.HasOne(d => d.Challenge)
                    .WithMany(p => p.Badges)
                    .HasForeignKey(d => d.ChallengeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Badge__Challenge__619B8048");
            });

            modelBuilder.Entity<Challenge>(entity =>
            {
                entity.ToTable("Challenge");

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.MissionsImg).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Unit).HasMaxLength(50);
            });

            modelBuilder.Entity<Contest>(entity =>
            {
                entity.ToTable("Contest");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.PlayTime).HasColumnType("decimal(18, 1)");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.Thumbnail).IsUnicode(false);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Contests)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK__Contest__GameId__51EF2864");
            });


            modelBuilder.Entity<Friend>(entity =>
            {
                entity.ToTable("Friend");

                entity.HasOne(d => d.AccountId1Navigation)
                    .WithMany(p => p.FriendAccountId1Navigations)
                    .HasForeignKey(d => d.AccountId1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friend__AccountI__1CBC4616");

                entity.HasOne(d => d.AccountId2Navigation)
                    .WithMany(p => p.FriendAccountId2Navigations)
                    .HasForeignKey(d => d.AccountId2)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friend__AccountI__1DB06A4F");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game");

                entity.Property(e => e.Name).HasMaxLength(50);
            });


            modelBuilder.Entity<Icon>(entity =>
            {
                entity.ToTable("Icon");

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<IconOfAccount>(entity =>
            {
                entity.ToTable("IconOfAccount");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.IconOfAccounts)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__IconOfAcc__Accou__66603565");

                entity.HasOne(d => d.Icon)
                    .WithMany(p => p.IconOfAccounts)
                    .HasForeignKey(d => d.IconId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__IconOfAcc__IconI__6754599E");
            });


            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.DateNotification).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Title).HasMaxLength(300);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Notificat__Accou__2B0A656D");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Report");

                entity.Property(e => e.DateReport).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Title).HasMaxLength(200);

                entity.HasOne(d => d.AccountId1Navigation)
                    .WithMany(p => p.ReportAccountId1Navigations)
                    .HasForeignKey(d => d.AccountId1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Report__AccountI__607251E5");

                entity.HasOne(d => d.AccountId2Navigation)
                    .WithMany(p => p.ReportAccountId2Navigations)
                    .HasForeignKey(d => d.AccountId2)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Report__AccountI__6166761E");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");

                entity.Property(e => e.Code)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Room__TopicId__05D8E0BE");
            });


            modelBuilder.Entity<Topic>(entity =>
            {
                entity.ToTable("Topic");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Topics)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("FK__Topic__GameId__73852659");
            });

            modelBuilder.Entity<TypeOfAsset>(entity =>
            {
                entity.ToTable("TypeOfAsset");

                entity.Property(e => e.Type)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TypeOfAssetInContest>(entity =>
            {
                entity.ToTable("TypeOfAssetInContest");

                entity.Property(e => e.Type)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
