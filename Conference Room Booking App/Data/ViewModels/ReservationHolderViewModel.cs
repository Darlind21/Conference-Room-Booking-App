using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class ReservationHolderViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ID Card Number")]
        public string IdCardNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        public int PhoneNumber { get; set; }
    }
}
