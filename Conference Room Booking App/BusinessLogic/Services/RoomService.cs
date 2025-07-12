using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.Data;
using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels.Booking;
using Conference_Room_Booking_App.Data.ViewModels.Room;
using Microsoft.EntityFrameworkCore;

namespace Conference_Room_Booking_App.BusinessLogic.Services
{
    public class RoomService(ApplicationDbContext context) : IRoomService
    {
        public async Task<RoomDetailVM> CreateRoomAsync(CreateRoomVM payload)
        {
            var roomCodeExists = context.Rooms
                .Any(r => r.RoomCode == payload.RoomCode);

            if (roomCodeExists) throw new Exception("A room with this code already exists. Room code must be unique!");

            var room = new Room
            {
                Name = payload.Name,
                RoomCode = payload.RoomCode,
                MaxCapacity = payload.MaxCapacity
            };

            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var createdRoom = new RoomDetailVM
            {
                Id = room.Id,
                Name = room.Name,
                RoomCode = room.RoomCode,
                MaxCapacity = room.MaxCapacity
            };

            return createdRoom;
        }

        public async Task<bool> DeactivateRoomAsync(int id)
        {
            var room = await context.Rooms
                .Where(x => x.Id == id)
                .Include(x => x.Bookings)
                .SingleOrDefaultAsync();

            if (room == null) throw new Exception("Room to be deactivated with this id does not exist");

            bool hasFutureOrOngoingConfirmedBookings = room.Bookings
                .Any(x => x.Status == Data.Enums.BookingStatus.Confirmed &&
                     x.EndTime > DateTime.UtcNow);

                if (hasFutureOrOngoingConfirmedBookings)
                {
                    throw new Exception("Cannot delete room since it has confirmed bookings in the future");
                }

            room.IsActive = false;

            return await context.SaveChangesAsync() > 1;
        }

        public async Task<bool> EditRoomAsync(EditRoomVM payload)
        {
            var room = await context.Rooms.FindAsync(payload.Id);
            //have not used Include() since the user might not want to change the room capacity
            //will only get bookings per room if user wants to change maxcapacity

            if (room == null) throw new Exception("Room to be edited with this id does not exist");

            if (payload.Name != null) room.Name = payload.Name;

            if (payload.MaxCapacity != null)
            {
                var bookingsForRoomQuery = context.Bookings
                    .Where(b => b.Id == room.Id);

                var belowConfirmedBookingsCapacity = await bookingsForRoomQuery
                    .AnyAsync(b => b.Status == Data.Enums.BookingStatus.Confirmed && b.AttendeesCount < payload.MaxCapacity);

                if (belowConfirmedBookingsCapacity) throw new Exception
                        ("Unable to edit this rooms maxcapacity because there are conflicting confirmed bookings that need more capacity. Please cancel bookings first!");

                room.MaxCapacity = payload.MaxCapacity.Value;

                var pendingBookings = await bookingsForRoomQuery //we cancel pending bookings that exceeds the new max capacity
                    .Where(b => b.Status == Data.Enums.BookingStatus.Pending 
                            && b.AttendeesCount < payload.MaxCapacity)
                    .ToListAsync();
                
                    foreach(var booking in pendingBookings)
                    {
                        booking.Status = Data.Enums.BookingStatus.Cancelled;
                    }
            }

            return await context.SaveChangesAsync() > 0;

        }

        public Task<List<RoomDetailVM>> FilterRoomsAsync(RoomFilterVM filter)
        {
            //var query = context.Rooms
            //    .Include(x => x.Bookings)
            //    .Include(r => r.UnavailabilityPeriods)
            //    .AsQueryable();

            //if (filter.Name != null)
            //{
            //    query = query
            //        .Where(x => x.Name.Contains(filter.Name));
            //}

            //if (filter.MinCapacity != null)
            //{
            //    query = query
            //        .Where(x => x.MaxCapacity >= filter.MinCapacity);
            //}

            //if (filter.MaxCapacity != null)
            //{
            //    query = query
            //        .Where(x => x.MaxCapacity <= filter.MaxCapacity);
            //}

            //var filteredRooms = await query.ToListAsync();

            //var roomListVM = new RoomListVM();

            //foreach (var room in filteredRooms)
            //{
            //    var bookingDetailVMs = bookingService.GetBookingsPerRoom(room.Id);

            //    var roomDetailVM = new RoomDetailVM
            //    {
            //        Id = room.Id,
            //        Name = room.Name,
            //        RoomCode = room.RoomCode,
            //        MaxCapacity = room.MaxCapacity,

            //        Bookings = bookingDetailVMs
            //    };

            //    roomListVM.RoomDetailVMs.Add(room);
            //}

            throw new NotImplementedException();
        }

        public async Task<List<RoomDetailVM>> GetAllActiveRoomsAsync()
        {
            var activeRooms = await context.Rooms
                .Where(r => r.IsActive == true)
                .ToListAsync();

            var roomDetailVMList = new List<RoomDetailVM>();

            if (activeRooms.Any())
            {
                foreach (var room in activeRooms)
                {
                    var bookings = await GetAllBookingsForRoomAsync(room.Id);

                    var roomDetailVM = new RoomDetailVM
                    {
                        Id = room.Id,
                        Name = room.Name,
                        RoomCode = room.RoomCode,
                        MaxCapacity = room.MaxCapacity
                    };

                    roomDetailVMList.Add(roomDetailVM);
                }
            }

            return roomDetailVMList;
        }

        public async Task<List<AvailableTimeslotVM>> GetAvailableTimeslotsAsync(int roomId, DateOnly date)
        {
            throw new NotImplementedException();
        }

        public async Task<RoomDetailVM> GetRoomByIdAsync(int roomId)
        {
            var room = await context.Rooms.FindAsync(roomId);

            if (room == null) throw new Exception("Room to get with this id does not exist");
        }
    }
}
