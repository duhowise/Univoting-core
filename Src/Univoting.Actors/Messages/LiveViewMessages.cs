using System;
using System.Collections.Generic;

namespace Univoting.Actors.Messages
{
    // Query from LiveViewActor to vote-parent
    public record GetVoteCountsPerPosition();
    // Response from vote-parent to LiveViewActor
    public record VoteCountsPerPosition(Dictionary<Guid, int> VoteCounts);

    // Query from parent to each VoteActor
    public record GetVotePosition();
    // Response from VoteActor
    public record VotePosition(Guid PositionId);
}
