using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateSkippedVote(Guid SkippedVoteId, Guid VoterId, DateTime Time, Guid PositionId);
    // Events
    public record SkippedVoteCreated(Guid SkippedVoteId, Guid VoterId, DateTime Time, Guid PositionId);
    // Queries
    public record GetSkippedVote(Guid SkippedVoteId);
    public record GetSkippedVotePosition();
    // Responses
    public record SkippedVoteDetails(Guid SkippedVoteId, Guid VoterId, DateTime Time, Guid PositionId);
    public record SkippedVotePosition(Guid PositionId);
    public record GetSkippedVoteCountsPerPosition();
    public record SkippedVoteCountsPerPosition(Dictionary<Guid, int> SkippedVoteCounts);
}
