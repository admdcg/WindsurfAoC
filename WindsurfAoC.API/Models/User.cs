using System.ComponentModel.DataAnnotations;

namespace WindsurfAoC.API.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public List<CompetitionParticipant> Participations { get; set; } = new();
}
