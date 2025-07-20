using Microsoft.AspNetCore.Identity;

namespace Conference_Room_Booking_App.Data.Models
{
    public class AppUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
