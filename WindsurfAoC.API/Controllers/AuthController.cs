using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindsurfAoC.API.Data;
using WindsurfAoC.API.DTOs;
using WindsurfAoC.API.Models;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Collections.Generic;

namespace WindsurfAoC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("El email ya está registrado");
        }

        using var hmac = new HMACSHA512();
        var user = new User
        {
            Email = request.Email,
            PasswordHash = GetHash25OnBase64(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new AuthResponse(user.Id, user.Email, user.IsAdmin, token);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized("Email o contraseña incorrectos");
        }

        var computedHash = GetHash25OnBase64(request.Password);
        
        if (user.PasswordHash != computedHash)
        {
            return Unauthorized("Email o contraseña incorrectos");
        }

        var token = GenerateJwtToken(user);
        return new AuthResponse(user.Id, user.Email, user.IsAdmin, token);
    }

    private byte[] GetHash256(String text)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(text);        
        // Crear una instancia del algoritmo SHA-256
        using (SHA256 sha256 = SHA256.Create())
        {
            // Calcular el hash
            byte[] hash = sha256.ComputeHash(messageBytes);
            return hash;
        }
    }

    private string GetHash25OnBase64(String text)
    {
        var hash = GetHash256(text);
        // Convertir el hash a una representación legible (base64)
        var hashBase64 = Convert.ToBase64String(hash);
        return hashBase64;
    }

    private string GetHash25OnHex(String text)
    {
        var hash = GetHash256(text);
        // Convertir el hash a una representación legible (base64)
        var hashHex = BitConverter.ToString(hash);
        return hashHex;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
