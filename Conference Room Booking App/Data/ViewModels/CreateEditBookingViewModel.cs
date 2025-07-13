namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class CreateEditBookingViewModel
    {
        public int? BookingId { get; set; }
        public string? BookingCode { get; set; }
        public bool IsEdit { get; set; }

        // Room Details (Read-only)
        public RoomDetailViewModel RoomDetail { get; set; } = new();

        // Booking Info
        public BookingInfoViewModel BookingInfo { get; set; } = new();

        // Reservation Holder Details
        public ReservationHolderViewModel ReservationHolder { get; set; } = new();
    }
}
