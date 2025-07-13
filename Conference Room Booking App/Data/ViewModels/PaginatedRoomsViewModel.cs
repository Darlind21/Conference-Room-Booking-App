namespace Conference_Room_Booking_App.Data.ViewModels
{
    public class PaginatedRoomsViewModel
    {
        public List<RoomCardViewModel> Rooms { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; } = 6;
    }
}
