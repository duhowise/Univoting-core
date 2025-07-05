using Akka.Actor;
using Akka.Persistence;
using Univoting.Akka.Messages;
using Univoting.Akka.Models;
using Univoting.Akka.Actors.States;
using Univoting.Models;

namespace Univoting.Akka.Actors;

/// <summary>
/// Dedicated actor for managing individual positions and their voting logic
/// This allows for better concurrency and separation of concerns
/// </summary>
public class PositionActor : ReceivePersistentActor
{
    private readonly string _positionId;
    private readonly string _electionId;
    
    private string _name = string.Empty;
    private int _priority;
    private bool _initialized;
    
    private readonly Dictionary<string, CandidateState> _candidates = new();
    private readonly Dictionary<string, VoteState> _votes = new();
    private readonly Dictionary<string, SkippedVoteState> _skippedVotes = new();
    private readonly HashSet<string> _votersWhoVoted = new();
    private readonly HashSet<string> _votersWhoSkipped = new();

    public override string PersistenceId => $"position-{_positionId}";

    public PositionActor(string positionId, string electionId)
    {
        _positionId = positionId;
        _electionId = electionId;
        
        Command<VotingCommand>(HandleCommand);
        Recover<VotingEvent>(ApplyEvent);
    }

    private void HandleCommand(VotingCommand cmd)
    {
        switch (cmd)
        {
            case AddPosition addPos when addPos.PositionId == _positionId:
                HandleAddPosition(addPos);
                break;
                
            case GetPosition getPos when getPos.PositionId == _positionId:
                HandleGetPosition();
                break;
                
            case AddCandidate addCandidate when addCandidate.PositionId == _positionId:
                HandleAddCandidate(addCandidate);
                break;
                
            case CastVote castVote when castVote.PositionId == _positionId:
                HandleCastVote(castVote);
                break;
                
            case SkipVote skipVote when skipVote.PositionId == _positionId:
                HandleSkipVote(skipVote);
                break;
                
            case GetVoteCount getCount when getCount.PositionId == _positionId:
                Sender.Tell(_votes.Count);
                break;
                
            case GetSkippedVoteCount getSkipped when getSkipped.PositionId == _positionId:
                Sender.Tell(_skippedVotes.Count);
                break;
                
            case GetCandidatesForPosition getCandidates when getCandidates.PositionId == _positionId:
                HandleGetCandidates();
                break;
                
            case GetVotingResults getResults when getResults.PositionId == _positionId:
                HandleGetVotingResults();
                break;
                
            default:
                if (!_initialized)
                {
                    Sender.Tell(new Status.Failure(new InvalidOperationException("Position not initialized.")));
                }
                else
                {
                    Unhandled(cmd);
                }
                break;
        }
    }

    private void HandleAddPosition(AddPosition addPos)
    {
        if (_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position already exists.")));
            return;
        }

        Persist(new PositionAdded(addPos.ElectionId, addPos.PositionId, addPos.Name, addPos.Priority), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetPosition()
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position not found.")));
            return;
        }

        var position = new Position
        {
            Id = Guid.Parse(_positionId),
            Name = _name,
            ElectionId = Guid.Parse(_electionId),
            Priority = new Priority { Number = _priority }
        };
        
        Sender.Tell(position);
    }

    private void HandleAddCandidate(AddCandidate addCandidate)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position not initialized.")));
            return;
        }

        if (_candidates.ContainsKey(addCandidate.CandidateId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Candidate already exists.")));
            return;
        }

        Persist(new CandidateAdded(addCandidate.PositionId, addCandidate.CandidateId,
            addCandidate.FirstName, addCandidate.LastName, addCandidate.Picture, addCandidate.Priority), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleCastVote(CastVote castVote)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position not initialized.")));
            return;
        }

        // Validation
        if (_votersWhoVoted.Contains(castVote.VoterId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter has already voted for this position.")));
            return;
        }

        if (_votersWhoSkipped.Contains(castVote.VoterId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter has already skipped this position.")));
            return;
        }

        if (!_candidates.ContainsKey(castVote.CandidateId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Candidate not found.")));
            return;
        }

        Persist(new VoteCast(castVote.VoterId, castVote.CandidateId, castVote.PositionId, DateTime.UtcNow), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleSkipVote(SkipVote skipVote)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position not initialized.")));
            return;
        }

        if (_votersWhoVoted.Contains(skipVote.VoterId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter has already voted for this position.")));
            return;
        }

        if (_votersWhoSkipped.Contains(skipVote.VoterId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter has already skipped this position.")));
            return;
        }

        Persist(new VoteSkipped(skipVote.VoterId, skipVote.PositionId, DateTime.UtcNow), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetCandidates()
    {
        var candidates = _candidates.Values.Select(c => new Candidate
        {
            Id = Guid.Parse(c.CandidateId),
            FirstName = c.FirstName,
            LastName = c.LastName,
            Picture = c.Picture,
            PositionId = Guid.Parse(_positionId),
            ElectionId = Guid.Parse(_electionId)
        }).ToList();

        Sender.Tell(candidates);
    }

    private void HandleGetVotingResults()
    {
        var results = _candidates.Values.Select(c => new VotingResult
        {
            CandidateId = c.CandidateId,
            CandidateName = $"{c.FirstName} {c.LastName}",
            VoteCount = _votes.Values.Count(v => v.CandidateId == c.CandidateId),
            Priority = c.Priority
        })
        .OrderByDescending(r => r.VoteCount)
        .ThenBy(r => r.Priority)
        .ToList();

        var positionResults = new PositionVotingResults
        {
            PositionId = _positionId,
            PositionName = _name,
            TotalVotes = _votes.Count,
            TotalSkipped = _skippedVotes.Count,
            CandidateResults = results
        };

        Sender.Tell(positionResults);
    }

    private void ApplyEvent(VotingEvent evt)
    {
        switch (evt)
        {
            case PositionAdded posAdded:
                _name = posAdded.Name;
                _priority = posAdded.Priority;
                _initialized = true;
                break;

            case CandidateAdded candidateAdded:
                _candidates[candidateAdded.CandidateId] = new CandidateState
                {
                    CandidateId = candidateAdded.CandidateId,
                    FirstName = candidateAdded.FirstName,
                    LastName = candidateAdded.LastName,
                    Picture = candidateAdded.Picture,
                    Priority = candidateAdded.Priority
                };
                break;

            case VoteCast voteCast:
                var voteId = Guid.NewGuid().ToString();
                _votes[voteId] = new VoteState
                {
                    VoteId = voteId,
                    VoterId = voteCast.VoterId,
                    CandidateId = voteCast.CandidateId,
                    Time = voteCast.Time
                };
                _votersWhoVoted.Add(voteCast.VoterId);
                break;

            case VoteSkipped voteSkipped:
                var skipId = Guid.NewGuid().ToString();
                _skippedVotes[skipId] = new SkippedVoteState
                {
                    SkippedVoteId = skipId,
                    VoterId = voteSkipped.VoterId,
                    Time = voteSkipped.Time
                };
                _votersWhoSkipped.Add(voteSkipped.VoterId);
                break;
        }
    }
}
