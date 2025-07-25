using Conference_Room_Booking_App.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class MyBookingsViewModel
    {
        [Display(Name = "Minimum Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Maximum Date")]
        public DateTime? EndDate { get; set; }


        [Display(Name = "Minimum Attendees")]
        public int? MinAttendees { get; set; }

        [Display(Name = "Maximum Attendees")]
        public int? MaxAttendees { get; set; }

        [Display(Name = "Status")]
        public BookingStatus? Status { get; set; }

        public PaginatedBookingsViewModel Bookings { get; set; } = new();
    }
}
