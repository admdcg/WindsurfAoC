using Microsoft.EntityFrameworkCore;
using WindsurfAoC.API.Models;

namespace WindsurfAoC.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<DailyChallenge> DailyChallenges { get; set; }
    public DbSet<ChallengeCompletion> ChallengeCompletions { get; set; }
    public DbSet<CompetitionParticipant> CompetitionParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<CompetitionParticipant>()
            .HasOne(cp => cp.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CompetitionParticipant>()
            .HasOne(cp => cp.Competition)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyChallenge>()
            .HasOne(dc => dc.Competition)
            .WithMany(c => c.DailyChallenges)
            .HasForeignKey(dc => dc.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChallengeCompletion>()
            .HasOne(cc => cc.DailyChallenge)
            .WithMany(dc => dc.Completions)
            .HasForeignKey(cc => cc.DailyChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChallengeCompletion>()
            .HasOne(cc => cc.User)
            .WithMany()
            .HasForeignKey(cc => cc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
