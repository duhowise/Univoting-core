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
            // Election management messages
            CreateElection create => create.ElectionId.ToString(),
            GetElection get => get.ElectionId.ToString(),
            UpdateElection update => update.ElectionId.ToString(),
            
            // Position management
            AddPosition addPos => addPos.ElectionId.ToString(),
            GetPosition getPos => getPos.ElectionId.ToString(),
            GetPositionsForElection getPos => getPos.ElectionId.ToString(),
            
            // Candidate management
            AddCandidate addCand => addCand.ElectionId.ToString(),
            GetCandidate getCand => getCand.ElectionId.ToString(),
            GetCandidatesForPosition getCands => getCands.ElectionId.ToString(),
            
            // Voter management
            RegisterVoter regVoter => regVoter.ElectionId.ToString(),
            GetVoter getVoter => getVoter.ElectionId.ToString(),
            GetVotersForElection getVoters => getVoters.ElectionId.ToString(),
            UpdateVoterStatus updateStatus => ExtractElectionIdFromVoterId(updateStatus.VoterId),
            
            // Voting operations
            CastVote castVote => castVote.ElectionId.ToString(),
            SkipVote skipVote => skipVote.ElectionId.ToString(),
            
            // Vote queries
            GetVotesForPosition getVotes => getVotes.ElectionId.ToString(),
            GetSkippedVotesForPosition getSkipped => getSkipped.ElectionId.ToString(),
            GetVoteCount getCount => getCount.ElectionId.ToString(),
            GetSkippedVoteCount getSkippedCount => getSkippedCount.ElectionId.ToString(),
            
            // Election administration
            AddModerator addMod => addMod.ElectionId.ToString(),
            GetModerator getMod => ExtractElectionIdFromVoterId(getMod.ModeratorId),
            GetModeratorsForElection getMods => getMods.ElectionId.ToString(),
            AddDepartment addDept => addDept.ElectionId.ToString(),
            GetDepartment getDept => ExtractElectionIdFromVoterId(getDept.DepartmentId),
            AddPollingStation addPS => addPS.ElectionId.ToString(),
            GetPollingStation getPS => ExtractElectionIdFromVoterId(getPS.PollingStationId),
            
            // Statistics and results
            GetElectionStatistics getStats => getStats.ElectionId.ToString(),
            GetVotingResults getResults => getResults.ElectionId.ToString(),
            GetVoterHistory getHistory => ExtractElectionIdFromVoterId(getHistory.VoterId),
            GetVoterProgress getProgress => ExtractElectionIdFromVoterId(getProgress.VoterId),
            
            // Eligibility checks
            CheckVoterEligibility checkElig => ExtractElectionIdFromVoterId(checkElig.VoterId),
            
            _ => null
        };
    }
    
    private static string ExtractElectionIdFromVoterId(string voterId)
    {
        // For UpdateVoterStatus, we might need to look up the election ID
        // In a real implementation, this might require a registry or database lookup
        // For this demo, we'll assume there's only one election running
        return "935f39e2-177c-4d1f-8d4d-7e1a2458e09e";
    }

    private new static object ExtractEntityMessage(object message)
    {
        return message;
    }
}
