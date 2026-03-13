using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;
using MoM.Api.Services;
using Microsoft.Data.SqlClient;

namespace MoM.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly MomContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly TokenService _tokenService;

        public AuthController(MomContext context, PasswordHasher passwordHasher, TokenService tokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userName = request.UserName.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest("Username is required.");
            }

            var exists = await _context.AuthUsers.AnyAsync(u => u.UserName.ToLower() == userName.ToLower());
            if (exists)
            {
                return Conflict("Username is already taken.");
            }

            var salt = _passwordHasher.GenerateSalt();
            var authUser = new AuthUser
            {
                UserName = userName,
                PasswordSalt = salt,
                PasswordHash = _passwordHasher.HashPassword(request.Password, salt)
            };

            _context.AuthUsers.Add(authUser);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsAuthSchemaMissing(ex))
            {
                return Problem(
                    title: "Authentication database is not ready",
                    detail: "Run the latest backend migration before registering users.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            return Ok(BuildResponse(authUser));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userName = request.UserName.Trim();
            AuthUser? authUser;

            try
            {
                authUser = await _context.AuthUsers
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());
            }
            catch (SqlException ex) when (IsMissingObjectSqlException(ex))
            {
                return Problem(
                    title: "Authentication database is not ready",
                    detail: "Run the latest backend migration before logging in.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            if (authUser is null || !_passwordHasher.VerifyPassword(request.Password, authUser.PasswordSalt, authUser.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(BuildResponse(authUser));
        }

        private AuthResponse BuildResponse(AuthUser authUser)
        {
            var (token, expiresAtUtc) = _tokenService.CreateToken(authUser.Id, authUser.UserName);
            return new AuthResponse
            {
                UserId = authUser.Id,
                UserName = authUser.UserName,
                Token = token,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        private static bool IsAuthSchemaMissing(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx && IsMissingObjectSqlException(sqlEx);
        }

        private static bool IsMissingObjectSqlException(SqlException ex)
        {
            return ex.Number == 208;
        }
    }
}
