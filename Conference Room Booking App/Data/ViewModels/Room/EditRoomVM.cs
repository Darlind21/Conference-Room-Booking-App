using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Conference_Room_Booking_App.Data.ViewModels.Room
{
    public class EditRoomVM
    {
        [HiddenInput] 
        public int Id { get; set; }

        [MinLength(5)] [MaxLength(100)]
        public string? Name { get; set; }

        [Range(5, 100)]
        public int? MaxCapacity { get; set; }
    }
}
