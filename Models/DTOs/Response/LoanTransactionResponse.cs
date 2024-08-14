using LibraryAPI.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.DTOs.Response
{
    public class LoanTransactionResponse
    {
        public int LoanId { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string? LoanStatus { get; set; }

        public DateTime LoanUpdate { get; set; }
    }
}
