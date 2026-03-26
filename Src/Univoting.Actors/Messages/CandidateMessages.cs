using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateCandidate(Guid CandidateId, string FirstName, string LastName, Guid PositionId, Guid ElectionId, int? PriorityNumber);
    // Events
    public record CandidateCreated(Guid CandidateId, string FirstName, string LastName, Guid PositionId, Guid ElectionId, int? PriorityNumber);
    // Queries
    public record GetCandidate(Guid CandidateId);
    // Responses
    public record CandidateDetails(Guid CandidateId, string FirstName, string LastName, Guid PositionId, Guid ElectionId, int? PriorityNumber);
}
