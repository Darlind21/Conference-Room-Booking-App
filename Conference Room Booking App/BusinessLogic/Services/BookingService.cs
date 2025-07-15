using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.Models;

using Conference_Room_Booking_App.Data.ViewModels;

//using Conference_Room_Booking_App.Data.ViewModels.Booking;
using Microsoft.EntityFrameworkCore;

namespace Conference_Room_Booking_App.BusinessLogic.Services
{
    public class BookingService(ApplicationDbContext _context) : IBookingService
    {
        public async Task<Booking?> GetBookingByCodeAsync(string bookingCode)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.ReservationHolder)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode && !b.IsDeleted);
        }

        public async Task<Booking?> CreateBookingAsync(int roomId, BookingInfoViewModel bookingInfo, ReservationHolderViewModel reservationHolder) //REVIEW:
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate room exists and is active
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.IsActive);
                if (room == null)
                {
                    return null;
                }

                // Validate room capacity
                if (bookingInfo.AttendeesCount > room.MaxCapacity)
                {
                    return null;
                }

                // Validate time slots
                if (bookingInfo.StartTime >= bookingInfo.EndTime || bookingInfo.StartTime <= DateTime.Now)
                {
                    return null;
                }

                // Check room availability
                var isAvailable = await IsRoomAvailableAsync(roomId, bookingInfo.StartTime, bookingInfo.EndTime);
                if (!isAvailable)
                {
                    return null;
                }

                // Create or get existing reservation holder
                var existingHolder = await _context.ReservationHolders
                    .FirstOrDefaultAsync(rh => rh.IdCardNumber == reservationHolder.IdCardNumber);

                ReservationHolder holder;
                if (existingHolder != null)
                {
                    // Update existing holder's information
                    existingHolder.FirstName = reservationHolder.FirstName;
                    existingHolder.LastName = reservationHolder.LastName;
                    existingHolder.Email = reservationHolder.Email;
                    existingHolder.PhoneNumber = reservationHolder.PhoneNumber;
                    holder = existingHolder;
                }
                else
                {
                    // Create new reservation holder
                    holder = new ReservationHolder
                    {
                        FirstName = reservationHolder.FirstName,
                        LastName = reservationHolder.LastName,
                        Email = reservationHolder.Email,
                        IdCardNumber = reservationHolder.IdCardNumber,
                        PhoneNumber = reservationHolder.PhoneNumber
                    };
                    _context.ReservationHolders.Add(holder);
                }

                // Generate unique booking code
                var bookingCode = await GenerateUniqueBookingCodeAsync();

                // Create booking
                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    AttendeesCount = bookingInfo.AttendeesCount,
                    StartTime = bookingInfo.StartTime,
                    EndTime = bookingInfo.EndTime,
                    Notes = bookingInfo.Notes,
                    RoomId = roomId,
                    Room = room,
                    ReservationHolder = holder,
                    Status = BookingStatus.Pending
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Update the holder's BookingId
                holder.BookingId = booking.Id;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return booking with all related data
                return await GetBookingByCodeAsync(bookingCode);
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<Booking?> UpdateBookingAsync(int bookingId, BookingInfoViewModel bookingInfo, ReservationHolderViewModel reservationHolder) //REVIEW:
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.ReservationHolder)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null || booking.Status != BookingStatus.Pending)
                {
                    return null;
                }

                // Validate room capacity
                if (bookingInfo.AttendeesCount > booking.Room.MaxCapacity)
                {
                    return null;
                }

                // Validate time slots
                if (bookingInfo.StartTime >= bookingInfo.EndTime || bookingInfo.StartTime <= DateTime.Now)
                {
                    return null;
                }

                // Check room availability (excluding current booking)
                var isAvailable = await IsRoomAvailableAsync(booking.RoomId, bookingInfo.StartTime, bookingInfo.EndTime, bookingId);
                if (!isAvailable)
                {
                    return null;
                }

                // Update booking information
                booking.AttendeesCount = bookingInfo.AttendeesCount;
                booking.StartTime = bookingInfo.StartTime;
                booking.EndTime = bookingInfo.EndTime;
                booking.Notes = bookingInfo.Notes;

                // Update reservation holder information
                booking.ReservationHolder.FirstName = reservationHolder.FirstName;
                booking.ReservationHolder.LastName = reservationHolder.LastName;
                booking.ReservationHolder.Email = reservationHolder.Email;
                booking.ReservationHolder.PhoneNumber = reservationHolder.PhoneNumber;

                // Check if ID card number changed
                if (booking.ReservationHolder.IdCardNumber != reservationHolder.IdCardNumber)
                {
                    // Check if new ID card number already exists
                    var existingHolder = await _context.ReservationHolders
                        .FirstOrDefaultAsync(rh => rh.IdCardNumber == reservationHolder.IdCardNumber && rh.Id != booking.ReservationHolder.Id);

                    if (existingHolder != null)
                    {
                        // ID card number already exists for another holder
                        return null;
                    }

                    booking.ReservationHolder.IdCardNumber = reservationHolder.IdCardNumber;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetBookingByCodeAsync(booking.BookingCode);
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<bool> CancelBookingAsync(string bookingCode) //REVIEW:
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode && !b.IsDeleted);

            if (booking == null)
            {
                return false;
            }

            // Only allow cancellation for pending or confirmed bookings
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                return false;
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Booking>> GetBookingsByRoomIdAsync(int roomId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.ReservationHolder)
                .Where(b => b.RoomId == roomId && !b.IsDeleted)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null) //REVIEW:
        {
            // Check for overlapping bookings
            var overlappingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                            !b.IsDeleted &&
                            b.Status == BookingStatus.Confirmed && //booking must be confirmed to be considered overlapping
                            b.EndTime > startTime && // if another booking that ends before the desired start time exists
                            b.StartTime < endTime) // and also if that same booking starts before desired end time it is considered overlapping 
                .ToListAsync();

            // Exclude the current booking if updating
            if (excludeBookingId.HasValue)
            {
                overlappingBookings = overlappingBookings.Where(b => b.Id != excludeBookingId.Value).ToList();
            }

            if (overlappingBookings.Any())
            {
                return false;
            }

            // Check for unavailability periods
            var unavailabilityPeriods = await _context.UnavailabilityPeriods
                .Where(up => up.RoomId == roomId &&
                            up.IsActive &&
                            ((up.StartTime < endTime && up.EndTime > startTime)))
                .ToListAsync();

            return !unavailabilityPeriods.Any();
        }

        private async Task<string> GenerateUniqueBookingCodeAsync() //TODO: FIX METHOD
        {
            string bookingCode;
            bool exists;

            do
            {
                // Generate a 8-character alphanumeric code
                bookingCode = GenerateRandomCode(8);
                exists = await _context.Bookings.AnyAsync(b => b.BookingCode == bookingCode);
            } while (exists);

            return bookingCode;
        }

        private string GenerateRandomCode(int length) //TODO: fIX METHOD
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
