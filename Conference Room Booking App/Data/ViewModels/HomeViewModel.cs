using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class HomeViewModel
    {
        [Display(Name = "Booking Code")]
        public string? BookingCode { get; set; }

        [Display(Name = "Start Time")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        [Display(Name = "Attendees Count")]
        public int? AttendeesCount { get; set; }

        public PaginatedRoomsViewModel Rooms { get; set; } = new();
    }
}
