namespace WindsurfAoC.API.Models;

public class CompetitionParticipant
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;
    
    public DateTime JoinDate { get; set; }
    
    public int TotalPoints { get; set; }
}
