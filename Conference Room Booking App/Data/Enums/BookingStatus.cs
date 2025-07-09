using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.Enums
{
    public enum BookingStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Confirmed")]
        Confirmed = 2,

        [Display(Name = "Rejected")]
        Rejected = 3,

        [Display(Name = "Cancelled")]
        Cancelled = 4
    }
}
