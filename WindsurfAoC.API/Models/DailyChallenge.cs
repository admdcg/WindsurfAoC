namespace WindsurfAoC.API.Models;

public class DailyChallenge
{
    public int Id { get; set; }
    
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;
    
    public int DayNumber { get; set; }
    
    public List<ChallengeCompletion> Completions { get; set; } = new();
}
