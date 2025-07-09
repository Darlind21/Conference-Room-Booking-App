using Conference_Room_Booking_App.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Conference_Room_Booking_App.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        //public DbSet<Room> Rooms { get; set; }
        //public DbSet<Booking> Bookings { get; set; }
        //public DbSet<ReservationHolder> ReservationHolders { get; set; }
        //public DbSet<AppUser> AppUsers { get; set; }
        //public DbSet<UnavailabilityPeriod> UnavailabilityPeriods { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    builder.Entity<Room>()
        //        .HasIndex(u => u.RoomCode)
        //        .IsUnique();

        //    builder.Entity<Room>()
        //        .HasIndex(u => u.Name)
        //        .IsUnique();

        //    builder.Entity<Room>()
        //        .HasMany(r => r.Bookings)
        //        .WithOne(b => b.Room)
        //        .OnDelete(DeleteBehavior.NoAction);

        //    builder.Entity<Room>()
        //        .HasMany(r => r.UnavailabilityPeriods)
        //        .WithOne(up => up.Room)
        //        .OnDelete(DeleteBehavior.Cascade);




        //    builder.Entity<Booking>()
        //        .HasOne(b => b.Room)
        //        .WithMany(r => r.Bookings);

        //    builder.Entity<Booking>()
        //        .HasOne(b => b.ReservationHolder)
        //        .WithOne(rh => rh.Booking)
        //        .OnDelete(DeleteBehavior.NoAction);




        //    builder.Entity<UnavailabilityPeriod>()
        //        .HasOne(up => up.Room)
        //        .WithMany(r => r.UnavailabilityPeriods)
        //        .OnDelete(DeleteBehavior.NoAction);



        //    builder.Entity<ReservationHolder>()
        //        .HasOne(rh => rh.Booking)
        //        .WithOne(b => b.ReservationHolder)
        //        .OnDelete(DeleteBehavior.NoAction);
        //}
    }
}
