using Conference_Room_Booking_App.Data.ViewModels.Booking;

namespace Conference_Room_Booking_App.BusinessLogic.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDetailVM>> GetBookingsPerRoomAsync(int id);
    }
}
