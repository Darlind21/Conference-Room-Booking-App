using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conference_Room_Booking_App.Data.Models
{
    public class UnavailabilityPeriod
    {
        public int Id { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Required]
        public required DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(200)]
        public string? Reason { get; set; }

        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;   
    }
}
