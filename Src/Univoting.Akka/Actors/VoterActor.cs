using Akka.Actor;
using Akka.Persistence;
using Univoting.Akka.Messages;
using Univoting.Akka.Models;
using Univoting.Akka.SharedModels;

namespace Univoting.Akka.Actors;

/// <summary>
/// Dedicated actor for managing voter state and voting behavior
/// Ensures voter-specific rules and maintains voting history
/// </summary>
public class VoterActor : ReceivePersistentActor
{
    private readonly string _voterId;
    private readonly string _electionId;
    
    private string _name = string.Empty;
    private string _identificationNumber = string.Empty;
    private VotingStatus _status = VotingStatus.Pending;
    private bool _initialized;
    
    private readonly Dictionary<string, string> _votesForPositions = new(); // PositionId -> CandidateId
    private readonly HashSet<string> _skippedPositions = new();
    private readonly Dictionary<string, DateTime> _votingHistory = new(); // PositionId -> Time

    public override string PersistenceId => $"voter-{_voterId}";

    public VoterActor(string voterId, string electionId)
    {
        _voterId = voterId;
        _electionId = electionId;
        
        Command<VotingCommand>(HandleCommand);
        Recover<VotingEvent>(ApplyEvent);
    }

    private void HandleCommand(VotingCommand cmd)
    {
        switch (cmd)
        {
            case RegisterVoter regVoter when regVoter.VoterId == _voterId:
                HandleRegisterVoter(regVoter);
                break;
                
            case UpdateVoterStatus updateStatus when updateStatus.VoterId == _voterId:
                HandleUpdateVoterStatus(updateStatus);
                break;
                
            case GetVoter getVoter when getVoter.VoterId == _voterId:
                HandleGetVoter();
                break;
                
            case CheckVoterEligibility checkEligibility when checkEligibility.VoterId == _voterId:
                HandleCheckEligibility(checkEligibility);
                break;
                
            case GetVoterHistory getHistory when getHistory.VoterId == _voterId:
                HandleGetVoterHistory();
                break;
                
            case GetVoterProgress getProgress when getProgress.VoterId == _voterId:
                HandleGetVoterProgress(getProgress);
                break;
                
            default:
                if (!_initialized)
                {
                    Sender.Tell(new Status.Failure(new InvalidOperationException("Voter not registered.")));
                }
                else
                {
                    Unhandled(cmd);
                }
                break;
        }
    }

    private void HandleRegisterVoter(RegisterVoter regVoter)
    {
        if (_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter already registered.")));
            return;
        }

        Persist(new VoterRegistered(regVoter.ElectionId, regVoter.VoterId, regVoter.Name, regVoter.IdentificationNumber), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleUpdateVoterStatus(UpdateVoterStatus updateStatus)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter not registered.")));
            return;
        }

        if (_status == updateStatus.Status)
        {
            Sender.Tell(Status.Success.Instance); // No change needed
            return;
        }

        Persist(new VoterStatusUpdated(updateStatus.VoterId, updateStatus.Status), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetVoter()
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter not found.")));
            return;
        }

        var voter = new Voter
        {
            Id = Guid.Parse(_voterId),
            Name = _name,
            IdentificationNumber = _identificationNumber,
            VotingStatus = _status,
            ElectionId = Guid.Parse(_electionId)
        };

        Sender.Tell(voter);
    }

    private void HandleCheckEligibility(CheckVoterEligibility checkEligibility)
    {
        if (!_initialized)
        {
            Sender.Tell(new VoterEligibilityResult(_voterId, false, "Voter not registered"));
            return;
        }

        var hasVotedForPosition = _votesForPositions.ContainsKey(checkEligibility.PositionId);
        var hasSkippedPosition = _skippedPositions.Contains(checkEligibility.PositionId);

        var isEligible = !hasVotedForPosition && !hasSkippedPosition && _status != VotingStatus.Voted;
        var reason = !isEligible 
            ? hasVotedForPosition ? "Already voted for this position" 
              : hasSkippedPosition ? "Already skipped this position" 
              : "Voting completed"
            : "Eligible to vote";

        Sender.Tell(new VoterEligibilityResult(_voterId, isEligible, reason));
    }

    private void HandleGetVoterHistory()
    {
        var history = new VoterVotingHistory
        {
            VoterId = _voterId,
            VoterName = _name,
            Status = _status,
            VotedPositions = _votesForPositions.Keys.ToList(),
            SkippedPositions = _skippedPositions.ToList(),
            VotingTimestamps = new Dictionary<string, DateTime>(_votingHistory)
        };

        Sender.Tell(history);
    }

    private void HandleGetVoterProgress(GetVoterProgress getProgress)
    {
        var totalPositions = getProgress.TotalPositionsInElection;
        var completedPositions = _votesForPositions.Count + _skippedPositions.Count;
        
        var progress = new VoterProgress
        {
            VoterId = _voterId,
            TotalPositions = totalPositions,
            CompletedPositions = completedPositions,
            VotedPositions = _votesForPositions.Count,
            SkippedPositions = _skippedPositions.Count,
            ProgressPercentage = totalPositions > 0 ? (double)completedPositions / totalPositions * 100 : 0,
            IsComplete = completedPositions >= totalPositions
        };

        Sender.Tell(progress);
    }

    private void ApplyEvent(VotingEvent evt)
    {
        switch (evt)
        {
            case VoterRegistered voterReg:
                _name = voterReg.Name;
                _identificationNumber = voterReg.IdentificationNumber;
                _status = VotingStatus.Pending;
                _initialized = true;
                break;

            case VoterStatusUpdated statusUpdated:
                _status = statusUpdated.Status;
                break;

            case VoteCast voteCast:
                _votesForPositions[voteCast.PositionId] = voteCast.CandidateId;
                _votingHistory[voteCast.PositionId] = voteCast.Time;
                if (_status == VotingStatus.Pending)
                    _status = VotingStatus.InProgress;
                break;

            case VoteSkipped voteSkipped:
                _skippedPositions.Add(voteSkipped.PositionId);
                _votingHistory[voteSkipped.PositionId] = voteSkipped.Time;
                if (_status == VotingStatus.Pending)
                    _status = VotingStatus.InProgress;
                break;
        }
    }
}
