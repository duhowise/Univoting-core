using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class PositionActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"position-{_positionId}";
        private Guid _positionId;
        private string _name;
        private Guid _rankId;
        private Guid _electionId;
        private Guid? _priorityId;

        // Required actor reference for Priority
        private readonly IRequiredActor<PriorityActor> _priorityActor;

        public PositionActor(IRequiredActor<PriorityActor> priorityActor = null)
        {
            _priorityActor = priorityActor;

            Command<CreatePosition>(cmd =>
            {
                if (cmd.PriorityId.HasValue && _priorityActor == null)
                {
                    Sender.Tell(new Status.Failure(new System.Exception("PriorityActor reference is required.")));
                    return;
                }
                Persist(new PositionCreated(cmd.PositionId, cmd.Name, cmd.RankId, cmd.ElectionId, cmd.PriorityId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new PositionDetails(_positionId, _name, _rankId, _electionId, _priorityId));
                });
            });

            Command<GetPosition>(cmd =>
            {
                Sender.Tell(new PositionDetails(_positionId, _name, _rankId, _electionId, _priorityId));
            });

            Recover<PositionCreated>(Apply);
        }

        private void Apply(PositionCreated evt)
        {
            _positionId = evt.PositionId;
            _name = evt.Name;
            _rankId = evt.RankId;
            _electionId = evt.ElectionId;
            _priorityId = evt.PriorityId;
        }
    }
}
