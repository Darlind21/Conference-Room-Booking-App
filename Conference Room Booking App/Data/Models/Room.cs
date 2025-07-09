using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.Models
{
    //[Index(nameof(RoomCode), IsUnique = true)] -- can be used instead of fluent api 
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        public required string RoomCode { get; set; }

        [Required]
        [Range(1,100)]
        public required int MaxCapacity { get; set; }
        public bool IsActive { get; set; } = true; //by default when a room is created it is active
        public List<Booking> Bookings { get; set; } = [];
        public List<UnavailabilityPeriod> UnavailabilityPeriods { get; set; } = [];
    }
}
