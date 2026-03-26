using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{

    public class VoteActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"vote-{_voteId}";
        private Guid _voteId;
        private Guid _voterId;
        private Guid _candidateId;
        private DateTime _time;
        private Guid _positionId;

        // Required actor reference for Position
        private readonly IRequiredActor<PositionActor> _positionActor;

        public VoteActor(IRequiredActor<PositionActor> positionActor = null)
        {
            // Use ActorSelection for specific child
            Command<CreateVote>(cmd =>
            {
                // Enforce that Position must exist
                var positionActor = Context.ActorSelection($"/user/position-parent/{cmd.PositionId}");
                // Optionally, send a message to check existence or handle as needed
                Persist(new VoteCreated(cmd.VoteId, cmd.VoterId, cmd.CandidateId, cmd.Time, cmd.PositionId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new VoteDetails(_voteId, _voterId, _candidateId, _time, _positionId));
                });
            });

            Command<GetVote>(cmd =>
            {
                Sender.Tell(new VoteDetails(_voteId, _voterId, _candidateId, _time, _positionId));
            });

            // Respond to parent with position for aggregation
            Command<Univoting.Actors.Messages.GetVotePosition>(_ =>
            {
                Sender.Tell(new Univoting.Actors.Messages.VotePosition(_positionId));
            });

            Recover<VoteCreated>(Apply);
        }

        private void Apply(VoteCreated evt)
        {
            _voteId = evt.VoteId;
            _voterId = evt.VoterId;
            _candidateId = evt.CandidateId;
            _time = evt.Time;
            _positionId = evt.PositionId;
        }
    }
}
