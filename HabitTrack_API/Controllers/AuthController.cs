using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Auth;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HabitTrack_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserEntity>> Register(UserDTO request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user == null)
                return BadRequest("User already exists.");

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return BadRequest("Username or password invalid");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin-only")]
        public IActionResult AuthorizedEndpoint()
        {
            return Ok("You are Authenticated!");
        }

    }
}
