using CDTApi.Models;
using CDTApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CDTApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CharitiesController : ControllerBase
    {
        private readonly ICharityRepository _charityRepo;

        public CharitiesController(ICharityRepository charityRepo)
        {
            _charityRepo = charityRepo;
        }
        
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
            return Ok(_charityRepo.GetAllCharities());
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string keyword)
        {
            try
            {
            var results = _charityRepo.SearchCharities(keyword);
            if (!results.Any())
                return NotFound("No charities found matching the keyword.");

            return Ok(results);
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{charityId}")]
        public IActionResult GetById(int charityId)
        {
            try
            {
            var charity = _charityRepo.GetById(charityId);
            if (charity == null)
                return NotFound("Charity not found.");

            return Ok(charity);
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult AddCharity([FromBody] Charity newCharity)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(newCharity.Name))
                return BadRequest("Charity name is required.");

            _charityRepo.AddCharity(newCharity);

            return Ok(new { message = "Charity added", charityId = newCharity.CharityId });
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
