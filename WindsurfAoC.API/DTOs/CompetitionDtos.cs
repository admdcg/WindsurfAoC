namespace WindsurfAoC.API.DTOs;

public record CreateCompetitionRequest(string Name, DateTime StartDate, DateTime EndDate);
public record CompetitionResponse(
    int Id, 
    string Name, 
    DateTime StartDate, 
    DateTime EndDate, 
    bool IsActive,
    IEnumerable<ParticipantResponse> Participants
);

public record ParticipantResponse(
    int Id,
    int UserId,
    string Email,
    DateTime JoinDate,
    int TotalPoints
);

public record CompletionResponse(
    int Id,
    int UserId,
    string UserEmail,
    int DayNumber,
    int PartNumber,
    DateTime CompletedAt
);
