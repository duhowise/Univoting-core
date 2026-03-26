using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateVote(Guid VoteId, Guid VoterId, Guid CandidateId, DateTime Time, Guid PositionId);
    // Events
    public record VoteCreated(Guid VoteId, Guid VoterId, Guid CandidateId, DateTime Time, Guid PositionId);
    // Queries
    public record GetVote(Guid VoteId);
    // Responses
    public record VoteDetails(Guid VoteId, Guid VoterId, Guid CandidateId, DateTime Time, Guid PositionId);
}
