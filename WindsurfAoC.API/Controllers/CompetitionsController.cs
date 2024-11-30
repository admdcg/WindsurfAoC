using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindsurfAoC.API.Data;
using WindsurfAoC.API.Models;
using WindsurfAoC.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WindsurfAoC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompetitionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompetitionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException();
        
        return int.Parse(userIdClaim.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompetitionResponse>>> GetCompetitions()
    {
        var competitions = await _context.Competitions
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Select(c => new CompetitionResponse(
                c.Id,
                c.Name,
                c.StartDate,
                c.EndDate,
                c.IsActive,
                c.Participants.Select(p => new ParticipantResponse(
                    p.Id,
                    p.UserId,
                    p.User.Email,
                    p.JoinDate,
                    p.TotalPoints
                ))
            ))
            .ToListAsync();

        return competitions;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompetitionResponse>> GetCompetition(int id)
    {
        var competition = await _context.Competitions
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (competition == null)
        {
            return NotFound();
        }

        return new CompetitionResponse(
            competition.Id,
            competition.Name,
            competition.StartDate,
            competition.EndDate,
            competition.IsActive,
            competition.Participants.Select(p => new ParticipantResponse(
                p.Id,
                p.UserId,
                p.User.Email,
                p.JoinDate,
                p.TotalPoints
            ))
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CompetitionResponse>> CreateCompetition(CreateCompetitionRequest request)
    {
        var competition = new Competition
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        _context.Competitions.Add(competition);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCompetition),
            new { id = competition.Id },
            new CompetitionResponse(
                competition.Id,
                competition.Name,
                competition.StartDate,
                competition.EndDate,
                competition.IsActive,
                Enumerable.Empty<ParticipantResponse>()
            )
        );
    }

    [Authorize]
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinCompetition(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Verificar si la competición existe
        var competition = await _context.Competitions.FindAsync(id);
        if (competition == null)
        {
            return NotFound("Competición no encontrada");
        }

        // Verificar si la competición está activa
        if (!competition.IsActive)
        {
            return BadRequest("No puedes unirte a una competición finalizada");
        }

        // Verificar si el usuario ya es participante
        var existingParticipant = await _context.CompetitionParticipants
            .FirstOrDefaultAsync(p => p.CompetitionId == id && p.UserId == userId);

        if (existingParticipant != null)
        {
            return BadRequest("Ya eres participante en esta competición");
        }

        // Crear nuevo participante
        var participant = new CompetitionParticipant
        {
            CompetitionId = id,
            UserId = userId,
            JoinDate = DateTime.UtcNow
        };

        _context.CompetitionParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Te has unido exitosamente a la competición" });
    }

    [Authorize]
    [HttpPost("{id}/days/{day}/complete")]
    public async Task<ActionResult> CompleteChallenge(int id, int day, [FromBody] CompleteChallengeRequest request)
    {
        var userId = GetCurrentUserId();

        var competition = await _context.Competitions.FindAsync(id);
        if (competition == null)
        {
            return NotFound("Competición no encontrada");
        }

        if (!competition.IsActive)
        {
            return BadRequest("La competición no está activa");
        }

        if (day < 1 || day > 25)
        {
            return BadRequest("Día inválido");
        }

        if (request.PartNumber != 1 && request.PartNumber != 2)
        {
            return BadRequest("Parte inválida");
        }

        var challenge = await _context.DailyChallenges
            .FirstOrDefaultAsync(dc => dc.CompetitionId == id && dc.DayNumber == day);

        if (challenge == null)
        {
            challenge = new DailyChallenge
            {
                CompetitionId = id,
                DayNumber = day
            };
            _context.DailyChallenges.Add(challenge);
            await _context.SaveChangesAsync();
        }

        if (await _context.ChallengeCompletions.AnyAsync(
            cc => cc.DailyChallengeId == challenge.Id && 
                  cc.UserId == userId && 
                  cc.PartNumber == request.PartNumber))
        {
            return BadRequest("Ya has completado esta parte del desafío");
        }

        var position = await _context.ChallengeCompletions.CountAsync(
            cc => cc.DailyChallengeId == challenge.Id && 
                  cc.PartNumber == request.PartNumber) + 1;

        var completion = new ChallengeCompletion
        {
            DailyChallengeId = challenge.Id,
            UserId = userId,
            PartNumber = request.PartNumber,
            CompletionTime = DateTime.UtcNow,
            Position = position
        };

        _context.ChallengeCompletions.Add(completion);

        var participant = await _context.CompetitionParticipants
            .FirstOrDefaultAsync(p => p.CompetitionId == id && p.UserId == userId);

        if (participant != null)
        {
            // El primero obtiene 100 puntos, el segundo 99, etc.
            var points = Math.Max(100 - position + 1, 1); // Mínimo 1 punto
            participant.TotalPoints += points;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CompetitionResponse>> UpdateCompetition(int id, UpdateCompetitionRequest request)
    {
        var competition = await _context.Competitions.FindAsync(id);
        if (competition == null)
        {
            return NotFound();
        }

        competition.Name = request.Name;
        competition.StartDate = request.StartDate;
        competition.EndDate = request.EndDate;
        competition.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new CompetitionResponse(
            competition.Id,
            competition.Name,
            competition.StartDate,
            competition.EndDate,
            competition.IsActive,
            Enumerable.Empty<ParticipantResponse>()
        );
    }

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<IEnumerable<ParticipantResponse>>> GetParticipants(int id)
    {
        var participants = await _context.CompetitionParticipants
            .Include(p => p.User)
            .Where(p => p.CompetitionId == id)
            .Select(p => new ParticipantResponse(
                p.Id,
                p.UserId,
                p.User.Email,
                p.JoinDate,
                p.TotalPoints
            ))
            .ToListAsync();

        return participants;
    }

    [HttpGet("{id}/completions")]
    public async Task<ActionResult<IEnumerable<CompletionResponse>>> GetCompletions(int id)
    {
        var completions = await _context.ChallengeCompletions
            .Include(cc => cc.DailyChallenge)
            .Include(cc => cc.User)
            .Where(cc => cc.DailyChallenge.CompetitionId == id)
            .Select(cc => new CompletionResponse(
                cc.Id,
                cc.UserId,
                cc.User.Email,
                cc.DailyChallenge.DayNumber,
                cc.PartNumber,
                cc.CompletionTime
            ))
            .ToListAsync();

        return completions;
    }
}

public record CompleteChallengeRequest(int PartNumber);
