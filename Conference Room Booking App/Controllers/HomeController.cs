using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.BusinessLogic.Services;
using Conference_Room_Booking_App.Data.Enums;
using Conference_Room_Booking_App.Data.Models;
using Conference_Room_Booking_App.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Conference_Room_Booking_App.Controllers
{
    public class HomeController(IBookingService _bookingService, IRoomService _roomService) : Controller
    {
        public async Task<IActionResult> Index(HomeViewModel model, int page = 1) //REVIEW:
            //Even though the view Index.cshtml doesnt explicitly say "Send a HomeViewModel to the controller", ASP.NET does that automatically via model-binding
        {
            var rooms = await _roomService.GetFilteredRoomsAsync(
                model.StartTime,
                model.EndTime,
                model.Date,
                model.AttendeesCount,
                page,
                6
            );

            model.Rooms = new PaginatedRoomsViewModel
            {
                Rooms = rooms.Items.Select(r => new RoomCardViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    RoomCode = r.RoomCode,
                    MaxCapacity = r.MaxCapacity,
                    PhotoUrl = r.PhotoUrl,
                    AvailableTimeSlots = r.AvailableTimeSlots
                }).ToList(),

                CurrentPage = page,
                TotalPages = rooms.TotalPages,
                TotalItems = rooms.TotalItems,
                ItemsPerPage = rooms.ItemsPerPage
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RoomsPartial", model.Rooms);
            }

            return View(model);
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> CheckBookingStatus(string bookingCode) //REVIEW: Check review below in the code 
        {
            if (string.IsNullOrEmpty(bookingCode))
            {
                return View("BookingStatus", new BookingStatusViewModel
                {
                    ErrorMessage = "Please enter a booking code."
                });
            }

            bookingCode = bookingCode.Trim().ToUpper();

            var booking = await _bookingService.GetBookingByCodeAsync(bookingCode);

            if (booking == null)
            {
                return View("BookingStatus", new BookingStatusViewModel
                {
                    ErrorMessage = "Booking code not found. Please check your code and try again."
                });
            }

            var viewModel = new BookingStatusViewModel
            {
                BookingCode = booking.BookingCode,
                Status = booking.Status.ToString(),
                StatusColor = _bookingService.GetStatusColor(booking.Status),
                RoomName = booking.Room.Name,
                RoomCode = booking.Room.RoomCode,
                AttendeesCount = booking.AttendeesCount,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Notes = booking.Notes,
                CanEdit = booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed,
                CanCancel = booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed
            };

            if (!string.IsNullOrEmpty(booking.AppUserId)) //if booking is done by an appuser
            {
                var appUserId = (User?.Identity?.IsAuthenticated == true) ?
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
                if (string.IsNullOrEmpty(appUserId)) //if the current user is not auth he cannot edit/cancel it
                {
                    viewModel.CanEdit = false;
                    viewModel.CanCancel = false;
                }
                else //else if we have an authenticated user
                {
                    if (booking.AppUserId != appUserId) //and he doesn't match the bookings appuserid he cannot edit/cancel it
                    {
                        viewModel.CanEdit = false;
                        viewModel.CanCancel = false;
                    } 
                }

            }

            if (booking.EndTime < DateTime.Now || booking.StartTime < DateTime.Now.AddMinutes(30))
            {
                viewModel.CanCancel = false; //can only cancel or edit no less then 30 mins before 
                viewModel.CanEdit = false;
            }

            //REVIEW: "GET" should be "POST" ?????
            // If this is a POST request (redirect from booking creation), redirect to GET to avoid form resubmission
            if (HttpContext.Request.Method == "POST")
            {
                return RedirectToAction("CheckBookingStatus", new { bookingCode = bookingCode });
            }

            return View("BookingStatus", viewModel);
        }
    }
}
