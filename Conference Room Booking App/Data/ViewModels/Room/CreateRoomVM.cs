using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels.Room
{
    public class CreateRoomVM
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public string RoomCode { get; set; } = null!;

        [Required]
        [Range(5, 100)]
        public int MaxCapacity { get; set; }
    }
}
