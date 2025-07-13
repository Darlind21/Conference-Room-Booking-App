using Conference_Room_Booking_App.BusinessLogic.Interfaces;
using Conference_Room_Booking_App.BusinessLogic.Services;
using Conference_Room_Booking_App.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Conference_Room_Booking_App.Controllers
{
    public class BookingController (IBookingService _bookingService, IRoomService _roomService) : Controller
    {
        public async Task<IActionResult> Create(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            var availableSlots = await _roomService.GetAvailableTimeSlotsAsync(roomId);

            var viewModel = new CreateEditBookingViewModel
            {
                IsEdit = false,
                RoomDetail = new RoomDetailViewModel
                {
                    Id = room.Id,
                    Name = room.Name,
                    RoomCode = room.RoomCode,
                    MaxCapacity = room.MaxCapacity,
                    PhotoUrl = room.PhotoUrl,
                    AvailableTimeSlots = availableSlots
                },
                BookingInfo = new BookingInfoViewModel
                {
                    StartTime = DateTime.Now.AddHours(1),
                    EndTime = DateTime.Now.AddHours(2)
                },
                ReservationHolder = new ReservationHolderViewModel()
            };

            return View("CreateEdit", viewModel);
        }

        public async Task<IActionResult> Edit(string bookingCode)
        {
            var booking = await _bookingService.GetBookingByCodeAsync(bookingCode);
            if (booking == null)
            {
                return NotFound();
            }

            var availableSlots = await _roomService.GetAvailableTimeSlotsAsync(booking.RoomId);

            var viewModel = new CreateEditBookingViewModel
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                IsEdit = true,
                RoomDetail = new RoomDetailViewModel
                {
                    Id = booking.Room.Id,
                    Name = booking.Room.Name,
                    RoomCode = booking.Room.RoomCode,
                    MaxCapacity = booking.Room.MaxCapacity,
                    PhotoUrl = booking.Room.PhotoUrl,
                    AvailableTimeSlots = availableSlots
                },
                BookingInfo = new BookingInfoViewModel
                {
                    AttendeesCount = booking.AttendeesCount,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    Notes = booking.Notes
                },
                ReservationHolder = new ReservationHolderViewModel
                {
                    FirstName = booking.ReservationHolder.FirstName,
                    LastName = booking.ReservationHolder.LastName,
                    Email = booking.ReservationHolder.Email,
                    IdCardNumber = booking.ReservationHolder.IdCardNumber,
                    PhoneNumber = booking.ReservationHolder.PhoneNumber
                }
            };

            return View("CreateEdit", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(CreateEditBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload room details for display
                var room = await _roomService.GetRoomByIdAsync(model.RoomDetail.Id);
                if (room != null)
                {
                    model.RoomDetail.AvailableTimeSlots = await _roomService.GetAvailableTimeSlotsAsync(room.Id);
                }
                return View("CreateEdit", model);
            }

            if (model.IsEdit && model.BookingId.HasValue)
            {
                var updatedBooking = await _bookingService.UpdateBookingAsync(
                    model.BookingId.Value,
                    model.BookingInfo,
                    model.ReservationHolder
                );

                if (updatedBooking != null)
                {
                    return RedirectToAction("CheckBookingStatus", "Home", new { bookingCode = updatedBooking.BookingCode });
                }
            }
            else
            {
                var newBooking = await _bookingService.CreateBookingAsync(
                    model.RoomDetail.Id,
                    model.BookingInfo,
                    model.ReservationHolder
                );

                if (newBooking != null)
                {
                    return RedirectToAction("CheckBookingStatus", "Home", new { bookingCode = newBooking.BookingCode });
                }
            }

            ModelState.AddModelError("", "An error occurred while processing your booking. Please try again.");
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(string bookingCode)
        {
            var result = await _bookingService.CancelBookingAsync(bookingCode);

            if (result)
            {
                return RedirectToAction("CheckBookingStatus", "Home", new { bookingCode });
            }

            TempData["ErrorMessage"] = "Unable to cancel booking. Please try again.";
            return RedirectToAction("CheckBookingStatus", "Home", new { bookingCode });
        }
    }
}
