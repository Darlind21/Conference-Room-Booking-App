using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.DTOs;
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

        public async Task<Booking?> CreateBookingAsync
            (int roomId, BookingInfoViewModel bookingInfo,
            ReservationHolderViewModel reservationHolder,
            string? appUserId = null)
        {
            try
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.IsActive);

                if (room == null) return null;
                
                if (bookingInfo.AttendeesCount > room.MaxCapacity || bookingInfo.AttendeesCount < 5) return null;

                if (bookingInfo.StartTime >= bookingInfo.EndTime || bookingInfo.StartTime <= DateTime.Now) return null;

                if (bookingInfo.EndTime > DateTime.Now.AddDays(60)) return null;

                var duration = (bookingInfo.EndTime - bookingInfo.StartTime).TotalMinutes;
                if (duration < 30 || duration > 240) return null;

                if (!IsQuarterHourInterval(bookingInfo.StartTime, bookingInfo.EndTime)) return null;

                var isAvailable = await IsRoomAvailableAsync(roomId, bookingInfo.StartTime, bookingInfo.EndTime);
                if (!isAvailable) return null;



                ReservationHolder newReservationHolder;

                if (!string.IsNullOrEmpty(appUserId)) //if user is authenticated
                {
                    var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUserId);
                    if (appUser == null) throw new ArgumentException("User was not found when submitting booking form");

                    if(reservationHolder.IdCardNumber == appUser.IdCardNumber) //if user uses his idcardnumber
                    {
                        // we get the reservationHolder entity associated with the user so we avoid duplicate rows for same logged in user
                        var userReservationHolder = await _context.ReservationHolders 
                            .Where(rh => rh.IdCardNumber == appUser.IdCardNumber)
                            .FirstOrDefaultAsync();

                        if (userReservationHolder != null) 
                        {
                            newReservationHolder = userReservationHolder;
                        }
                        else //if auth user has never done a booking before 
                        {
                            newReservationHolder = await CreateReservationHolderAsync(reservationHolder);
                        }
                    }
                    else //if user doesnt use his idcardnumber we create new reservation holder row
                    {
                        newReservationHolder = await CreateReservationHolderAsync(reservationHolder);
                    }


                }
                else //else if user is not authenticated we create new reservation holder row
                {
                    newReservationHolder = await CreateReservationHolderAsync(reservationHolder);
                }

                var bookingCode = await GenerateUniqueBookingCodeAsync();

                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    AttendeesCount = bookingInfo.AttendeesCount,
                    StartTime = bookingInfo.StartTime,
                    EndTime = bookingInfo.EndTime,
                    Notes = bookingInfo.Notes,
                    RoomId = roomId,
                    Room = room,
                    ReservationHolder = newReservationHolder,
                    Status = BookingStatus.Pending,
                    AppUserId = appUserId
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return await GetBookingByCodeAsync(bookingCode);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Booking?> UpdateBookingAsync
            (int bookingId, BookingInfoViewModel bookingInfo,
            ReservationHolderViewModel reservationHolder, string? appUserId = null) //REVIEW:
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.ReservationHolder)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                //if booking is cancelled or rejected it cannot be updated
                if (booking == null || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected) return null;

                if (booking.Room == null || booking.ReservationHolder == null) return null;

                if (bookingInfo.AttendeesCount > booking.Room.MaxCapacity || bookingInfo.AttendeesCount < 5) return null;

                if (bookingInfo.StartTime >= bookingInfo.EndTime || bookingInfo.StartTime <= DateTime.Now) return null;

                if (bookingInfo.EndTime > DateTime.Now.AddDays(60)) return null;

                var duration = (bookingInfo.EndTime - bookingInfo.StartTime).TotalMinutes;
                if (duration < 30 || duration > 240) return null;

                if (!IsQuarterHourInterval(bookingInfo.StartTime, bookingInfo.EndTime)) return null;

                // Check room availability (excluding current booking)
                var isAvailable = await IsRoomAvailableAsync(booking.RoomId, bookingInfo.StartTime, bookingInfo.EndTime, bookingId);
                if (!isAvailable) return null;



                if (!string.IsNullOrEmpty(appUserId)) //if user is authenticated
                {
                    var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == appUserId) ?? throw new ArgumentException("User was not found when updating booking form");

                    if (reservationHolder.IdCardNumber == appUser.IdCardNumber) //if user uses his idcardnumber
                    {
                        // we get the reservationHolder entity associated with the user so we avoid duplicate rows for same logged in user
                        var userReservationHolder = await _context.ReservationHolders
                            .Where(rh => rh.IdCardNumber == appUser.IdCardNumber)
                            .FirstOrDefaultAsync();

                        if (userReservationHolder != null) //if auth user has done a booking before
                        {
                            booking.ReservationHolder = userReservationHolder;
                            booking.AppUserId = appUserId;
                        }
                        else //if auth user has never done a booking before
                        { 
                            booking.ReservationHolder = BuildReservationHolder(reservationHolder);
                            booking.AppUserId = appUserId;
                        }
                    }
                    else //if user doesnt use his idcardnumber we create new reservation holder row and disassociate the booking from his appuserId
                    {
                        booking.ReservationHolder = BuildReservationHolder(reservationHolder);
                        booking.AppUserId = null;
                    }


                }
                else //else if user is not authenticated we still create new reservation holder row
                {
                    booking.ReservationHolder = BuildReservationHolder(reservationHolder);
                }

                // Update the booking fields
                booking.AttendeesCount = bookingInfo.AttendeesCount;
                booking.StartTime = bookingInfo.StartTime;
                booking.EndTime = bookingInfo.EndTime;
                booking.Notes = bookingInfo.Notes;
                booking.Status = BookingStatus.Pending;

                _context.Bookings.Update(booking);

                await _context.SaveChangesAsync();

                return await GetBookingByCodeAsync(booking.BookingCode);
            }
            catch
            {
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

        private async Task<string> GenerateUniqueBookingCodeAsync()
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

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private ReservationHolder BuildReservationHolder(ReservationHolderViewModel reservationHolder)
        {
            var holder = new ReservationHolder
            {
                FirstName = reservationHolder.FirstName,
                LastName = reservationHolder.LastName,
                Email = reservationHolder.Email,
                IdCardNumber = reservationHolder.IdCardNumber,
                PhoneNumber = reservationHolder.PhoneNumber
            };

            return holder;
        }

        private async Task<ReservationHolder> CreateReservationHolderAsync(ReservationHolderViewModel reservationHolder)
        {
            var holder = BuildReservationHolder(reservationHolder);

            await _context.ReservationHolders.AddAsync(holder);
            await _context.SaveChangesAsync();

            return holder;
        }

        private bool IsQuarterHourInterval(DateTime startTime, DateTime endTime)
        {
            // Check if the start and end times are in quarter-hour intervals
            return (startTime.Minute % 15 == 0 && endTime.Minute % 15 == 0);
        }

        public async Task<bool> IsIdCardNumberValid(ReservationHolderViewModel reservationHolder, string? userId)
        {
            if (!string.IsNullOrEmpty(userId)) //if user is authenticated 
            {
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new ArgumentException("Invalid userId provided.");

                // if user has not provided the same IdCardNumber as in his account
                if (appUser.IdCardNumber != reservationHolder.IdCardNumber)
                {
                    //we check if the IdCardNumber belongs to another user
                    var belongsToAnotherUser = await _context.Users
                        .AnyAsync(u => u.IdCardNumber == reservationHolder.IdCardNumber && u.Id != userId);

                    if (belongsToAnotherUser) return false;

                    return true; // If it does not belong to another user, we assume it's valid for a new reservation holder
                }
                // if user has provided the same IdCardNumber as in his account
                else
                {
                    // We can assume the IdCardNumber is valid since it matches the authenticated user's IdCardNumber
                    return true;
                }
            }
            else //if user is not authenticated
            {
                //we check if the IdCardNumber number belongs to a registered app user
                var belongsToAnotherUser = await _context.Users
                    .AnyAsync(u => u.IdCardNumber == reservationHolder.IdCardNumber);

                if (belongsToAnotherUser) return false; // If it belongs to another user, we return false
                    
                // If it does not belong to any user, we assume it's valid for a new reservation holder
                return true;
                
            }
        }

        public string GetStatusColor(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "warning",
                BookingStatus.Confirmed => "success",
                BookingStatus.Rejected => "danger",
                BookingStatus.Cancelled => "secondary",
                _ => "secondary"
            };
        }

        public Task<List<Booking>> GetBookingsForUserAsync(int roomId) //TODO: Implement
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResult<BookingStatusViewModel>> GetFilteredBookingsAsync
            (string appUserId, DateTime? minDate, DateTime? maxDate,
            int? minAttendees, int? maxAttendees, BookingStatus? status,
            int page, int itemsPerPage)
        {
            if (string.IsNullOrEmpty(appUserId)) throw new ArgumentException("AppUserId not provided when filtering bookings");

            var query = _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.AppUserId == appUserId && b.IsDeleted == false);
                

            if (minDate.HasValue) query = query.Where(b => b.StartTime.Date >= minDate.Value.Date);

            if (maxDate.HasValue) query = query.Where(b => b.EndTime.Date <= maxDate.Value.Date);

            if (minAttendees != null) query = query.Where(b => b.AttendeesCount >= minAttendees);

            if (maxAttendees != null) query = query.Where(b => b.AttendeesCount <= maxAttendees);

            if (status != null) query = query.Where(b => b.Status == status);

            var bookings = await query
                .OrderByDescending(b => b.EndTime)
                .ToListAsync();

            //pagination params
            var totalItems = bookings.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            var paginatedBookings = bookings
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            var bookingStatusList= new List<BookingStatusViewModel>();

            foreach( var booking in paginatedBookings)
            {
                var newBookingStatus = new BookingStatusViewModel
                {
                        BookingCode = booking.BookingCode,
                        Status = booking.Status.ToString(),
                        StatusColor = GetStatusColor(booking.Status),
                        RoomName = booking.Room.Name,
                        RoomCode = booking.Room.RoomCode,
                        AttendeesCount = booking.AttendeesCount,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Notes = booking.Notes,
                        CanEdit = booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed,
                        CanCancel = booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed


                };

                if (booking.EndTime < DateTime.Now || booking.StartTime < DateTime.Now.AddMinutes(30))
                {
                    newBookingStatus.CanCancel = false; //can only cancel or edit no less then 30 mins before 
                    newBookingStatus.CanEdit = false;
                }

                bookingStatusList.Add(newBookingStatus);
            }

            return new PaginatedResult<BookingStatusViewModel>
            {
                Items = bookingStatusList,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage
            };
        }
    }
}
