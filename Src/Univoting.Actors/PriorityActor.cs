using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class PriorityActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"priority-{_priorityId}";
        private Guid _priorityId;
        private int _number;

        public PriorityActor()
        {
            Command<CreatePriority>(cmd =>
            {
                Persist(new PriorityCreated(cmd.PriorityId, cmd.Number), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new PriorityDetails(_priorityId, _number));
                });
            });

            Command<GetPriority>(cmd =>
            {
                Sender.Tell(new PriorityDetails(_priorityId, _number));
            });

            Recover<PriorityCreated>(Apply);
        }

        private void Apply(PriorityCreated evt)
        {
            _priorityId = evt.PriorityId;
            _number = evt.Number;
        }
    }
}
