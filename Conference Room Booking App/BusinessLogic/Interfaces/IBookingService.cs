//using Conference_Room_Booking_App.Data.ViewModels.Booking;

using Conference_Room_Booking_App.Data.DTOs;
using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels;

namespace Conference_Room_Booking_App.BusinessLogic.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> GetBookingByCodeAsync(string bookingCode);
        Task<Booking?> CreateBookingAsync(int roomId, BookingInfoViewModel bookingInfo, ReservationHolderViewModel reservationHolder, string? appUserId = null);
        Task<Booking?> UpdateBookingAsync(int bookingId, BookingInfoViewModel bookingInfo, ReservationHolderViewModel reservationHolder, string? appUserId = null);
        Task<bool> CancelBookingAsync(string bookingCode);
        Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
        Task<bool> IsIdCardNumberValid(ReservationHolderViewModel reservationHolder, string? userId);
        Task<List<Booking>> GetBookingsForUserAsync(int roomId);
        string GetStatusColor(BookingStatus status);
        Task <PaginatedResult<BookingStatusViewModel>>GetFilteredBookingsAsync(string appUserId, DateTime? minDate, DateTime? maxDate, int? minAttendees, int? maxAttendees, BookingStatus? status, int page, int itemsPerPage);
    }
}
