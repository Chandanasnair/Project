using CDTApi.DTOs;
using CDTApi.Models;
using CDTApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using CDTApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace CDTApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferRepository _transferRepo;
        private readonly IBrokerageRepository _brokerageRepo;
        private readonly IDAFRepository _dafRepo;

        public TransfersController(
            ITransferRepository transferRepo,
            IBrokerageRepository brokerageRepo,
            IDAFRepository dafRepo)
        {
            _transferRepo = transferRepo;
            _brokerageRepo = brokerageRepo;
            _dafRepo = dafRepo;
        }

        [HttpPost("fund-daf")]
        public IActionResult TransferToDAF([FromBody] TransferRequestDTO dto)
        {
            try
            {
            Console.WriteLine($"Transfer request: {dto.UserId}, {dto.BrokerageAccountId}, {dto.DAFAccountId}, {dto.Amount}");
            var brokerage = _brokerageRepo.GetByUserId(dto.UserId);
            var daf = _dafRepo.GetByAccountId(dto.DAFAccountId);

            if (brokerage == null || daf == null)
                return BadRequest("Account not found.");

            if (dto.Amount <= 0 || dto.Amount > brokerage.AvailableBalance)
                return BadRequest("Invalid or insufficient amount.");

            var transfer = new Transfer
            {
                UserId = dto.UserId,
                BrokerageAccountId = dto.BrokerageAccountId,
                DAFAccountId = dto.DAFAccountId,
                Amount = dto.Amount,
                Date = DateTime.Now,
                Status = "Success",
                ReferenceNote = dto.ReferenceNote
            };

            _transferRepo.AddTransfer(transfer);
            _brokerageRepo.UpdateBalance(brokerage.BrokerageAccountId, dto.Amount);
            _dafRepo.UpdateBalance(daf.DAFAccountId, dto.Amount, isDonation: false);

            return Ok(new { message = "Transfer successful", transferId = transfer.TransferId });
            }
            catch (Exception ex)
            {
            Console.WriteLine($"Error occurred during transfer: {ex.Message}");
            return StatusCode(500, "An error occurred while processing the transfer.");
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetTransfersForUser(int userId)
        {
            try
            {
            var transfers = _transferRepo.GetTransfersByUserId(userId);
            return Ok(transfers);
            }
            catch (Exception ex)
            {
            Console.WriteLine($"Error occurred while fetching transfers for user {userId}: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving the transfers.");
            }
        }
    }
}
