namespace WindsurfAoC.API.DTOs;

public record UpdateCompetitionRequest(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);
