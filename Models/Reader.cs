using System.ComponentModel.DataAnnotations;

namespace Lab6_GRPC.Models
{
    public class Reader
    {
        [Key]
        public int ReaderID { get; set; }

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        public string? MiddleName { get; set; }
        public string? HomeAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DateOfBirth { get; set; }

        [Required]
        public string LibraryCardNumber { get; set; } = "";

        public List<BookLoan> BookLoans { get; set; } = new();
    }
}
