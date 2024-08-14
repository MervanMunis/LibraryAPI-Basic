using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models.Entities
{
    public class LoanTransaction
    {
        public int LoanId { get; set; }

        [ForeignKey(nameof(LoanId))]
        public Loan? Loan { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

        public string? LoanStatus { get; set; }

        [DataType(DataType.Date)]
        public DateTime LoanUpdate { get; set; } = DateTime.Now;
    }
}
