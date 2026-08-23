using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<TmsUser> userManager,
    RoleManager<IdentityRole> roleManager,
    TmsDbContext context,
    TokenService tokenService) : ControllerBase
{
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Ok(new { message = "Registration request received." });

        var user = new TmsUser { UserName = request.Email, Email = request.Email, FirstName = request.FirstName, LastName = request.LastName };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        if (!await roleManager.RoleExistsAsync(request.Role))
            await roleManager.CreateAsync(new IdentityRole(request.Role));
        await userManager.AddToRoleAsync(user, request.Role);
        return Ok(new { message = "Registration successful." });
    }

    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return Unauthorized(new { detail = "Invalid credentials." });
        if (await userManager.IsLockedOutAsync(user))
            return StatusCode(423, new { detail = "Account locked due to multiple failed login attempts." });
        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword) { await userManager.AccessFailedAsync(user); return Unauthorized(new { detail = "Invalid credentials." }); }
        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateJwt(user, roles);
        var refreshToken = new RefreshToken { Token = Guid.NewGuid().ToString("N"), UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(7), IsUsed = false, IsRevoked = false };
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();
        return Ok(new { accessToken, refreshToken = refreshToken.Token });
    }

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
        if (storedToken == null) return Unauthorized(new { detail = "Invalid refresh token." });
        if (storedToken.IsUsed)
        {
            var userTokens = await context.RefreshTokens.Where(rt => rt.UserId == storedToken.UserId).ToListAsync();
            foreach (var t in userTokens) t.IsRevoked = true;
            await context.SaveChangesAsync();
            return Unauthorized(new { detail = "Token theft detected. All user sessions revoked." });
        }
        if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { detail = "Refresh token expired or revoked." });
        storedToken.IsUsed = true;
        var newRefreshToken = new RefreshToken { Token = Guid.NewGuid().ToString("N"), UserId = storedToken.UserId, ExpiresAt = DateTime.UtcNow.AddDays(7), IsUsed = false, IsRevoked = false };
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();
        var user = await userManager.FindByIdAsync(storedToken.UserId);
        var roles = await userManager.GetRolesAsync(user!);
        var newAccessToken = tokenService.GenerateJwt(user!, roles);
        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken.Token });
    }
}