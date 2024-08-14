namespace LibraryAPI.Models.DTOs.Request
{
    public class BookCopyRequest
    {
        public long BookCopyId { get; set; }

        public int? LocationId { get; set; }
    }
}
