namespace WindsurfAoC.API.Models;

public class ChallengeCompletion
{
    public int Id { get; set; }
    
    public int DailyChallengeId { get; set; }
    public DailyChallenge DailyChallenge { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int PartNumber { get; set; } // 1 o 2
    
    public DateTime CompletionTime { get; set; }
    
    public int Position { get; set; }
}
