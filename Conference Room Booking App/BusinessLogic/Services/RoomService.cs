using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.DTOs;
using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels;

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

        public async Task<PaginatedResult<RoomCardViewModel>> GetFilteredRoomsAsync(
            DateTime? startTime,
            DateTime? endTime,
            DateTime? date,
            int? attendeesCount,
            int page,
            int itemsPerPage)
        {
            var query = _context.Rooms.Where(r => r.IsActive);

            //If user filters via max capacity
            if (attendeesCount.HasValue)
            {
                query = query.Where(r => r.MaxCapacity >= attendeesCount.Value);
            }

            var rooms = await query.ToListAsync();

            // Apply availability filter if time parameters are provided
            if (startTime.HasValue && endTime.HasValue)
            {
                var availableRooms = new List<Room>();

                foreach (var room in rooms)
                {
                    var requestStartTime = startTime.Value;
                    var requestEndTime = endTime.Value;

                    // If date is specified it iscombined with the times
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

            //Pagination params
            var totalItems = rooms.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            var paginatedRooms = rooms
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            var roomsWithAvailability = new List<RoomCardViewModel>();
            foreach (var room in paginatedRooms) //Mapping Room to RoomWithAvailability and get avb time slots
            {
                var availableSlots = await GetAvailableTimeSlotsAsync(room.Id);
                roomsWithAvailability.Add(new RoomCardViewModel
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomCode = room.RoomCode,
                    MaxCapacity = room.MaxCapacity,
                    PhotoUrl = GetRoomPhotoUrl(room.Id),
                    AvailableTimeSlots = availableSlots
                });
            }

            return new PaginatedResult<RoomCardViewModel>
            {
                Items = roomsWithAvailability,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage
            };
        }

        public async Task<List<string>> GetAvailableTimeSlotsAsync(int roomId) //FEATURE: Method works but might want to add feature so if a timeslot is unavb,
                                                                               //the next avb time slot starts 15 mins from the end of the unavb one
        {
            var availableSlots = new List<string>();
            var today = DateTime.Today; //midnight of current day (00:00)
            var now = DateTime.Now;

            //REVIEW: Might want to change the rooms operation hours
            //We assume that rooms operate between 8:00 - 23:00 and the can be booked only within quarter hour intervals -- 12:00 or 12:15 or 12:30 or 12:45
            //Also minimum duration must be 30 mins and max 4 hours
            var startHour = 8;
            var endHour = 23;
            var minDuration = TimeSpan.FromMinutes(30);
            var maxDuration = TimeSpan.FromHours(4);
            var interval = TimeSpan.FromMinutes(15);

            for (int day = 0; day < 60; day++) //we get availability for at max 60 days in advance
            {
                var checkDate = today.AddDays(day);

                for (var startTime = checkDate.AddHours(startHour);
                     startTime < checkDate.AddHours(endHour);
                     startTime = startTime.Add(interval)) //loop that checks every possible starting time for a booking for a single day within working hours with 15 mins intervals
                {
                    // Skips past times -- loop breaks until start time is later than 1 hour from now
                    if (startTime <= now.AddHours(1)) //timeslot is considered avb only if its at least 1h from now since you can only make a booking at least 1h in advance
                        continue;

                    // Loop below tries each timeslot staring by max duration(4h) and decreasing by 15mins intervals if that timeslot is unavb starting from 8:00 
                    for (var duration = maxDuration; duration >= minDuration; duration = duration - interval)
                    {
                        var endTime = startTime + duration;

                        // Dont check past 23:00
                        if (endTime > checkDate.AddHours(endHour))
                            continue;

                        // Check if this entire block is available
                        var isAvailable = await IsRoomAvailableAsync(roomId, startTime, endTime);
                        if (isAvailable)
                        {
                            availableSlots.Add($"{startTime:MMM dd} {startTime:HH:mm}-{endTime:HH:mm}");

                            if (availableSlots.Count >= 10)
                                return availableSlots;

                            //if timeslot is avb the start time is set at the end of avb interval 
                            startTime = endTime - interval;
                            break;
                        }
                    }
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
            var overlappingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                            !b.IsDeleted &&
                            b.Status == BookingStatus.Confirmed && //booking must be confirmed to be considered overlapping
                            b.EndTime > startTime &&
                            b.StartTime < endTime) // if another booking that has an endtime before the desired start time exists it is considered overlapping
                .AnyAsync();

            if (overlappingBookings)
            {
                return false;
            }


            var unavailabilityPeriods = await _context.UnavailabilityPeriods
                .Where(up => up.RoomId == roomId &&
                            up.IsActive &&
                            ((up.StartTime < endTime && up.EndTime > startTime)))
                .AnyAsync();

            return !unavailabilityPeriods;
        }

        private string? GetRoomPhotoUrl(int roomId) //FEATURE: Implement to add photo upload feature
        {
            return null;
        }
        

        //TODO:
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



        //TODO:
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


        //TODO:
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
    }
}
