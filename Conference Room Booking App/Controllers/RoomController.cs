using Microsoft.AspNetCore.Mvc;

namespace Conference_Room_Booking_App.Controllers
{
    public class RoomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
