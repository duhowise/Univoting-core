using Akka.Actor;
using Akka.Persistence;
using Univoting.Akka.Messages;
using Univoting.Akka.Actors.States;
using Univoting.Akka.Actors.Snapshots;
using Univoting.Models;

namespace Univoting.Akka.Actors;

public class ElectionActor : ReceivePersistentActor
{
    private const int SnapshotInterval = 50;
    
    private readonly Guid _electionId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private byte[]? _logo;
    private string? _brandColour;
    private bool _initialized;
    private int _eventCount;
    
    private readonly Dictionary<string, PositionState> _positions = new();
    private readonly Dictionary<string, VoterState> _voters = new();
    private readonly Dictionary<string, ModeratorState> _moderators = new();
    private readonly Dictionary<string, DepartmentState> _departments = new();
    private readonly Dictionary<string, PollingStationState> _pollingStations = new();

    public override string PersistenceId => _electionId.ToString();

    public ElectionActor(Guid electionId)
    {
        _electionId = electionId;
        
        // Command handlers
        Command<VotingCommand>(HandleCommand);
        
        // Event recovery handlers
        Recover<VotingEvent>(ApplyEvent);
        
        // Snapshot recovery
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is ElectionSnapshot snap)
            {
                _name = snap.Name;
                _description = snap.Description;
                _logo = snap.Logo;
                _brandColour = snap.BrandColour;
                _positions.Clear();
                foreach (var pos in snap.Positions)
                    _positions[pos.Key] = pos.Value;
                _voters.Clear();
                foreach (var voter in snap.Voters)
                    _voters[voter.Key] = voter.Value;
                _moderators.Clear();
                foreach (var mod in snap.Moderators)
                    _moderators[mod.Key] = mod.Value;
                _departments.Clear();
                foreach (var dept in snap.Departments)
                    _departments[dept.Key] = dept.Value;
                _pollingStations.Clear();
                foreach (var ps in snap.PollingStations)
                    _pollingStations[ps.Key] = ps.Value;
                _eventCount = snap.EventCount;
                _initialized = !string.IsNullOrEmpty(_name);
            }
        });
    }

    private void HandleCommand(VotingCommand cmd)
    {
        // Add validation for all commands
        if (cmd == null)
        {
            Sender.Tell(new Status.Failure(new ArgumentNullException(nameof(cmd))));
            return;
        }

        switch (cmd)
        {
            case CreateElection create when create.ElectionId == _electionId:
                HandleCreateElection(create);
                break;
                
            case GetElection get when get.ElectionId == _electionId:
                HandleGetElection();
                break;
                
            case UpdateElection update when update.ElectionId == _electionId:
                HandleUpdateElection(update);
                break;
                
            case AddPosition addPos when addPos.ElectionId == _electionId && _initialized:
                HandleAddPosition(addPos);
                break;
                
            case GetPosition getPos when getPos.ElectionId == _electionId && _positions.ContainsKey(getPos.PositionId):
                HandleGetPosition(getPos);
                break;
                
            case GetPositionsForElection getPos when getPos.ElectionId == _electionId:
                HandleGetPositionsForElection();
                break;
                
            case RegisterVoter regVoter when regVoter.ElectionId == _electionId:
                HandleRegisterVoter(regVoter);
                break;
                
            case GetVotersForElection getVoters when getVoters.ElectionId == _electionId:
                HandleGetVotersForElection();
                break;
                
            case UpdateVoterStatus updateStatus when _voters.ContainsKey(updateStatus.VoterId):
                HandleUpdateVoterStatus(updateStatus);
                break;
                
            case CastVote castVote when castVote.ElectionId == _electionId && ValidateVote(castVote):
                HandleCastVote(castVote);
                break;
                
            case SkipVote skipVote when skipVote.ElectionId == _electionId && ValidateSkipVote(skipVote):
                HandleSkipVote(skipVote);
                break;
                
            case GetVotesForPosition getVotes when getVotes.ElectionId == _electionId && _positions.Values.Any(p => p.PositionId == getVotes.PositionId):
                HandleGetVotesForPosition(getVotes);
                break;
                
            case GetSkippedVotesForPosition getSkipped when getSkipped.ElectionId == _electionId && _positions.Values.Any(p => p.PositionId == getSkipped.PositionId):
                HandleGetSkippedVotesForPosition(getSkipped);
                break;
                
            case GetVoteCount getCount when getCount.ElectionId == _electionId && _positions.Values.Any(p => p.PositionId == getCount.PositionId):
                HandleGetVoteCount(getCount);
                break;
                
            case GetSkippedVoteCount getSkippedCount when getSkippedCount.ElectionId == _electionId && _positions.Values.Any(p => p.PositionId == getSkippedCount.PositionId):
                HandleGetSkippedVoteCount(getSkippedCount);
                break;
                
            case AddCandidate addCandidate when addCandidate.ElectionId == _electionId && ValidateAddCandidate(addCandidate):
                HandleAddCandidate(addCandidate);
                break;
                
            case GetCandidatesForPosition getCandidates when getCandidates.ElectionId == _electionId && _positions.Values.Any(p => p.PositionId == getCandidates.PositionId):
                HandleGetCandidatesForPosition(getCandidates);
                break;
                
            case GetCandidate getCandidate when getCandidate.ElectionId == _electionId:
                HandleGetCandidate(getCandidate);
                break;
                
            case GetVoter getVoter when getVoter.ElectionId == _electionId:
                HandleGetVoter(getVoter);
                break;
                
            case AddModerator addMod when addMod.ElectionId == _electionId:
                HandleAddModerator(addMod);
                break;
                
            case AddDepartment addDept when addDept.ElectionId == _electionId:
                HandleAddDepartment(addDept);
                break;
                
            case AddPollingStation addPS when addPS.ElectionId == _electionId:
                HandleAddPollingStation(addPS);
                break;
                
            default:
                // Provide specific error messages for common misrouting issues
                var errorMessage = cmd switch
                {
                    CreateElection create when create.ElectionId != _electionId => 
                        $"Command for election {create.ElectionId} sent to actor {_electionId}",
                    AddPosition addPos when !_initialized => 
                        "Cannot add position: Election not initialized",
                    CastVote castVote when castVote.ElectionId != _electionId => 
                        $"Vote for election {castVote.ElectionId} sent to actor {_electionId}",
                    _ when !_initialized => 
                        "Election not created.",
                    _ => 
                        $"Unhandled command type: {cmd.GetType().Name}"
                };
                
                Sender.Tell(new Status.Failure(new InvalidOperationException(errorMessage)));
                break;
        }
    }

    private void HandleCreateElection(CreateElection create)
    {
        if (_initialized)
        {
            Sender.Tell(new Status.Success("Election already exists."));
            return;
        }

        Persist(new ElectionCreated(create.ElectionId, create.Name, create.Description, create.Logo, create.BrandColour), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetElection()
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Election not found.")));
            return;
        }

        var election = new Election
        {
            Id = _electionId,
            Name = _name,
            Description = _description,
            Logo = _logo,
            BrandColour = _brandColour
        };
        
        Sender.Tell(election);
    }

    private void HandleUpdateElection(UpdateElection update)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Election not found.")));
            return;
        }

        Persist(new ElectionUpdated(update.ElectionId, update.Name, update.Description, update.Logo, update.BrandColour), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleAddPosition(AddPosition addPos)
    {
        if (_positions.ContainsKey(addPos.PositionId))
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

    private void HandleGetPosition(GetPosition getPos)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Election not found.")));
            return;
        }

        if (!_positions.TryGetValue(getPos.PositionId, out var positionState))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Position not found.")));
            return;
        }

        var position = new Position
        {
            Id = Guid.Parse(positionState.PositionId),
            Name = positionState.Name,
            ElectionId =_electionId,
            Priority = new Priority { Number = positionState.Priority }
        };
        
        Sender.Tell(position);
    }

    private void HandleGetPositionsForElection()
    {
        var positions = _positions.Values.Select(p => new Position
        {
            Id = Guid.Parse(p.PositionId),
            Name = p.Name,
            ElectionId =_electionId
        }).ToList();
        
        Sender.Tell(positions);
    }

    private void HandleRegisterVoter(RegisterVoter regVoter)
    {
        if (_voters.ContainsKey(regVoter.VoterId))
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

    private void HandleGetVotersForElection()
    {
        var voters = _voters.Values.Select(v => new Voter
        {
            Id = Guid.Parse(v.VoterId),
            Name = v.Name,
            IdentificationNumber = v.IdentificationNumber,
            VotingStatus = v.Status,
            ElectionId =_electionId
        }).ToList();
        
        Sender.Tell(voters);
    }

    private void HandleUpdateVoterStatus(UpdateVoterStatus updateStatus)
    {
        Persist(new VoterStatusUpdated(updateStatus.VoterId, updateStatus.Status), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private bool ValidateVote(CastVote castVote)
    {
        if (!_voters.ContainsKey(castVote.VoterId))
            return false;
            
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == castVote.PositionId);
        if (position == null)
            return false;
            
        if (!position.Candidates.ContainsKey(castVote.CandidateId))
            return false;
            
        // Check if voter already voted for this position
        return !position.Votes.Values.Any(v => v.VoterId == castVote.VoterId);
    }

    private void HandleCastVote(CastVote castVote)
    {
        Persist(new VoteCast(castVote.VoterId, castVote.CandidateId, castVote.PositionId, DateTime.UtcNow), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private bool ValidateSkipVote(SkipVote skipVote)
    {
        if (!_voters.ContainsKey(skipVote.VoterId))
            return false;
            
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == skipVote.PositionId);
        if (position == null)
            return false;
            
        // Check if voter already voted or skipped this position
        return !position.Votes.Values.Any(v => v.VoterId == skipVote.VoterId) &&
               !position.SkippedVotes.Values.Any(s => s.VoterId == skipVote.VoterId);
    }

    private void HandleSkipVote(SkipVote skipVote)
    {
        Persist(new VoteSkipped(skipVote.VoterId, skipVote.PositionId, DateTime.UtcNow), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetVotesForPosition(GetVotesForPosition getVotes)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == getVotes.PositionId);
        if (position != null)
        {
            var votes = position.Votes.Values.Select(v => new Vote
            {
                Id = Guid.Parse(v.VoteId),
                VoterId = Guid.Parse(v.VoterId),
                CandidateId = Guid.Parse(v.CandidateId),
                PositionId = Guid.Parse(getVotes.PositionId),
                Time = v.Time
            }).ToList();
            
            Sender.Tell(votes);
        }
        else
        {
            Sender.Tell(new List<Vote>());
        }
    }

    private void HandleGetSkippedVotesForPosition(GetSkippedVotesForPosition getSkipped)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == getSkipped.PositionId);
        if (position != null)
        {
            var skippedVotes = position.SkippedVotes.Values.Select(s => new SkippedVote
            {
                Id = Guid.Parse(s.SkippedVoteId),
                VoterId = Guid.Parse(s.VoterId),
                PositionId = Guid.Parse(getSkipped.PositionId),
                Time = s.Time
            }).ToList();
            
            Sender.Tell(skippedVotes);
        }
        else
        {
            Sender.Tell(new List<SkippedVote>());
        }
    }

    private void HandleGetVoteCount(GetVoteCount getCount)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == getCount.PositionId);
        var count = position?.Votes.Count ?? 0;
        Sender.Tell(count);
    }

    private void HandleGetSkippedVoteCount(GetSkippedVoteCount getSkippedCount)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == getSkippedCount.PositionId);
        var count = position?.SkippedVotes.Count ?? 0;
        Sender.Tell(count);
    }

    private bool ValidateAddCandidate(AddCandidate addCandidate)
    {
        return _positions.Values.Any(p => p.PositionId == addCandidate.PositionId);
    }

    private void HandleAddCandidate(AddCandidate addCandidate)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == addCandidate.PositionId);
        if (position != null && position.Candidates.ContainsKey(addCandidate.CandidateId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Candidate already exists in position.")));
            return;
        }

        Persist(new CandidateAdded(addCandidate.PositionId, addCandidate.CandidateId, 
            addCandidate.FirstName, addCandidate.LastName, addCandidate.Picture, addCandidate.Priority), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleGetCandidatesForPosition(GetCandidatesForPosition getCandidates)
    {
        var position = _positions.Values.FirstOrDefault(p => p.PositionId == getCandidates.PositionId);
        if (position != null)
        {
            var candidates = position.Candidates.Values.Select(c => new Candidate
            {
                Id = Guid.Parse(c.CandidateId),
                FirstName = c.FirstName,
                LastName = c.LastName,
                Picture = c.Picture,
                PositionId = Guid.Parse(getCandidates.PositionId),
                ElectionId =_electionId
            }).ToList();
            
            Sender.Tell(candidates);
        }
        else
        {
            Sender.Tell(new List<Candidate>());
        }
    }

    private void HandleGetCandidate(GetCandidate getCandidate)
    {
        // Find the candidate across all positions
        foreach (var position in _positions.Values)
        {
            if (position.Candidates.TryGetValue(getCandidate.CandidateId, out var candidateState))
            {
                var candidate = new Candidate
                {
                    Id = Guid.Parse(candidateState.CandidateId),
                    FirstName = candidateState.FirstName,
                    LastName = candidateState.LastName,
                    Picture = candidateState.Picture,
                    PositionId = Guid.Parse(position.PositionId),
                    ElectionId = _electionId
                };
                
                Sender.Tell(candidate);
                return;
            }
        }
        
        Sender.Tell(new Status.Failure(new InvalidOperationException("Candidate not found.")));
    }

    private void HandleGetVoter(GetVoter getVoter)
    {
        if (!_initialized)
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Election not found.")));
            return;
        }

        if (!_voters.TryGetValue(getVoter.VoterId, out var voterState))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Voter not found.")));
            return;
        }

        var voter = new Voter
        {
            Id = Guid.Parse(voterState.VoterId),
            Name = voterState.Name,
            IdentificationNumber = voterState.IdentificationNumber,
            VotingStatus = voterState.Status,
            ElectionId = _electionId
        };
        
        Sender.Tell(voter);
    }

    private void HandleAddModerator(AddModerator addMod)
    {
        if (_moderators.ContainsKey(addMod.ModeratorId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Moderator already exists.")));
            return;
        }

        Persist(new ModeratorAdded(addMod.ElectionId, addMod.ModeratorId, addMod.Name, addMod.Badge), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleAddDepartment(AddDepartment addDept)
    {
        if (_departments.ContainsKey(addDept.DepartmentId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Department already exists.")));
            return;
        }

        Persist(new DepartmentAdded(addDept.ElectionId, addDept.DepartmentId, addDept.Name), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void HandleAddPollingStation(AddPollingStation addPS)
    {
        if (_pollingStations.ContainsKey(addPS.PollingStationId))
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Polling station already exists.")));
            return;
        }

        Persist(new PollingStationAdded(addPS.ElectionId, addPS.PollingStationId, addPS.Name), evt =>
        {
            ApplyEvent(evt);
            Sender.Tell(Status.Success.Instance);
        });
    }

    private void ApplyEvent(VotingEvent evt)
    {
        switch (evt)
        {
            case ElectionCreated created:
                _name = created.Name;
                _description = created.Description;
                _logo = created.Logo;
                _brandColour = created.BrandColour;
                _initialized = true;
                break;
                
            case ElectionUpdated updated:
                if (!string.IsNullOrEmpty(updated.Name))
                    _name = updated.Name;
                if (!string.IsNullOrEmpty(updated.Description))
                    _description = updated.Description;
                if (updated.Logo != null)
                    _logo = updated.Logo;
                if (updated.BrandColour != null)
                    _brandColour = updated.BrandColour;
                break;
                
            case PositionAdded posAdded:
                _positions[posAdded.PositionId] = new PositionState
                {
                    PositionId = posAdded.PositionId,
                    Name = posAdded.Name,
                    Priority = posAdded.Priority
                };
                break;
                
            case VoterRegistered voterReg:
                _voters[voterReg.VoterId] = new VoterState
                {
                    VoterId = voterReg.VoterId,
                    Name = voterReg.Name,
                    IdentificationNumber = voterReg.IdentificationNumber,
                    Status = VotingStatus.Pending
                };
                break;
                
            case VoterStatusUpdated statusUpdated:
                if (_voters.TryGetValue(statusUpdated.VoterId, out var voter))
                {
                    voter.Status = statusUpdated.Status;
                }
                break;
                
            case VoteCast voteCast:
                var votePosition = _positions.Values.FirstOrDefault(p => p.PositionId == voteCast.PositionId);
                if (votePosition != null)
                {
                    var voteId = Guid.NewGuid().ToString();
                    votePosition.Votes[voteId] = new VoteState
                    {
                        VoteId = voteId,
                        VoterId = voteCast.VoterId,
                        CandidateId = voteCast.CandidateId,
                        Time = voteCast.Time
                    };
                }
                break;
                
            case VoteSkipped voteSkipped:
                var skipPosition = _positions.Values.FirstOrDefault(p => p.PositionId == voteSkipped.PositionId);
                if (skipPosition != null)
                {
                    var skipId = Guid.NewGuid().ToString();
                    skipPosition.SkippedVotes[skipId] = new SkippedVoteState
                    {
                        SkippedVoteId = skipId,
                        VoterId = voteSkipped.VoterId,
                        Time = voteSkipped.Time
                    };
                }
                break;
                
            case CandidateAdded candidateAdded:
                var candidatePosition = _positions.Values.FirstOrDefault(p => p.PositionId == candidateAdded.PositionId);
                if (candidatePosition != null)
                {
                    candidatePosition.Candidates[candidateAdded.CandidateId] = new CandidateState
                    {
                        CandidateId = candidateAdded.CandidateId,
                        FirstName = candidateAdded.FirstName,
                        LastName = candidateAdded.LastName,
                        Picture = candidateAdded.Picture,
                        Priority = candidateAdded.Priority
                    };
                }
                break;
                
            case ModeratorAdded modAdded:
                _moderators[modAdded.ModeratorId] = new ModeratorState
                {
                    ModeratorId = modAdded.ModeratorId,
                    Name = modAdded.Name,
                    Badge = modAdded.Badge
                };
                break;
                
            case DepartmentAdded deptAdded:
                _departments[deptAdded.DepartmentId] = new DepartmentState
                {
                    DepartmentId = deptAdded.DepartmentId,
                    Name = deptAdded.Name
                };
                break;
                
            case PollingStationAdded psAdded:
                _pollingStations[psAdded.PollingStationId] = new PollingStationState
                {
                    PollingStationId = psAdded.PollingStationId,
                    Name = psAdded.Name
                };
                break;
        }
        
        _eventCount++;
        
        // Save snapshots periodically
        if (_eventCount % SnapshotInterval == 0)
        {
            SaveSnapshot(new ElectionSnapshot
            {
                ElectionId = _electionId,
                Name = _name,
                Description = _description,
                Logo = _logo,
                BrandColour = _brandColour,
                Positions = new Dictionary<string, PositionState>(_positions),
                Voters = new Dictionary<string, VoterState>(_voters),
                Moderators = new Dictionary<string, ModeratorState>(_moderators),
                Departments = new Dictionary<string, DepartmentState>(_departments),
                PollingStations = new Dictionary<string, PollingStationState>(_pollingStations),
                EventCount = _eventCount
            });
        }
    }
}
