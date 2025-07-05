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
            GetPosition getPos => getPos.ElectionId,
            GetPositionsForElection getPos => getPos.ElectionId,
            RegisterVoter regVoter => regVoter.ElectionId,
            GetVotersForElection getVoters => getVoters.ElectionId,
            AddCandidate addCandidate => addCandidate.ElectionId,
            GetCandidatesForPosition getCandidates => getCandidates.ElectionId,
            CastVote castVote => castVote.ElectionId,
            SkipVote skipVote => skipVote.ElectionId,
            GetVoteCount getCount => getCount.ElectionId,
            GetSkippedVoteCount getSkippedCount => getSkippedCount.ElectionId,
            GetVotingResults getResults => getResults.ElectionId,
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
}
