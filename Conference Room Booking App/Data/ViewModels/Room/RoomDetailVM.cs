using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels.Booking;
using Conference_Room_Booking_App.Data.ViewModels.UnavailabilityPeriod;
using Microsoft.AspNetCore.Mvc;

namespace Conference_Room_Booking_App.Data.ViewModels.Room
{
    public class RoomDetailVM
    {
        [HiddenInput]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public int MaxCapacity { get; set; }

    }
}
