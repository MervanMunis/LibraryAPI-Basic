using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs.Request
{
    public class LoanUpdateRequest
    {
        [Required]
        public int LoanId { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        public string? LoanStatus { get; set; }
    }
}
