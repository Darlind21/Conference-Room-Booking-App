namespace Conference_Room_Booking_App.Data.Models
{
    public class AppUser //only one user must exist and that must be the admin 
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
