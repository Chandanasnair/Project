using CDTApi.Models;
using CDTApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CDTApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BrokerageController : ControllerBase
    {
        private readonly IBrokerageRepository _brokerageRepo;

        public BrokerageController(IBrokerageRepository brokerageRepo)
        {
            _brokerageRepo = brokerageRepo;
        }

        [HttpPost("create")]
        public IActionResult CreateBrokerageAccount([FromBody] BrokerageAccount account)
        {
            try
            {
            var existing = _brokerageRepo.GetByUserId(account.UserId);
            if (existing != null)
                return BadRequest("Brokerage account already exists for this user.");

            _brokerageRepo.AddBrokerageAccount(account);
            return Ok(new { message = "Brokerage account created." });
            }
            catch (Exception ex)
            {
            return StatusCode(500, new { message = "An error occurred while creating the brokerage account.", error = ex.Message });
            }
        }

        [HttpGet("{userId}")]
        public IActionResult GetBrokerageAccount(int userId)
        {
            try
            {
            var account = _brokerageRepo.GetByUserId(userId);
            if (account == null)
                return NotFound("No brokerage account found.");

            return Ok(account);
            }
            catch (Exception ex)
            {
            return StatusCode(500, new { message = "An error occurred while retrieving the brokerage account.", error = ex.Message });
            }
        }
    }
}
