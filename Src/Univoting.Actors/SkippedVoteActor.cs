using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class SkippedVoteActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"skippedvote-{_skippedVoteId}";
        private Guid _skippedVoteId;
        private Guid _voterId;
        private DateTime _time;
        private Guid _positionId;

        public SkippedVoteActor()

        {
            // Use ActorSelection for specific child
            Command<CreateSkippedVote>(cmd =>
            {
                var positionActor = Context.ActorSelection($"/user/position-parent/{cmd.PositionId}");
                Persist(new SkippedVoteCreated(cmd.SkippedVoteId, cmd.VoterId, cmd.Time, cmd.PositionId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new SkippedVoteDetails(_skippedVoteId, _voterId, _time, _positionId));
                });
            });

            Command<GetSkippedVote>(cmd =>
            {
                Sender.Tell(new SkippedVoteDetails(_skippedVoteId, _voterId, _time, _positionId));
            });

            // Respond to parent with position for aggregation
            Command<Univoting.Actors.Messages.GetSkippedVotePosition>(_ =>
            {
                Sender.Tell(new Univoting.Actors.Messages.SkippedVotePosition(_positionId));
            });

            Recover<SkippedVoteCreated>(Apply);
        }

        private void Apply(SkippedVoteCreated evt)
        {
            _skippedVoteId = evt.SkippedVoteId;
            _voterId = evt.VoterId;
            _time = evt.Time;
            _positionId = evt.PositionId;
        }
    }
}
