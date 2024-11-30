using System.ComponentModel.DataAnnotations;

namespace WindsurfAoC.API.Models;

public class Competition
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public List<CompetitionParticipant> Participants { get; set; } = new();

    public List<DailyChallenge> DailyChallenges { get; set; } = new();
}
