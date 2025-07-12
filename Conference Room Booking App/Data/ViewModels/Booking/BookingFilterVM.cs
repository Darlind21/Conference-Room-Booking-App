using Conference_Room_Booking_App.Data.Enums;

namespace Conference_Room_Booking_App.Data.ViewModels.Booking
{
    public class BookingFilterVM
    {
        public int? AttendeesCountMin { get; set; }
        public int? AttendeesCountMax { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public BookingStatus BookingStatus { get; set; }
    }
}
