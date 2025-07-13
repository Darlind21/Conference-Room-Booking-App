using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.DTOs;
using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.Models;
//using Conference_Room_Booking_App.Data.ViewModels.Booking;
//using Conference_Room_Booking_App.Data.ViewModels.Room;
using Microsoft.EntityFrameworkCore;

namespace Conference_Room_Booking_App.BusinessLogic.Services
{
    public class RoomService(ApplicationDbContext _context) : IRoomService
    {
        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Bookings)
                .Include(r => r.UnavailabilityPeriods)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<PaginatedResult<RoomWithAvailability>> GetFilteredRoomsAsync(
            DateTime? startTime,
            DateTime? endTime,
            DateTime? date,
            int? attendeesCount,
            int page,
            int itemsPerPage)
        {
            var query = _context.Rooms.Where(r => r.IsActive);

            // Apply capacity filter
            if (attendeesCount.HasValue)
            {
                query = query.Where(r => r.MaxCapacity >= attendeesCount.Value);
            }

            // Get all rooms first
            var rooms = await query.ToListAsync();

            // Apply availability filter if time parameters are provided
            if (startTime.HasValue && endTime.HasValue)
            {
                var availableRooms = new List<Room>();

                foreach (var room in rooms)
                {
                    var requestStartTime = startTime.Value;
                    var requestEndTime = endTime.Value;

                    // If date is specified, combine it with the times
                    if (date.HasValue)
                    {
                        requestStartTime = date.Value.Date.Add(requestStartTime.TimeOfDay);
                        requestEndTime = date.Value.Date.Add(requestEndTime.TimeOfDay);
                    }

                    var isAvailable = await IsRoomAvailableAsync(room.Id, requestStartTime, requestEndTime);
                    if (isAvailable)
                    {
                        availableRooms.Add(room);
                    }
                }

                rooms = availableRooms;
            }

            // Apply pagination
            var totalItems = rooms.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            var paginatedRooms = rooms
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            // Convert to RoomWithAvailability and get time slots
            var roomsWithAvailability = new List<RoomWithAvailability>();
            foreach (var room in paginatedRooms)
            {
                var availableSlots = await GetAvailableTimeSlotsAsync(room.Id);
                roomsWithAvailability.Add(new RoomWithAvailability
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomCode = room.RoomCode,
                    MaxCapacity = room.MaxCapacity,
                    PhotoUrl = GetRoomPhotoUrl(room.Id), // You can implement this method
                    AvailableTimeSlots = availableSlots
                });
            }

            return new PaginatedResult<RoomWithAvailability>
            {
                Items = roomsWithAvailability,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage
            };
        }

        public async Task<List<string>> GetAvailableTimeSlotsAsync(int roomId)
        {
            var availableSlots = new List<string>();
            var today = DateTime.Today;
            var currentTime = DateTime.Now;

            // Generate time slots for the next 7 days
            for (int day = 0; day < 7; day++)
            {
                var checkDate = today.AddDays(day);

                // Generate hourly slots from 8 AM to 6 PM
                for (int hour = 8; hour <= 18; hour++)
                {
                    var slotStart = checkDate.AddHours(hour);
                    var slotEnd = slotStart.AddHours(1);

                    // Skip past time slots
                    if (slotStart <= currentTime)
                        continue;

                    // Check if this slot is available
                    var isAvailable = await IsRoomAvailableAsync(roomId, slotStart, slotEnd);
                    if (isAvailable)
                    {
                        var timeSlot = $"{slotStart:MMM dd} {slotStart:HH:mm}-{slotEnd:HH:mm}";
                        availableSlots.Add(timeSlot);
                    }

                    // Limit to 10 slots to avoid UI clutter
                    if (availableSlots.Count >= 10)
                        break;
                }

                if (availableSlots.Count >= 10)
                    break;
            }

            return availableSlots;
        }

        public async Task<List<Room>> GetAllActiveRoomsAsync()
        {
            return await _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        private async Task<bool> IsRoomAvailableAsync(int roomId, DateTime startTime, DateTime endTime)
        {
            // Check for overlapping bookings
            var overlappingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                           !b.IsDeleted &&
                           (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                           ((b.StartTime < endTime && b.EndTime > startTime)))
                .AnyAsync();

            if (overlappingBookings)
            {
                return false;
            }

            // Check for unavailability periods
            var unavailabilityPeriods = await _context.UnavailabilityPeriods
                .Where(up => up.RoomId == roomId &&
                            up.IsActive &&
                            ((up.StartTime < endTime && up.EndTime > startTime)))
                .AnyAsync();

            return !unavailabilityPeriods;
        }

        private string? GetRoomPhotoUrl(int roomId)
        {
            // You can implement this method to return actual photo URLs
            // For now, return null or a default placeholder
            // Example: return $"/images/rooms/room_{roomId}.jpg";
            return null;
        }
    }

//    public class RoomService(ApplicationDbContext context) : IRoomService
//    {
//        public async Task<RoomDetailVM> CreateRoomAsync(CreateRoomVM payload)
//        {
//            var roomCodeExists = context.Rooms
//                .Any(r => r.RoomCode == payload.RoomCode);

//            if (roomCodeExists) throw new Exception("A room with this code already exists. Room code must be unique!");

//            var room = new Room
//            {
//                Name = payload.Name,
//                RoomCode = payload.RoomCode,
//                MaxCapacity = payload.MaxCapacity
//            };

