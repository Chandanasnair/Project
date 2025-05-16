// using CDTApi.Models;
// using CDTApi.Repositories;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace CDTApi.Controllers
// {
//     [Authorize]
//     [ApiController]
//     [Route("api/[controller]")]
//     public class DAFController : ControllerBase
//     {
//         private readonly IDAFRepository _dafRepo;

//         public DAFController(IDAFRepository dafRepo)
//         {
//             _dafRepo = dafRepo;
//         }

//         [HttpGet("{userId:id}")]
//         public IActionResult GetByUserId(int userId)
//         {
//             var daf = _dafRepo.GetByUserId(userId);
//             if (daf == null)
//                 return NotFound("DAF account not found.");

//             return Ok(daf);
//         }

//         [HttpPost("create")]
//         public IActionResult Create([FromBody] DAFAccount newAccount)
//         {
//             var existing = _dafRepo.GetByUserId(newAccount.UserId);
//             if (existing != null)
//                 return BadRequest("DAF account already exists.");

//             _dafRepo.AddDAFAccount(newAccount);
//             return Ok("DAF account created.");
//         }

//         [HttpGet("{AccountNumber:alpha}")]
//         public IActionResult GetByAccountNumber(string accountNumber)
//         {
//             var daf = _dafRepo.GetByAccountNumber(accountNumber);
//             if (daf == null)
//                 return NotFound("DAF account not found.");

//             return Ok(daf);
//         }
//     }
// }
using CDTApi.Models;
using CDTApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CDTApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DAFController : ControllerBase
    {
        private readonly IDAFRepository _dafRepo;

        public DAFController(IDAFRepository dafRepo)
        {
            _dafRepo = dafRepo;
        }

        

        [HttpGet("{userId:int}")] // Use 'int' for integer userId
        public IActionResult GetByUserId(int userId)
        {
            try
            {
            var daf = _dafRepo.GetByUserId(userId);
            if (daf == null)
                return NotFound("DAF account not found.");

            return Ok(daf);
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("account/{accountId:int}")] // Use 'int' for integer accountId
        public IActionResult GetByAccountId(int accountId)
        {
            try
            {
            var daf = _dafRepo.GetByAccountId(accountId);
            if (daf == null)
                return NotFound("DAF account not found.");

            return Ok(daf);
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("account/{AccountNumber}")] // Updated route to avoid conflict
        public IActionResult GetByAccountNumber(string AccountNumber)
        {
            try
            {
            var daf = _dafRepo.GetByAccountNumber(AccountNumber);
            if (daf == null)
                return NotFound("DAF account not found.");

            return Ok(daf);
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] DAFAccount newAccount)
        {
            try
            {
            var existing = _dafRepo.GetByUserId(newAccount.UserId);
            if (existing != null)
                return BadRequest("DAF account already exists.");

            _dafRepo.AddDAFAccount(newAccount);
            return Ok("DAF account created.");
            }
            catch (Exception ex)
            {
            return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}