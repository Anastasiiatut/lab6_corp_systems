using Lab6_GRPC.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab6_GRPC.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<Reader> Readers => Set<Reader>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookLoan> BookLoans => Set<BookLoan>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=library.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reader>()
                .HasIndex(r => r.LibraryCardNumber)
                .IsUnique();

            modelBuilder.Entity<BookLoan>()
                .HasOne(l => l.Reader)
                .WithMany(r => r.BookLoans)
                .HasForeignKey(l => l.ReaderID);

            modelBuilder.Entity<BookLoan>()
                .HasOne(l => l.Book)
                .WithMany(b => b.BookLoans)
                .HasForeignKey(l => l.BookID);
        }
    }
}
