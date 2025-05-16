using CDTApi.DTOs;
using CDTApi.Models;
using CDTApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using CDTApi.Services; 
namespace CDTApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IDAFRepository _dafRepo;
        private readonly EmailService _emailService;
        private readonly JWTService _jwtService;

        private readonly JsonDataContext _authContext; // Assuming you have a context for database operations
        public UsersController(JsonDataContext authContext, IUserRepository userRepo, IDAFRepository dafRepo, EmailService emailService, JWTService jwtService)
        {
            _authContext = authContext;
            _userRepo = userRepo;
            _dafRepo = dafRepo;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            try
            {
            if (_userRepo.GetByEmail(dto.Email) is not null)
                return Conflict("Email already exists.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                RegistrationSource = dto.RegistrationSource,
                Mobile = dto.Mobile,
                Location = dto.Location
            };

            _userRepo.AddUser(user); // UserId generated inside

            // If the registration is for a DAF account, create the DAF record
            if (dto.RegistrationSource == "DAF")
            {
                int newId = _dafRepo.GetByUserId(user.UserId) == null
                ? _userRepo.GetAllUsers().Count() + 1
                : _dafRepo.GetByUserId(user.UserId)!.DAFAccountId;

                string accountNumber = $"DAF-{newId:D3}";

                var dafAccount = new DAFAccount
                {
                DAFAccountId = newId,
                UserId = user.UserId,
                AccountNumber = accountNumber,
                DAFBalance = 0,
                TotalDonated = 0
                };

                // After adding User + DAF account successfully
                await _emailService.SendEmailAsync(
                user.Email,
                "Welcome to Donor Advisor Fund!",
                $"<h3>Hi {user.Name},</h3><p>Thank you for registering with us.</p><p>Your DAF Account Number is <b>{accountNumber}</b>.</p><p>Happy Giving!</p>"
                );

                _dafRepo.AddDAFAccount(dafAccount);

                return Ok(new
                {
                message = "DAF user registered successfully",
                dafAccountNumber = accountNumber,
                userId = user.UserId
                });
            }

            return Ok(new { message = "User registered successfully", userId = user.UserId });
            }
            catch (Exception ex)
            {
            // Log the exception (you can use a logging framework here)
            return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
            var user = _userRepo.GetByEmail(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            user.Token = _jwtService.CreateJWTToken(user);
            var newAccessToken = user.Token;
            var newRefreshToken = _jwtService.CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            _authContext.SaveChanges(); // Save the new refresh token to the database

            return Ok(new
            {
                tokenApi = new TokenApiDTO
                {
                    AccessToken = user.Token,
                    RefreshToken = user.RefreshToken,
                },
                message = "Login successful",
                userId = user.UserId,
                name = user.Name,
                registrationSource = user.RegistrationSource
            });
            }
            catch (Exception ex)
            {
            // Log the exception (you can use a logging framework here)
            return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
            }
        }

        
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenApiDTO tokenApiDTO)
        {
            if(tokenApiDTO == null || string.IsNullOrEmpty(tokenApiDTO.RefreshToken))
                return BadRequest("Invalid token request.");
            
            string accessToken = tokenApiDTO.AccessToken;
            string refreshToken = tokenApiDTO.RefreshToken;

            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _userRepo.GetAllUsers().FirstOrDefault(u => u.UserId.ToString() == userId);
            if (user == null || user.RefreshToken != refreshToken )
                return BadRequest("Invalid token.");
            
            var newAccessToken = _jwtService.CreateJWTToken(user);
            var newRefreshToken = _jwtService.CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.Token = newAccessToken;
            _authContext.SaveChanges(); // Save the new refresh token to the database

            return Ok(new TokenApiDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
            
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
            var users = _userRepo.GetAllUsers();
            return Ok(users);
            }
            catch (Exception ex)
            {
            // Log the exception (you can use a logging framework here)
            return StatusCode(500, new { message = "An error occurred while retrieving users.", error = ex.Message });
            }
        }
        
    }
}
