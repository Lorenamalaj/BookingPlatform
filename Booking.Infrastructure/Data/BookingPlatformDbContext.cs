using Microsoft.EntityFrameworkCore;
using Booking.Domain.Users;
using Booking.Domain.Roles;
using Booking.Domain.UserRoles;
using Booking.Domain.OwnerProfiles;
using Booking.Domain.Addresses;
using Booking.Domain.Reviews;
using BookingEntity = Booking.Domain.Bookings.Booking;
using PropertyEntity = Booking.Domain.Properties.Property;

namespace Booking.Infrastructure.Data
{
    public class BookingPlatformDbContext : DbContext
    {
        public BookingPlatformDbContext(DbContextOptions<BookingPlatformDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<OwnerProfile> OwnerProfiles { get; set; }
        public DbSet<PropertyEntity> Properties { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<BookingEntity> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()")
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.ProfileImageUrl)
                    .HasMaxLength(500);

                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // Role Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.Description)
                    .HasMaxLength(255);

                entity.HasIndex(r => r.Name)
                    .IsUnique();

                // Seed default roles
                entity.HasData(
                    new
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "Guest",
                        Description = "Regular guest user who can book properties"
                    },
                    new
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "Owner",
                        Description = "Property owner who can list properties"
                    },
                    new
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "Admin",
                        Description = "Administrator with full system access"
                    }
                );
            });

            // UserRole Configuration (Many-to-Many)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Role>()
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OwnerProfile Configuration
            modelBuilder.Entity<OwnerProfile>(entity =>
            {
                entity.ToTable("OwnerProfiles");
                entity.HasKey(op => op.UserId);

                entity.Property(op => op.IdentityCardNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(op => op.VerificationStatus)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(op => op.BusinessName)
                    .HasMaxLength(255);

                entity.Property(op => op.CreditCard)
                    .HasMaxLength(100);

                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<OwnerProfile>(op => op.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Address Configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Addresses");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(a => a.PostalCode)
                    .HasMaxLength(20);
            });

            // Property Configuration
            modelBuilder.Entity<PropertyEntity>(entity =>
            {
                entity.ToTable("Properties");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(p => p.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(p => p.PropertyType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Address>()
                    .WithMany()
                    .HasForeignKey(p => p.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Booking Configuration
            modelBuilder.Entity<BookingEntity>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(b => b.CleaningFee)
                    .HasColumnType("decimal(18,2)");

                entity.Property(b => b.AmenitiesUpCharge)
                    .HasColumnType("decimal(18,2)");

                entity.Property(b => b.PriceForPeriod)
                    .HasColumnType("decimal(18,2)");

                entity.Property(b => b.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(b => b.BookingStatus)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne<PropertyEntity>()
                    .WithMany()
                    .HasForeignKey(b => b.PropertyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(b => b.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

         entity.HasOne(b => b.Property)
        .WithMany()
        .HasForeignKey(b => b.PropertyId)
        .OnDelete(DeleteBehavior.Restrict);
            });

            // Review Configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.Comment)
                    .HasMaxLength(1000);

                entity.HasOne<BookingEntity>()
                    .WithMany()
                    .HasForeignKey(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(r => r.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}