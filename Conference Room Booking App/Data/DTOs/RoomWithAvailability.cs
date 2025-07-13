namespace Conference_Room_Booking_App.Data.DTOs
{
    public class RoomWithAvailability
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public string? PhotoUrl { get; set; }
        public List<string> AvailableTimeSlots { get; set; } = new();
    }
}
