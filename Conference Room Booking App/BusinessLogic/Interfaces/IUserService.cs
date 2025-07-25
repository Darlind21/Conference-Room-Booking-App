using Conference_Room_Booking_App.Data.Models;

namespace Conference_Room_Booking_App.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        Task<AppUser?> GetAppUserByIdAsync(string userId);
    }
}
