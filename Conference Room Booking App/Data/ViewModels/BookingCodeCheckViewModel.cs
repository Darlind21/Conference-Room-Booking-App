using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class BookingCodeCheckViewModel
    {
        [Required]
        [Display(Name = "Booking Code")]
        public string BookingCode { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
