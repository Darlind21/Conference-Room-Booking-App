namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class BookingStatusViewModel
    {
        public string BookingCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public int AttendeesCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanCancel { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
