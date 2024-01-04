using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BBBSBackend.DBModels;

public partial class BbbsContext : DbContext
{
    public BbbsContext()
    {
    }

    public BbbsContext(DbContextOptions<BbbsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdditionalService> AdditionalServices { get; set; }

    public virtual DbSet<AdditionalServiceForBooking> AdditionalServiceForBookings { get; set; }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<FlywaySchemaHistory> FlywaySchemaHistories { get; set; }

    public virtual DbSet<LanguageStringTable> LanguageStringTables { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomForBooking> RoomForBookings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdditionalService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("AdditionalService");

            entity.HasIndex(e => e.Description, "idx_description");

            entity.HasIndex(e => e.ThumbnailImagePath, "idx_thumpnailPath");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2048)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PricePerUnit)
                .HasColumnType("double(10,2)")
                .HasColumnName("pricePerUnit");
            entity.Property(e => e.ThumbnailImagePath)
                .HasMaxLength(2048)
                .HasColumnName("thumbnailImagePath");
        });

        modelBuilder.Entity<AdditionalServiceForBooking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("AdditionalServiceForBooking");

            entity.HasIndex(e => e.AdditionalServiceId, "additionalServiceId");

            entity.HasIndex(e => e.BookingId, "bookingId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalServiceId).HasColumnName("additionalServiceId");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");

            entity.HasOne(d => d.AdditionalService).WithMany(p => p.AdditionalServiceForBookings)
                .HasForeignKey(d => d.AdditionalServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdditionalServiceForBooking_ibfk_2");

            entity.HasOne(d => d.Booking).WithMany(p => p.AdditionalServiceForBookings)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AdditionalServiceForBooking_ibfk_1");
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("AdminUser");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.PasswordSalt)
                .HasMaxLength(255)
                .HasColumnName("passwordSalt");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Booking");

            entity.HasIndex(e => e.CustomerId, "customerId");

            entity.HasIndex(e => e.Comment, "idx_comment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("'uuid()'")
                .HasColumnName("id");
            entity.Property(e => e.ArrivalDate)
                .HasColumnType("date")
                .HasColumnName("arrivalDate");
            entity.Property(e => e.Comment)
                .HasMaxLength(10240)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.DepartureDate)
                .HasColumnType("date")
                .HasColumnName("departureDate");
            entity.Property(e => e.NumberOfPeople).HasColumnName("numberOfPeople");
            entity.Property(e => e.Paid).HasColumnName("paid");
            entity.Property(e => e.ReservationTimeOut).HasColumnName("reservationTimeOut");
            entity.Property(e => e.Canceled).HasColumnName("canceled");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Booking_ibfk_1");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.FirstName, "idx_firstName");

            entity.HasIndex(e => e.LastName, "idx_lastName");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(1024)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(1024)
                .HasColumnName("lastName");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .HasColumnName("postalCode");
            entity.Property(e => e.ReceiveNewsletter).HasColumnName("receiveNewsletter");
        });

        modelBuilder.Entity<FlywaySchemaHistory>(entity =>
        {
            entity.HasKey(e => e.InstalledRank).HasName("PRIMARY");

            entity.ToTable("flyway_schema_history");

            entity.HasIndex(e => e.Success, "flyway_schema_history_s_idx");

            entity.Property(e => e.InstalledRank).HasColumnName("installed_rank");
            entity.Property(e => e.Checksum).HasColumnName("checksum");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.ExecutionTime).HasColumnName("execution_time");
            entity.Property(e => e.InstalledBy)
                .HasMaxLength(100)
                .HasColumnName("installed_by");
            entity.Property(e => e.InstalledOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("installed_on");
            entity.Property(e => e.Script)
                .HasMaxLength(1000)
                .HasColumnName("script");
            entity.Property(e => e.Success).HasColumnName("success");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.Version)
                .HasMaxLength(50)
                .HasColumnName("version");
        });

        modelBuilder.Entity<LanguageStringTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("LanguageStringTable");

            entity.HasIndex(e => e.DataString, "idx_dataString");

            entity.HasIndex(e => e.UniqueIdentifier, "uniqueIdentifier").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DataString)
                .HasMaxLength(2048)
                .HasColumnName("dataString");
            entity.Property(e => e.LangCode)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("langCode");
            entity.Property(e => e.UniqueIdentifier).HasColumnName("uniqueIdentifier");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Room");

            entity.HasIndex(e => e.Description, "idx_description");

            entity.HasIndex(e => e.ThumbnailImagePath, "idx_thumpnailPath");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Description)
                .HasMaxLength(2048)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PricePerNight)
                .HasColumnType("double(10,2)")
                .HasColumnName("pricePerNight");
            entity.Property(e => e.ThumbnailImagePath)
                .HasMaxLength(2048)
                .HasColumnName("thumbnailImagePath");
        });

        modelBuilder.Entity<RoomForBooking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("RoomForBooking");

            entity.HasIndex(e => e.BookingId, "bookingId");

            entity.HasIndex(e => e.RoomId, "roomId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.RoomId).HasColumnName("roomId");

            entity.HasOne(d => d.Booking).WithMany(p => p.RoomForBookings)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RoomForBooking_ibfk_1");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomForBookings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RoomForBooking_ibfk_2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
