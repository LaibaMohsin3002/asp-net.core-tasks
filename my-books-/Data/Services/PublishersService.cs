using my_books.Data.Models;
using my_books.Data.Paging;
using my_books.Data.ViewModels;
using my_books.Exceptions;
using System.Text.RegularExpressions;

namespace my_books.Data.Services
{
    public class PublishersService
    {
        private AppDbContext _context;

        public PublishersService(AppDbContext context)
        {
            _context = context;
        }

        public List<Publisher> GetAllPublishers(string? sortBy, string? searchString, int? pageNumber)
        {
            var allPublishers = _context.Publishers.OrderBy(n => n.Name).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "name_desc":
                        allPublishers = allPublishers.OrderByDescending(n => n.Name).ToList();
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                allPublishers = allPublishers.Where(n => n.Name.Contains(searchString)).ToList();
            }

            int pageSize = 5;
            allPublishers = PaginatedList<Publisher>.Create(allPublishers.AsQueryable(), pageNumber ?? 1, pageSize);
            return allPublishers;
        }

        public Publisher AddPublisher(PublisherVM publisher)
        {
            if (StringStartsWithNumber(publisher.Name))
                throw new PublisherNameException("Name starts with number", publisher.Name);

            var _publisher = new Publisher()
            {
                Name = publisher.Name
            };
            _context.Publishers.Add(_publisher);
            _context.SaveChanges();

            return _publisher;
        }
        public Publisher GetPublisherById(int id) => _context.Publishers.FirstOrDefault(n => n.Id == id);
        public PublisherWithBooksAndAuthorsVM GetPublisherByData(int publisherId)
        {
            var _publisherWithBooks = _context.Publishers
                .Where(p => p.Id == publisherId)
                .Select(publisher => new PublisherWithBooksAndAuthorsVM()
                {
                    Name = publisher.Name,
                    BookAuthors = publisher.Books.Select(book => new BookAuthorVM()
                    {
                        BookName = book.Title,
                        BookAuthors = book.Book_Authors.Select(ba => ba.Author.FullName).ToList()
                    }).ToList()
                }).FirstOrDefault();

            return _publisherWithBooks;
        }


        //public void DeletePublisherById(int id)
        //{
        //    var _publisher = _context.Publishers.FirstOrDefault(p => p.Id == id);

        //    if (_publisher != null)
        //    {
        //        // Get all books belonging to this publisher
        //        var booksByPublisher = _context.Books.Where(b => b.PublisherId == id).ToList();

        //        foreach (var book in booksByPublisher)
        //        {
        //            // Delete Book_Author rows for each book first
        //            var bookAuthors = _context.Book_Authors.Where(ba => ba.BookId == book.Id);
        //            _context.Book_Authors.RemoveRange(bookAuthors);
        //        }

        //        _context.Books.RemoveRange(booksByPublisher);
        //        _context.Publishers.Remove(_publisher);
        //        _context.SaveChanges();
        //    }
        //}

        public void DeletePublisherById(int id)
        {
            var _publisher = _context.Publishers.FirstOrDefault(n => n.Id == id);

            if (_publisher != null)
            {
                _context.Publishers.Remove(_publisher);
                _context.SaveChanges();
            }
            else
            {
                throw new Exception($"The publisher with ID {id} does not exist");
            }
        }

        private bool StringStartsWithNumber(string name) => (Regex.IsMatch(name, @"^\d"));
    }
}