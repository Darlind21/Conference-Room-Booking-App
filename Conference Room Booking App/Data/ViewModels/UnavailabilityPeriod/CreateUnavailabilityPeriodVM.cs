using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels.UnavailabilityPeriod
{
    public class CreateUnavailabilityPeriodVM
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [MaxLength(200)]
        public string? Reason { get; set; }

    }
}
