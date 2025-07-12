using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.ViewModels.Booking;
using Microsoft.EntityFrameworkCore;

namespace Conference_Room_Booking_App.BusinessLogic.Services
{
    public class BookingService(ApplicationDbContext context) : IBookingService
    {
        public async Task<List<BookingDetailVM>> GetBookingsPerRoomAsync(int roomId)
        {
            var bookings = await context.Bookings
                .Where(x => x.RoomId == roomId)
                .Include(x => x.ReservationHolder)
                .ToListAsync();

            var bookingDetailVMList = new List<BookingDetailVM>();

            foreach (var booking in bookings)
            {
                var bookingVM = new BookingDetailVM
                {
                    Id = booking.Id,
                    BookingCode = booking.BookingCode,
                    AttendeesCount = booking.AttendeesCount,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Status = booking.Status,
                    Notes = booking.Notes
                };

                bookingDetailVMList.Add(bookingVM);
            }

            return bookingDetailVMList;
        }
    }
}
