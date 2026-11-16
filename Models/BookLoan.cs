using System.ComponentModel.DataAnnotations;

namespace Lab6_GRPC.Models
{
    public class BookLoan
    {
        [Key]
        public int LoanID { get; set; }

        public int ReaderID { get; set; }
        public int BookID { get; set; }

        [Required]
        public string BorrowDate { get; set; } = "";

        public string? ReturnDate { get; set; }

        public Reader Reader { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
