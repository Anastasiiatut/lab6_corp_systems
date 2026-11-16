using Grpc.Core;
using Lab6_GRPC.Data;
using Lab6_GRPC.Models;
using Microsoft.EntityFrameworkCore;
namespace Lab6_GRPC.Services;

public class LibraryGrpcService : LibraryService.LibraryServiceBase
{
    public override async Task<ActiveLoanList> GetActiveLoans(Empty request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var loans = await db.BookLoans
            .Where(l => l.ReturnDate == null)
            .Select(l => new ActiveLoanSchema
            {
                LoanID = l.LoanID,
                ReaderName = l.Reader.LastName + " " + l.Reader.FirstName,
                Title = l.Book.Title,
                Author = l.Book.Author,
                BorrowDate = l.BorrowDate
            })
            .ToListAsync();

        var response = new ActiveLoanList();
        response.Items.AddRange(loans);
        return response;
    }

    public override async Task<BookList> GetBooks(Empty request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var books = await db.Books
            .Select(b => new BookSchema
            {
                BookID = b.BookID,
                Title = b.Title,
                Author = b.Author,
                Price = b.Price
            })
            .ToListAsync();

        var response = new BookList();
        response.Items.AddRange(books);
        return response;
    }

    public override async Task<BookLoanList> GetAllLoans(Empty request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var rawLoans = await db.BookLoans
            .OrderByDescending(l => l.LoanID)
            .Select(l => new
            {
                l.LoanID,
                ReaderName = l.Reader.LastName + " " + l.Reader.FirstName,
                l.Book.Title,
                l.BorrowDate,
                l.ReturnDate
            })
            .ToListAsync();

        var response = new BookLoanList();
        foreach (var l in rawLoans)
        {
            var item = new BookLoanSchema
            {
                LoanID = l.LoanID,
                ReaderName = l.ReaderName,
                Title = l.Title,
                BorrowDate = l.BorrowDate
            };

            if (l.ReturnDate is not null)
                item.ReturnDate = l.ReturnDate;

            response.Items.Add(item);
        }

        return response;
    }

    public override async Task<ReaderList> GetReaders(Empty request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var rawReaders = await db.Readers.ToListAsync();

        var response = new ReaderList();
        foreach (var r in rawReaders)
        {
            var item = new ReaderSchema
            {
                ReaderID = r.ReaderID,
                LastName = r.LastName,
                FirstName = r.FirstName,
                LibraryCardNumber = r.LibraryCardNumber
            };

            if (r.MiddleName is not null) item.MiddleName = r.MiddleName;
            if (r.HomeAddress is not null) item.HomeAddress = r.HomeAddress;
            if (r.PhoneNumber is not null) item.PhoneNumber = r.PhoneNumber;
            if (r.DateOfBirth is not null) item.DateOfBirth = r.DateOfBirth;

            response.Items.Add(item);
        }

        return response;
    }

    public override async Task<BookLoanSchema> ReturnBook(ReturnBookRequest request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var loan = await db.BookLoans
            .Include(l => l.Reader)
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.LoanID == request.Id);

        if (loan == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Видачу з ID {request.Id} не знайдено"));
        }

        if (loan.ReturnDate == null)
        {
            loan.ReturnDate = DateTime.Now.ToString("yyyy-MM-dd");
            await db.SaveChangesAsync();
        }

        var response = new BookLoanSchema
        {
            LoanID = loan.LoanID,
            ReaderName = loan.Reader.LastName + " " + loan.Reader.FirstName,
            Title = loan.Book.Title,
            BorrowDate = loan.BorrowDate
        };

        if (loan.ReturnDate is not null)
            response.ReturnDate = loan.ReturnDate;

        return response;
    }

    public override async Task<BookLoanSchema> AddLoan(AddLoanRequest request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var loan = new BookLoan
        {
            ReaderID = request.ReaderID,
            BookID = request.BookID,
            BorrowDate = DateTime.Now.ToString("yyyy-MM-dd"),
            ReturnDate = null
        };

        db.BookLoans.Add(loan);
        await db.SaveChangesAsync();

        var saved = await db.BookLoans
            .Include(l => l.Reader)
            .Include(l => l.Book)
            .FirstAsync(l => l.LoanID == loan.LoanID);

        var response = new BookLoanSchema
        {
            LoanID = saved.LoanID,
            ReaderName = saved.Reader.LastName + " " + saved.Reader.FirstName,
            Title = saved.Book.Title,
            BorrowDate = saved.BorrowDate
        };

        if (saved.ReturnDate is not null)
            response.ReturnDate = saved.ReturnDate;

        return response;
    }

    public override async Task<BookSchema> CreateBook(CreateBookRequest request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Price = request.Price
        };

        db.Books.Add(book);
        await db.SaveChangesAsync();

        return new BookSchema
        {
            BookID = book.BookID,
            Title = book.Title,
            Author = book.Author,
            Price = book.Price
        };
    }

    public override async Task<ReaderSchema> CreateReader(CreateReaderRequest request, ServerCallContext context)
    {
        using var db = new LibraryContext();

        var reader = new Reader
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.HasMiddleName ? request.MiddleName : null,
            HomeAddress = request.HasAddress ? request.Address : null,
            PhoneNumber = request.HasPhone ? request.Phone : null,
            DateOfBirth = request.HasDob ? request.Dob : null,
            LibraryCardNumber = request.Card
        };

        db.Readers.Add(reader);
        await db.SaveChangesAsync();

        var response = new ReaderSchema
        {
            ReaderID = reader.ReaderID,
            LastName = reader.LastName,
            FirstName = reader.FirstName,
            LibraryCardNumber = reader.LibraryCardNumber
        };

        if (reader.MiddleName is not null) response.MiddleName = reader.MiddleName;
        if (reader.HomeAddress is not null) response.HomeAddress = reader.HomeAddress;
        if (reader.PhoneNumber is not null) response.PhoneNumber = reader.PhoneNumber;
        if (reader.DateOfBirth is not null) response.DateOfBirth = reader.DateOfBirth;

        return response;
    }
}
