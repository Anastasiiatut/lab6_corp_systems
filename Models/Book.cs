using System.ComponentModel.DataAnnotations;

namespace Lab6_GRPC.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Author { get; set; } = "";

        public double Price { get; set; }

        public List<BookLoan> BookLoans { get; set; } = new();
    }
}
