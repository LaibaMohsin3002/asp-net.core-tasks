using Microsoft.AspNetCore.Mvc;
using my_books.Data.Services;
using my_books.Data.ViewModels;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private BooksService _booksService;

        public BooksController(BooksService booksService)
        {
            _booksService = booksService;
        }

        [HttpPost]
        public IActionResult AddBook([FromBody] BookVM book)
        {
            _booksService.AddBookWithAuthors(book);
            return Ok();
        }

        [HttpGet("Get-book-by-id/{id}")]
        public IActionResult GetBookById(int id)
        {
            var result = _booksService.GetBookById(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("Get-books")]
        public IActionResult GetAllBooks()
        {
            var result = _booksService.GetAllBooks();
            return Ok(result);
        }

        [HttpPut("Update-book-by-id/{id}")]
        public IActionResult UpdateBook(int id, [FromBody] BookVM book)
        {
            var success = _booksService.UpdateBook(id, book);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpDelete("Delete-book-by-id/{id}")]
        public IActionResult DeleteBook(int id)
        {
            var success = _booksService.DeleteBook(id);
            if (!success)
                return NotFound();

            return Ok();
        }
    }

}