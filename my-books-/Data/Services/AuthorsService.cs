using my_books.Data.Models;
using my_books.Data.ViewModels;

namespace my_books.Data.Services
{
    public class AuthorsService
    {
        private AppDbContext _context;

        public AuthorsService(AppDbContext context)
        {
            _context = context;
        }

        public void AddAuthor(AuthorVM book)
        {
            var _author = new Author()
            {
                FullName = book.FullName
            };
            _context.Authors.Add(_author);
            _context.SaveChanges();
        }

        public AuthorWithBooksVM GetAuthorById(int authorId)
        {
            var _authorWithBooks = _context.Authors
                .Where(a => a.Id == authorId)
                .Select(author => new AuthorWithBooksVM()
                {
                    FullName = author.FullName,
                    BookNames = author.Book_Authors.Select(ba => ba.Book.Title).ToList()
                }).FirstOrDefault();

            return _authorWithBooks;
        }
    }
}