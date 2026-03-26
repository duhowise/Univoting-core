using System;
using Univoting.Models;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateVoter(Guid VoterId, string Name, string IdentificationNumber, VotingStatus Status);
    // Events
    public record VoterCreated(Guid VoterId, string Name, string IdentificationNumber, VotingStatus Status);
    // Relationship Commands
    public record AddSkippedVoteToVoter(Guid VoterId, Guid SkippedVoteId);
    public record AddVoteToVoter(Guid VoterId, Guid VoteId);
    // Voting Flow Commands
    public record AuthenticateVoter(Guid VoterId, string PIN);
    public record StartVoting(Guid VoterId, List<Guid> PositionIdsByFaculty);
    public record ConfirmVote(Guid VoterId, Guid PositionId, Guid VoteId);
    public record ConfirmSkip(Guid VoterId, Guid PositionId, Guid SkippedVoteId);
    // Queries
    public record GetVoter(Guid VoterId);
    // Responses
    public record VoterDetails(Guid VoterId, string Name, string IdentificationNumber, VotingStatus Status);
    public record AuthenticationResult(bool Success, string Message);
    public record NextPosition(Guid PositionId);
    public record VoteConfirmed(Guid PositionId, Guid VoteId);
    public record SkipConfirmed(Guid PositionId, Guid SkippedVoteId);
    public record VotingCompleted();
    public record VotingError(string ErrorMessage);
}
