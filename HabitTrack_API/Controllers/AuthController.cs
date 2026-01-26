using HabitTrack_API.Common;
using HabitTracker.Application.DTOs;
using HabitTracker.Application.UseCases.Auth;
using HabitTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace HabitTrack_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserEntity>> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }

        [HttpPost("login/test")]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok("LOGIN ENDPOINT REACHED");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Value);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (!result.IsSuccess)
                return Unauthorized(result.ErrorMessage);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            var result = await _authService.LogoutAsync(userId);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new UserResponseDTO
            {
                Id = User.GetUserId().ToString(),
                UserName = User.Identity?.Name!,
                Email = User.FindFirst(ClaimTypes.Email)?.Value!,
                Role = User.FindFirst(ClaimTypes.Role)?.Value!
            });
        }
    }
}
