using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conference_Room_Booking_App.Data.Models
{
    [Index (nameof(IdCardNumber), IsUnique = true)]
    public class ReservationHolder
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string IdCardNumber { get; set; }
        public required int PhoneNumber { get; set; }


        [ForeignKey(nameof(Booking))]
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; } = null!;
    }
}
