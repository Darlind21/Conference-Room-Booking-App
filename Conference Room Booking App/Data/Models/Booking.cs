using Conference_Room_Booking_App.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conference_Room_Booking_App.Data.Models
{
    public class Booking 
    {
        public int Id { get; set; }
        public required string BookingCode { get; set; } = null!;
        public required int AttendeesCount { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending; //When a new booking is created its status goes automatically to pending 
        public bool IsDeleted { get; set; } = false;
        public string? Notes { get; set; }


        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; }
        public required Room Room { get; set; }


        [ForeignKey(nameof(ReservationHolder))]
        public int ReservationHolderId { get; set; }
        public required ReservationHolder ReservationHolder { get; set; }


        [ForeignKey(nameof(AppUser))]
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
