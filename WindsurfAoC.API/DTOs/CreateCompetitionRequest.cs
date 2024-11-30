namespace WindsurfAoC.API.DTOs;

public record CreateCompetitionRequest(
    string Name,
    DateTime StartDate,
    DateTime EndDate
);
