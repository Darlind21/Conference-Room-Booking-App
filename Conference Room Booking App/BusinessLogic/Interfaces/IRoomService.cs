using Conference_Room_Booking_App.Data.DTOs;
using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels;
//using Conference_Room_Booking_App.Data.ViewModels.Booking;
//using Conference_Room_Booking_App.Data.ViewModels.Room;

namespace Conference_Room_Booking_App.BusinessLogic.Interfaces
{
    public interface IRoomService
    {
        Task<Room?> GetRoomByIdAsync(int id);
        Task<PaginatedResult<RoomCardViewModel>> GetFilteredRoomsAsync(DateTime? startTime, DateTime? endTime, DateTime? date, int? attendeesCount, int page, int itemsPerPage);
        Task<List<string>> GetAvailableTimeSlotsAsync(int roomId);
        Task<List<Room>> GetAllActiveRoomsAsync();

        //TODO:
    //    Task<RoomDetailVM> CreateRoomAsync (CreateRoomVM payload);
        //TODO:
    //    Task<bool> EditRoomAsync(EditRoomVM payload);
        //TODO:
    //    Task<bool> DeactivateRoomAsync(int id);
    }
}
