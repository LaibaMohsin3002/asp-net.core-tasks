using my_books.Data.Models;
using my_books.Data.ViewModels;

namespace my_books.Data.Services
{
    public class BooksService
    {
        private AppDbContext _context;

        public BooksService(AppDbContext context)
        {
            _context = context;
        }

        public void AddBookWithAuthors(BookVM book)
        {
            var _book = new Book()
            {
                Title = book.Title,
                Description = book.Description,
                IsRead = book.IsRead,
                DateRead = book.IsRead ? book.DateRead.Value : null,
                rate = book.IsRead ? book.Rate.Value : null,
                Genre = book.Genre,
                CoverUrl = book.CoverUrl,
                DateAdded = DateTime.Now,
                PublisherId = book.PublisherId
            };

            _context.Books.Add(_book);
            _context.SaveChanges();

            foreach (var authorId in book.AuthorIds)
            {
                var book_author = new Book_Author()
                {
                    BookId = _book.Id,
                    AuthorId = authorId
                };
                _context.Book_Authors.Add(book_author);
            }
            _context.SaveChanges();
        }

        public List<Book> GetAllBooks() => _context.Books.ToList();

        public BookWithAuthorsVM GetBookById(int bookId)
        {
            var _bookWithAuthors = _context.Books
                .Where(n => n.Id == bookId)
                .Select(book => new BookWithAuthorsVM()
                {
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.IsRead ? book.DateRead.Value : null,
                    Rate = book.IsRead ? book.rate.Value : null,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors.Select(n => n.Author.FullName).ToList()
                }).FirstOrDefault();

            return _bookWithAuthors;
        }

        public bool UpdateBook(int id, BookVM book)
        {
            var existingBook = _context.Books.FirstOrDefault(b => b.Id == id);
            if (existingBook == null)
                return false;

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.IsRead = book.IsRead;
            existingBook.DateRead = book.IsRead ? book.DateRead.Value : null;
            existingBook.rate = book.IsRead ? book.Rate.Value : null;
            existingBook.Genre = book.Genre;
            existingBook.CoverUrl = book.CoverUrl;

            _context.Books.Update(existingBook);
            _context.SaveChanges();

            return true;
        }

        public bool DeleteBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            _context.SaveChanges();

            return true;
        }
    }
}