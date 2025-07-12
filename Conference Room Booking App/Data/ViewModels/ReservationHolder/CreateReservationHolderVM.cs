using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels.ReservationHolder
{
    public class CreateReservationHolderVM
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string IdCardNumber { get; set; } = null!;

        [Required]
        [Phone]
        public int PhoneNumber { get; set; }
    }
}
