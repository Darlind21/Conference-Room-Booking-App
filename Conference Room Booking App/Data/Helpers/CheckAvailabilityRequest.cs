namespace Conference_Room_Booking_App.Data.Helpers
{
    // Request helper class for the CheckAvailability endpoint
    public class CheckAvailabilityRequest
    {
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? ExcludeBookingId { get; set; }
    }
}
