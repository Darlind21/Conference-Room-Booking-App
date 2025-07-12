using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels.Booking;
using Conference_Room_Booking_App.Data.ViewModels.Room;

namespace Conference_Room_Booking_App.BusinessLogic.Interfaces
{
    public interface IRoomService
    {
        Task<RoomDetailVM> CreateRoomAsync (CreateRoomVM payload);
        Task<List<RoomDetailVM>> GetAllActiveRoomsAsync ();
        Task<bool> EditRoomAsync(EditRoomVM payload);
        Task<List<RoomDetailVM>> FilterRoomsAsync(RoomFilterVM filter);
        Task<bool> DeactivateRoomAsync(int id);
        Task<List<AvailableTimeslotVM>> GetAvailableTimeslotsAsync(int roomId, DateOnly date);
        Task<List<BookingDetailVM>> GetAllBookingsForRoomAsync(int roomId);
        Task<RoomDetailVM> GetRoomByIdAsync(int roomId);
    }
}
