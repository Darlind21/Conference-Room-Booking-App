using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.ViewModels.ReservationHolder;
using Microsoft.AspNetCore.Mvc;

namespace Conference_Room_Booking_App.Data.ViewModels.Booking
{
    public class BookingDetailVM
    {
        [HiddenInput]
        public int Id { get; set; }
        public string BookingCode { get; set; } = null!;
        public int AttendeesCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; } 
        public string? Notes { get; set; }
    }
}
