using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_books.ActionResults;
using my_books.Data.Services;
using my_books.Data.ViewModels;
using my_books.Data.ViewModels.Authentication;
using my_books.Exceptions;

namespace my_books.Controllers
{
    [Authorize(Roles = UserRoles.Publisher)]
    [Route("api/[controller]")]
    [ApiController]
    public class PublishersController : ControllerBase
    {
        private PublishersService _publishersService;

        private readonly ILogger<PublishersController> _logger;

        public PublishersController(PublishersService publishersService, ILogger<PublishersController> logger)
        {
            _publishersService = publishersService;
            _logger = logger;
        }

        [HttpGet("get-all-publishers")]

        public IActionResult GetAllPublishers(string? sortBy, string? searchString, int? pageNumber)
        {
           
            try
            {
                _logger.LogInformation("Fetching publishers - sortBy: {SortBy}, search: {SearchString}, page: {PageNumber}", sortBy, searchString, pageNumber);
                var _result = _publishersService.GetAllPublishers(sortBy, searchString, pageNumber);
                return Ok(_result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load publishers");
                return BadRequest("Sorry, we could not load the publishers");
            }
        }

        [HttpPost("add-publisher")]
        public IActionResult AddPublisher([FromBody] PublisherVM publisher)
        {
            try
            {
                var newPublisher = _publishersService.AddPublisher(publisher);
                return Created(nameof(AddPublisher), newPublisher);
            }
            catch (PublisherNameException ex)
            {
                return BadRequest($"{ex.Message}, Publisher name: {ex.PublisherName}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetPublisherById(int id)
        {
            var _response = _publishersService.GetPublisherById(id);

            if (_response != null)
            {
                return Ok(_response);
            }
            else
            {
                return NotFound();
            }
        }
        //[HttpPost]
        //public IActionResult AddPublisher([FromBody] PublisherVM publisher)
        //{
        //    _publishersService.AddPublisher(publisher);
        //    return Ok();
        //}


        [HttpGet("with-books-and-authors/{id}")]
        public IActionResult GetPublisherByData(int id)
        {
            var result = _publishersService.GetPublisherByData(id);
            return Ok(result);
        }

        [HttpDelete("delete-publisher-by-id/{id}")]
        public IActionResult DeletePublisherById(int id)
        {
            try
            {
                _publishersService.DeletePublisherById(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
