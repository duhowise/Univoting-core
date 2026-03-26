using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class VoterActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"voter-{_voterId}";
        private Guid _voterId;
        private string _name;
        private string _identificationNumber;
        private VotingStatus _status;
        private string _pin;
        private string _faculty;
        private List<Guid> _skippedVoteIds = new();
        private List<Guid> _voteIds = new();
        private HashSet<Guid> _votedPositionIds = new();
        private List<Guid> _availablePositionIds = new();
        private int _currentPositionIndex = 0;
        private bool _authenticated = false;

        public VoterActor()
        {
            Command<CreateVoter>(cmd =>
            {
                Persist(new VoterCreated(cmd.VoterId, cmd.Name, cmd.IdentificationNumber, cmd.Status), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new VoterDetails(_voterId, _name, _identificationNumber, _status));
                });
            });

            Command<AuthenticateVoter>(cmd =>
            {
                if (cmd.PIN == _pin)
                {
                    _authenticated = true;
                    Sender.Tell(new AuthenticationResult(true, "Authentication successful."));
                }
                else
                {
                    Sender.Tell(new AuthenticationResult(false, "Invalid PIN."));
                }
            });

            Command<StartVoting>(cmd =>
            {
                if (!_authenticated)
                {
                    Sender.Tell(new VotingError("Voter not authenticated."));
                    return;
                }
                // Filter positions by faculty
                _availablePositionIds = cmd.PositionIdsByFaculty;
                _currentPositionIndex = 0;
                ProceedToNextPosition();
            });

            Command<ConfirmVote>(cmd =>
            {
                if (!_authenticated)
                {
                    Sender.Tell(new VotingError("Voter not authenticated."));
                    return;
                }
                if (_votedPositionIds.Contains(cmd.PositionId))
                {
                    Sender.Tell(new VotingError("You have already voted for this position."));
                    return;
                }
                // Confirm vote
                _voteIds.Add(cmd.VoteId);
                _votedPositionIds.Add(cmd.PositionId);
                // Publish to event stream for LiveViewActor
                Context.System.EventStream.Publish(cmd);
                Sender.Tell(new VoteConfirmed(cmd.PositionId, cmd.VoteId));
                ProceedToNextPosition();
            });

            Command<ConfirmSkip>(cmd =>
            {
                if (!_authenticated)
                {
                    Sender.Tell(new VotingError("Voter not authenticated."));
                    return;
                }
                if (_votedPositionIds.Contains(cmd.PositionId))
                {
                    Sender.Tell(new VotingError("You have already voted or skipped this position."));
                    return;
                }
                _skippedVoteIds.Add(cmd.SkippedVoteId);
                _votedPositionIds.Add(cmd.PositionId);
                // Publish to event stream for LiveViewActor
                Context.System.EventStream.Publish(cmd);
                Sender.Tell(new SkipConfirmed(cmd.PositionId, cmd.SkippedVoteId));
                ProceedToNextPosition();
            });

            Command<GetVoter>(cmd =>
            {
                Sender.Tell(new VoterDetails(_voterId, _name, _identificationNumber, _status));
            });

            Recover<VoterCreated>(Apply);
        }

        private void ProceedToNextPosition()
        {
            if (_currentPositionIndex < _availablePositionIds.Count)
            {
                var nextPositionId = _availablePositionIds[_currentPositionIndex++];
                Sender.Tell(new NextPosition(nextPositionId));
            }
            else
            {
                _status = VotingStatus.Voted;
                Sender.Tell(new VotingCompleted());
            }
        }

        private void Apply(VoterCreated evt)
        {
            _voterId = evt.VoterId;
            _name = evt.Name;
            _identificationNumber = evt.IdentificationNumber;
            _status = evt.Status;
            // For demo: set PIN and faculty here or via another command/event
            _pin = "1234";
            _faculty = "Science";
        }
    }
}
