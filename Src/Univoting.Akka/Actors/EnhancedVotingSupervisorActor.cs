using Akka.Actor;
using Univoting.Akka.Utility;
using Univoting.Akka.Messages;
using Univoting.Akka.Models;
using Univoting.Akka.Actors.MessageExtractors;


namespace Univoting.Akka.Actors;

/// <summary>
/// Enhanced supervisor actor that manages different types of entities
/// Provides better separation of concerns and scalability
/// </summary>
public class EnhancedVotingSupervisorActor : UntypedActor
{
    // Internal coordination messages
    private record EligibilityChecked(CastVote CastVote, VoterEligibilityResult EligibilityResult, IActorRef OriginalSender);
    private record SkipEligibilityChecked(SkipVote SkipVote, VoterEligibilityResult EligibilityResult, IActorRef OriginalSender);
    
    private readonly IActorRef _electionsParent;
    private readonly IActorRef _positionsParent;
    private readonly IActorRef _votersParent;

    public EnhancedVotingSupervisorActor()
    {
        // Create separate parent actors for different entity types
        _electionsParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new ElectionMessageExtractor(),
                id => Props.Create(() => new ElectionActor(Guid.Parse(id)))
            ),
            "elections-parent");

        _positionsParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new PositionMessageExtractor(),
                id => Props.Create(() => new PositionActor(id, ExtractElectionIdFromPositionId(id)))
            ),
            "positions-parent");

        _votersParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new VoterMessageExtractor(),
                id => Props.Create(() => new VoterActor(id, ExtractElectionIdFromVoterId(id)))
            ),
            "voters-parent");
    }

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            // Internal coordination messages
            case EligibilityChecked eligibilityChecked:
                HandleEligibilityChecked(eligibilityChecked);
                break;
                
            case SkipEligibilityChecked skipEligibilityChecked:
                HandleSkipEligibilityChecked(skipEligibilityChecked);
                break;
            
            // Election-only commands
            case CreateElection:
            case GetElection:
            case UpdateElection:
            case AddModerator:
            case AddDepartment:
            case AddPollingStation:
            case GetPositionsForElection:
            case GetVotersForElection:
            case RegisterVoter:
            case UpdateVoterStatus:
                _electionsParent.Forward(message);
                break;

            // Position-specific commands (route to position actors)
            case AddPosition:
            case GetPosition:
            case AddCandidate:
            case GetCandidatesForPosition:
            case GetVotingResults:
                _positionsParent.Forward(message);
                break;
                
            // Vote counting (can be handled by either, but let's route to positions for consistency)
            case GetVoteCount:
            case GetSkippedVoteCount:
                _positionsParent.Forward(message);
                break;

            // Voter-specific commands
            case GetVoter:
            case CheckVoterEligibility:
            case GetVoterHistory:
            case GetVoterProgress:
                _votersParent.Forward(message);
                break;

            // Voting commands that need coordination
            case CastVote castVote:
                HandleCastVote(castVote);
                break;

            case SkipVote skipVote:
                HandleSkipVote(skipVote);
                break;

            // Query commands
            case GetElectionStatistics getStats:
                HandleGetElectionStatistics(getStats);
                break;

            default:
                Unhandled(message);
                break;
        }
    }

    private void HandleCastVote(CastVote castVote)
    {
        var sender = Sender;
        
        // First check voter eligibility using proper actor pattern
        _votersParent.Ask<VoterEligibilityResult>(
                new CheckVoterEligibility(castVote.VoterId, castVote.PositionId), 
                TimeSpan.FromSeconds(5))
            .PipeTo(Self, sender, 
                success: eligibilityResult => new EligibilityChecked(castVote, eligibilityResult, sender),
                failure: ex => new Status.Failure(ex));
    }
    
    private void HandleEligibilityChecked(EligibilityChecked message)
    {
        if (message.EligibilityResult.IsEligible)
        {
            // If eligible, forward to position actor
            _positionsParent.Ask<object>(message.CastVote, TimeSpan.FromSeconds(10))
                .PipeTo(message.OriginalSender);
        }
        else
        {
            message.OriginalSender.Tell(new Status.Failure(
                new InvalidOperationException(message.EligibilityResult.Reason)));
        }
    }

    private void HandleSkipVote(SkipVote skipVote)
    {
        var sender = Sender;
        
        // First check voter eligibility using proper actor pattern
        _votersParent.Ask<VoterEligibilityResult>(
                new CheckVoterEligibility(skipVote.VoterId, skipVote.PositionId), 
                TimeSpan.FromSeconds(5))
            .PipeTo(Self, sender, 
                success: eligibilityResult => new SkipEligibilityChecked(skipVote, eligibilityResult, sender),
                failure: ex => new Status.Failure(ex));
    }
    
    private void HandleSkipEligibilityChecked(SkipEligibilityChecked message)
    {
        if (message.EligibilityResult.IsEligible)
        {
            // If eligible, forward to position actor
            _positionsParent.Ask<object>(message.SkipVote, TimeSpan.FromSeconds(10))
                .PipeTo(message.OriginalSender);
        }
        else
        {
            message.OriginalSender.Tell(new Status.Failure(
                new InvalidOperationException(message.EligibilityResult.Reason)));
        }
    }

    private void HandleGetElectionStatistics(GetElectionStatistics getStats)
    {
        var sender = Sender;
        
        // Use Task.Run with proper return type for PipeTo
        Task.Run(async () =>
        {
            try
            {
                // Get election info
                var election = await _electionsParent.Ask<Election>(
                    new GetElection(getStats.ElectionId), TimeSpan.FromSeconds(10));
                
                // Get positions
                var positions = await _electionsParent.Ask<List<Position>>(
                    new GetPositionsForElection(getStats.ElectionId), TimeSpan.FromSeconds(10));
                
                // Get voters
                var voters = await _electionsParent.Ask<List<Voter>>(
                    new GetVotersForElection(getStats.ElectionId), TimeSpan.FromSeconds(10));
                
                // Get vote counts for each position
                var positionStats = new List<PositionStatistics>();
                foreach (var position in positions)
                {
                    var voteCount = await _positionsParent.Ask<int>(
                        new GetVoteCount(position.Id.ToString(), election.Id), TimeSpan.FromSeconds(5));
                    
                    var skippedCount = await _positionsParent.Ask<int>(
                        new GetSkippedVoteCount(position.Id.ToString(), election.Id), TimeSpan.FromSeconds(5));
                    
                    positionStats.Add(new PositionStatistics
                    {
                        PositionId = position.Id.ToString(),
                        PositionName = position.Name,
                        VoteCount = voteCount,
                        SkippedCount = skippedCount,
                        TotalParticipation = voteCount + skippedCount
                    });
                }
                
                var statistics = new ElectionStatistics
                {
                    ElectionId = getStats.ElectionId,
                    ElectionName = election.Name,
                    TotalVoters = voters.Count,
                    TotalPositions = positions.Count,
                    PositionStatistics = positionStats,
                    OverallParticipationRate = CalculateOverallParticipationRate(voters.Count, positionStats)
                };
                
                sender.Tell(statistics);
            }
            catch (Exception ex)
            {
                sender.Tell(new Status.Failure(ex));
            }
        });
    }

    private double CalculateOverallParticipationRate(int totalVoters, List<PositionStatistics> positionStats)
    {
        if (totalVoters == 0 || positionStats.Count == 0)
            return 0;

        var totalPossibleVotes = totalVoters * positionStats.Count;
        var totalActualParticipation = positionStats.Sum(p => p.TotalParticipation);
        
        return (double)totalActualParticipation / totalPossibleVotes * 100;
    }

    private static string ExtractElectionIdFromVoterId(string voterId)
    {
        var parts = voterId.Split('-');
        return parts.Length > 1 ? parts[0] : voterId;
    }

    private static string ExtractElectionIdFromPositionId(string positionId)
    {
        var parts = positionId.Split('-');
        return parts.Length > 1 ? parts[0] : positionId;
    }
}

// Message extractors for different entity types