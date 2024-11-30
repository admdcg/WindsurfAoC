namespace WindsurfAoC.API.DTOs;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password);
public record AuthResponse(int Id, string Email, bool IsAdmin, string Token);
