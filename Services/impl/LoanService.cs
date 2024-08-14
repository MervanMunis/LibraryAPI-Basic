using LibraryAPI.Data;
using LibraryAPI.Exceptions;
using LibraryAPI.Models.DTOs.Request;
using LibraryAPI.Models.DTOs.Response;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Enums;
using LibraryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class LoanService : ILoanService
{
    private readonly LibraryAPIContext _context;
    private readonly IPenaltyService _penaltyService;

    public LoanService(LibraryAPIContext context, IPenaltyService penaltyService)
    {
        _context = context;
        _penaltyService = penaltyService;
    }

    /// <summary>
    /// Retrieves loans by employee ID.
    /// </summary>
    /// <param name="employeeId">The ID of the employee.</param>
    /// <returns>A list of loans associated with the employee.</returns>
    public async Task<ServiceResult<IEnumerable<LoanResponse>>> GetLoansByEmployeeIdAsync(string employeeId)
    {
        var loans = await _context.Loans!
            .Where(loan => loan.EmployeeId == employeeId)
            .Include(l => l.Member)
                .ThenInclude(m => m!.ApplicationUser)
            .Include(e => e.Employee)
                .ThenInclude(a => a!.ApplicationUser) 
            .Include(l => l.BookCopy)
                .ThenInclude(bc => bc!.Book)
            .Select(l => new LoanResponse()
            {
                LoanId = l.LoanId,
                BookCopyId = l.BookCopyId,
                LoanedDate = l.LoanedDate,
                CountDay = l.CountDay,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                LoanStatus = l.LoanStatus,
                BookTitle = l.BookCopy!.Book!.Title,
                MemberName = l.Member!.ApplicationUser!.Name,
                MemberIdNumber = l.Member.ApplicationUser.IdNumber.ToString(),
                EmployeeName = l.Employee!.ApplicationUser!.Name,
                EmployeeIdNumber = l.Employee.ApplicationUser.IdNumber.ToString()
            })
            .ToListAsync();
        return ServiceResult<IEnumerable<LoanResponse>>.SuccessResult(loans);
    }

    /// <summary>
    /// Retrieves loans by member ID.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <returns>A list of loans associated with the member.</returns>
    public async Task<ServiceResult<IEnumerable<LoanResponse>>> GetLoansByMemberIdAsync(string memberId)
    {
        var loans = await _context.Loans!
            .Where(loan => loan.MemberId == memberId)
            .Include(l => l.Employee)
                .ThenInclude(m => m!.ApplicationUser)
            .Include(e => e.Member)
                .ThenInclude(a => a!.ApplicationUser)
            .Include(l => l.BookCopy)
                .ThenInclude(bc => bc!.Book)
            .Select(l => new LoanResponse()
            {
                LoanId = l.LoanId,
                BookCopyId = l.BookCopyId,
                LoanedDate = l.LoanedDate,
                CountDay = l.CountDay,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                LoanStatus = l.LoanStatus,
                BookTitle = l.BookCopy!.Book!.Title,
                MemberName = l.Member!.ApplicationUser!.Name,
                MemberIdNumber = l.Member.ApplicationUser.IdNumber.ToString(),
                EmployeeName = l.Employee!.ApplicationUser!.Name,
                EmployeeIdNumber = l.Employee.ApplicationUser.IdNumber.ToString()
            })
            .ToListAsync();
        return ServiceResult<IEnumerable<LoanResponse>>.SuccessResult(loans);
    }


    /// <summary>
    /// Retrieves a loan by its ID.
    /// </summary>
    /// <param name="id">The ID of the loan.</param>
    /// <returns>The loan details.</returns>
    public async Task<ServiceResult<LoanResponse>> GetLoanByIdAsync(int id)
    {
        var loan = await _context.Loans!
            .Include(l => l.Member)
            .Include(l => l.Employee)
            .Include(l => l.BookCopy)
            .ThenInclude(bc => bc!.Book)
            .Select(l => new LoanResponse()
            {
                LoanId = l.LoanId,
                BookCopyId = l.BookCopyId,
                LoanedDate = l.LoanedDate,
                CountDay = l.CountDay,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                LoanStatus = l.LoanStatus,
                MemberName = l.Member!.ApplicationUser!.Name,
                MemberIdNumber = l.Member.ApplicationUser.IdNumber.ToString(),
                EmployeeName = l.Employee!.ApplicationUser!.Name,
                EmployeeIdNumber = l.Employee.ApplicationUser.IdNumber.ToString()
            })
            .FirstOrDefaultAsync(l => l.LoanId == id);

        if (loan == null)
        {
            return ServiceResult<LoanResponse>.FailureResult("Loan not found");
        }

        return ServiceResult<LoanResponse>.SuccessResult(loan);
    }

    /// <summary>
    /// Adds a new loan to the database.
    /// </summary>
    /// <param name="loanRequest">The loan details.</param>
    /// <returns>A success message if the loan is created successfully.</returns>
    public async Task<ServiceResult<string>> AddLoanAsync(LoanRequest loanRequest)
    {
        var bookCopy = await _context.BookCopies!
            .FirstOrDefaultAsync(bc => bc.BookCopyId == loanRequest.BookCopyId && bc.BookCopyStatus == Status.Active.ToString());

        var member = await _context.Members!
            .Include(m => m.ApplicationUser)
            .FirstOrDefaultAsync(m => m.ApplicationUser!.IdNumber == loanRequest.MemberIdNumber);

        if (member == null)
        {
            return ServiceResult<string>.FailureResult("Member with the given IDNumber does not exist!");
        }

        var employee = await _context.Employees!
            .Include(e => e.ApplicationUser)
            .FirstOrDefaultAsync(e => e.EmployeeId == loanRequest.EmployeeId);

        if (employee == null)
        {
            return ServiceResult<string>.FailureResult("Employee does not exist");
        }

        var memberIdNumber = member!.MemberId;

        if (bookCopy == null)
        {
            return ServiceResult<string>.FailureResult("Book copy not available for loan");
        }

        var newLoan = new Loan
        {
            CountDay = loanRequest.HowManyDays,
            MemberId = memberIdNumber,
            EmployeeId = loanRequest.EmployeeId,
            BookCopyId = bookCopy.BookCopyId,
            LoanStatus = Status.Borrowed.ToString()
        };

        newLoan.DueDate = newLoan.LoanedDate.AddDays(newLoan.CountDay);

        await _context.Loans!.AddAsync(newLoan);

        bookCopy.BookCopyStatus = Status.Borrowed.ToString();
        _context.Update(bookCopy).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return ServiceResult<string>.SuccessMessageResult("Loan successfully created!");
    }

    /// <summary>
    /// Updates an existing loan's status.
    /// </summary>
    /// <param name="loanUpdateRequest">The loan update details.</param>
    /// <returns>A success message if the loan is updated successfully.</returns>
    public async Task<ServiceResult<bool>> UpdateLoanAsync(LoanUpdateRequest  loanUpdateRequest)
    {
        var loan = await _context.Loans!.FindAsync(loanUpdateRequest.LoanId);
        if (loan == null)
        {
            return ServiceResult<bool>.FailureResult("Loan not found");
        }

        if (loanUpdateRequest.LoanStatus == Status.Returned.ToString() && loan.LoanStatus != Status.Borrowed.ToString())
        {
            return ServiceResult<bool>.FailureResult("The loan cannot be marked as returned if the book is not currently borrowed.");
        }

        if (loanUpdateRequest.LoanStatus != null && loan.LoanStatus != loanUpdateRequest.LoanStatus)
        {
            loan.LoanStatus = loanUpdateRequest.LoanStatus;

            LoanTransaction loanTransaction = new LoanTransaction()
            {
                LoanId = loanUpdateRequest.LoanId,
                EmployeeId = loanUpdateRequest.EmployeeId,
                LoanStatus = loanUpdateRequest.LoanStatus,
            };

            await _context.LoanTransactions!.AddAsync(loanTransaction);
        }

        _context.Update(loan).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Loans.AnyAsync(l => l.LoanId == loanUpdateRequest.LoanId))
            {
                return ServiceResult<bool>.FailureResult("Loan not found");
            }
            else
            {
                throw;
            }
        }
    }

    public async Task<ServiceResult<IEnumerable<LoanTransactionResponse>>> GetLoanTransactionByLoanId(int loanId)
    {
        var loanTransactions = await _context.LoanTransactions!
            .Where(lt => lt.LoanId == loanId)
            .Select(t => new LoanTransactionResponse
            {
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee!.ApplicationUser!.Name,
                LoanId = t.LoanId,
                LoanUpdate = t.LoanUpdate,
                LoanStatus = t.LoanStatus
            })
            .ToListAsync();

        if (loanTransactions == null || !loanTransactions.Any())
        {
            return ServiceResult<IEnumerable<LoanTransactionResponse>>.FailureResult("No loan transactions found for the given loan ID.");
        }

        return ServiceResult<IEnumerable<LoanTransactionResponse>>.SuccessResult(loanTransactions);
    }

    /// <summary>
    /// Marks a loan as returned and updates the book copy status.
    /// </summary>
    /// <param name="id">The ID of the loan to return.</param>
    /// <returns>A success message if the book is returned successfully.</returns>
    public async Task<ServiceResult<bool>> ReturnBookAsync(int id)
    {
        var loan = await _context.Loans!
            .Include(l => l.BookCopy)
            .FirstOrDefaultAsync(l => l.LoanId == id);

        if (loan == null)
        {
            return ServiceResult<bool>.FailureResult("Loan not found");
        }

        if (loan.LoanStatus != Status.Borrowed.ToString())
        {
            return ServiceResult<bool>.FailureResult("Book is not borrowed");
        }

        loan.ReturnDate = DateTime.Now;
        loan.LoanStatus = Status.Returned.ToString();

        LoanTransaction loanTransaction = new LoanTransaction()
        {
            LoanId = loan.LoanId,
            EmployeeId = loan.EmployeeId,
            LoanStatus = loan.LoanStatus,
        };

        await _context.LoanTransactions!.AddAsync(loanTransaction);

        _context.Update(loan).State = EntityState.Modified;

        if (loan.BookCopy != null)
        {
            loan.BookCopy.BookCopyStatus = Status.Active.ToString();
            _context.Update(loan.BookCopy).State = EntityState.Modified;
        }

        LoanReturnRequest loanReturnRequest = new LoanReturnRequest()
        {
            MemberId = loan.MemberId,
            ReturnDate = DateTime.Now,
            DueDate = loan.DueDate
        };

        await _penaltyService.CalculatePenaltiesAsync(loanReturnRequest);

        try
        {
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Loans!.AnyAsync(l => l.LoanId == id))
            {
                return ServiceResult<bool>.FailureResult("Loan not found");
            }
            else
            {
                throw;
            }
        }
    }
}
