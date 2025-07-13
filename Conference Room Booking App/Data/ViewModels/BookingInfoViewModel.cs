using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class BookingInfoViewModel
    {
        [Required]
        [Display(Name = "Attendees Count")]
        public int AttendeesCount { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
