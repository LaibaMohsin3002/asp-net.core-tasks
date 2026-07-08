using Microsoft.AspNetCore.Mvc;
using my_books.Data.Services;
using my_books.Data.ViewModels;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private AuthorsService _authorsService;

        public AuthorsController(AuthorsService authorsService)
        {
            _authorsService = authorsService;
        }

        [HttpPost]
        public IActionResult AddAuthor([FromBody] AuthorVM author)
        {
            _authorsService.AddAuthor(author);
            return Ok();
        }

        [HttpGet("get-authors-with-book-id/{id}")]
        public IActionResult GetAuthorById(int id)
        {
            var result = _authorsService.GetAuthorById(id);
            return Ok(result);
        }
    }
}