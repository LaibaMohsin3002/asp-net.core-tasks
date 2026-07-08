using Microsoft.EntityFrameworkCore;
using my_books.Data.Models;

namespace my_books.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book_Author> Book_Authors { get; set; }
        public DbSet<Log> Logs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book_Author>()
                .HasKey(ba => ba.Id);

            modelBuilder.Entity<Book_Author>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.Book_Authors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<Book_Author>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.Book_Authors)
                .HasForeignKey(ba => ba.AuthorId);

            modelBuilder.Entity<Log>().HasKey(n => n.Id);
        }
    }
}