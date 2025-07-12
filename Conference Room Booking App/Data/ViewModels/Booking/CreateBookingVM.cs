using Conference_Room_Booking_App.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels.Booking
{
    public class CreateBookingVM
    {
        [HiddenInput]
        public int RoomdId { get; set; }

        [Required]
        [Range(1, 100)]
        public int AttendeesCount { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
