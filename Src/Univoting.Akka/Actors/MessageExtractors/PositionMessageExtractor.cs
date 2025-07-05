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
            AddPosition addPos => addPos.PositionId,
            GetPosition getPos => getPos.PositionId,
            AddCandidate addCandidate => addCandidate.PositionId,
            CastVote castVote => castVote.PositionId,
            SkipVote skipVote => skipVote.PositionId,
            GetVoteCount getCount => getCount.PositionId,
            GetSkippedVoteCount getSkippedCount => getSkippedCount.PositionId,
            GetCandidatesForPosition getCandidates => getCandidates.PositionId,
            GetVotingResults getResults => getResults.PositionId,
            _ => null
        };
    }

    private new static object ExtractEntityMessage(object message) => message;
}