//            context.Rooms.Add(room);
//            await context.SaveChangesAsync();

//            var createdRoom = new RoomDetailVM
//            {
//                Id = room.Id,
//                Name = room.Name,
//                RoomCode = room.RoomCode,
//                MaxCapacity = room.MaxCapacity
//            };

//            return createdRoom;
//        }

//        public async Task<bool> DeactivateRoomAsync(int id)
//        {
//            var room = await context.Rooms
//                .Where(x => x.Id == id)
//                .Include(x => x.Bookings)
//                .SingleOrDefaultAsync();

//            if (room == null) throw new Exception("Room to be deactivated with this id does not exist");

//            bool hasFutureOrOngoingConfirmedBookings = room.Bookings
//                .Any(x => x.Status == Data.Enums.BookingStatus.Confirmed &&
//                     x.EndTime > DateTime.UtcNow);

//                if (hasFutureOrOngoingConfirmedBookings)
//                {
//                    throw new Exception("Cannot delete room since it has confirmed bookings in the future");
//                }

//            room.IsActive = false;

//            return await context.SaveChangesAsync() > 1;
//        }

//        public async Task<bool> EditRoomAsync(EditRoomVM payload)
//        {
//            var room = await context.Rooms.FindAsync(payload.Id);
//            //have not used Include() since the user might not want to change the room capacity
//            //will only get bookings per room if user wants to change maxcapacity

//            if (room == null) throw new Exception("Room to be edited with this id does not exist");

//            if (payload.Name != null) room.Name = payload.Name;

//            if (payload.MaxCapacity != null)
//            {
//                var bookingsForRoomQuery = context.Bookings
//                    .Where(b => b.Id == room.Id);

//                var belowConfirmedBookingsCapacity = await bookingsForRoomQuery
//                    .AnyAsync(b => b.Status == Data.Enums.BookingStatus.Confirmed && b.AttendeesCount < payload.MaxCapacity);

//                if (belowConfirmedBookingsCapacity) throw new Exception
//                        ("Unable to edit this rooms maxcapacity because there are conflicting confirmed bookings that need more capacity. Please cancel bookings first!");

//                room.MaxCapacity = payload.MaxCapacity.Value;

//                var pendingBookings = await bookingsForRoomQuery //we cancel pending bookings that exceeds the new max capacity
//                    .Where(b => b.Status == Data.Enums.BookingStatus.Pending 
//                            && b.AttendeesCount < payload.MaxCapacity)
//                    .ToListAsync();
                
//                    foreach(var booking in pendingBookings)
//                    {
//                        booking.Status = Data.Enums.BookingStatus.Cancelled;
//                    }
//            }

//            return await context.SaveChangesAsync() > 0;

//        }

//        public Task<List<RoomDetailVM>> FilterRoomsAsync(RoomFilterVM filter)
//        {
//            //var query = context.Rooms
//            //    .Include(x => x.Bookings)
//            //    .Include(r => r.UnavailabilityPeriods)
//            //    .AsQueryable();

//            //if (filter.Name != null)
//            //{
//            //    query = query
//            //        .Where(x => x.Name.Contains(filter.Name));
//            //}

//            //if (filter.MinCapacity != null)
//            //{
//            //    query = query
//            //        .Where(x => x.MaxCapacity >= filter.MinCapacity);
//            //}

//            //if (filter.MaxCapacity != null)
//            //{
//            //    query = query
//            //        .Where(x => x.MaxCapacity <= filter.MaxCapacity);
//            //}

//            //var filteredRooms = await query.ToListAsync();

//            //var roomListVM = new RoomListVM();

//            //foreach (var room in filteredRooms)
//            //{
//            //    var bookingDetailVMs = bookingService.GetBookingsPerRoom(room.Id);

//            //    var roomDetailVM = new RoomDetailVM
//            //    {
//            //        Id = room.Id,
//            //        Name = room.Name,
//            //        RoomCode = room.RoomCode,
//            //        MaxCapacity = room.MaxCapacity,

//            //        Bookings = bookingDetailVMs
//            //    };

//            //    roomListVM.RoomDetailVMs.Add(room);
//            //}

//            throw new NotImplementedException();
//        }

//        public async Task<List<RoomDetailVM>> GetAllActiveRoomsAsync()
//        {
//            var activeRooms = await context.Rooms
//                .Where(r => r.IsActive == true)
//                .ToListAsync();

//            var roomDetailVMList = new List<RoomDetailVM>();

//            if (activeRooms.Any())
//            {
//                foreach (var room in activeRooms)
//                {
//                    var bookings = await GetAllBookingsForRoomAsync(room.Id);

//                    var roomDetailVM = new RoomDetailVM
//                    {
//                        Id = room.Id,
//                        Name = room.Name,
//                        RoomCode = room.RoomCode,
//                        MaxCapacity = room.MaxCapacity
//                    };

//                    roomDetailVMList.Add(roomDetailVM);
//                }
//            }

//            return roomDetailVMList;
//        }

//        public async Task<List<AvailableTimeslotVM>> GetAvailableTimeslotsAsync(int roomId, DateOnly date)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<RoomDetailVM> GetRoomByIdAsync(int roomId)
//        {
//            var room = await context.Rooms.FindAsync(roomId);

//            if (room == null) throw new Exception("Room to get with this id does not exist");
//        }
//    }
}
