using Univoting.Akka.Messages;
using Univoting.Akka.Utility;

namespace Univoting.Akka.Actors.MessageExtractors;

public class PositionMessageExtractor : EntityMessageExtractor
{
    public PositionMessageExtractor() : base(
        ExtractEntityId,
        ExtractEntityMessage,
        "position-shard")
    {
    }

    private new static string? ExtractEntityId(object message)
    {
        return message switch
        {
            // Position management
            AddPosition addPos => addPos.PositionId,
            GetPosition getPos => getPos.PositionId,
            
            // Candidate management
            AddCandidate addCandidate => addCandidate.PositionId,
            GetCandidatesForPosition getCandidates => getCandidates.PositionId,
            
            // Voting operations
            CastVote castVote => castVote.PositionId,
            SkipVote skipVote => skipVote.PositionId,
            
            // Vote counting and results
            GetVoteCount getCount => getCount.PositionId,
            GetSkippedVoteCount getSkippedCount => getSkippedCount.PositionId,
            GetVotingResults getResults => getResults.PositionId,
            
            _ => null
        };
    }

    private new static object ExtractEntityMessage(object message) => message;
}
