using Akka.Cluster.Sharding;
using Univoting.Akka.Messages;
using Univoting.Akka.Utility;

namespace Univoting.Akka.Actors.MessageExtractors;

public class ElectionMessageExtractor : EntityMessageExtractor, IMessageExtractor
{
    public ElectionMessageExtractor() : base(
        ExtractEntityId,
        ExtractEntityMessage,
        "election-shard")
    {
    }

    private new static string? ExtractEntityId(object message)
    {
        return message switch
        {
            CreateElection create => create.ElectionId,
            GetElection get => get.ElectionId,
            UpdateElection update => update.ElectionId,
            AddPosition addPos => addPos.ElectionId,
            GetPositionsForElection getPos => getPos.ElectionId,
            RegisterVoter regVoter => regVoter.ElectionId,
            GetVotersForElection getVoters => getVoters.ElectionId,
            UpdateVoterStatus updateStatus => ExtractElectionIdFromVoterId(updateStatus.VoterId),
            CastVote castVote => ExtractElectionIdFromVoterId(castVote.VoterId),
            SkipVote skipVote => ExtractElectionIdFromVoterId(skipVote.VoterId),
            GetVotesForPosition getVotes => ExtractElectionIdFromPositionId(getVotes.PositionId),
            GetSkippedVotesForPosition getSkipped => ExtractElectionIdFromPositionId(getSkipped.PositionId),
            GetVoteCount getCount => ExtractElectionIdFromPositionId(getCount.PositionId),
            GetSkippedVoteCount getSkippedCount => ExtractElectionIdFromPositionId(getSkippedCount.PositionId),
            AddCandidate addCandidate => ExtractElectionIdFromPositionId(addCandidate.PositionId),
            GetCandidatesForPosition getCandidates => ExtractElectionIdFromPositionId(getCandidates.PositionId),
            AddModerator addMod => addMod.ElectionId,
            AddDepartment addDept => addDept.ElectionId,
            AddPollingStation addPS => addPS.ElectionId,
            _ => null
        };
    }

    private new static object ExtractEntityMessage(object message)
    {
        return message;
    }

    // In a real system, you would need a way to map voter IDs to election IDs
    // For this example, we'll assume the election ID is embedded in the voter ID
    private static string? ExtractElectionIdFromVoterId(string voterId)
    {
        // This is a simplified approach - in reality you'd need a lookup mechanism
        // For now, assume voter ID format: "electionId-voterNumber"
        var parts = voterId.Split('-');
        return parts.Length > 1 ? parts[0] : null;
    }

    // Similar for position IDs
    private static string? ExtractElectionIdFromPositionId(string positionId)
    {
        // Simplified approach - in reality you'd need a lookup mechanism
        // For now, assume position ID format: "electionId-positionNumber"
        var parts = positionId.Split('-');
        return parts.Length > 1 ? parts[0] : null;
    }
}
