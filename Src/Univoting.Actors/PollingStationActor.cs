using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class PollingStationActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"pollingstation-{_pollingStationId}";
        private Guid _pollingStationId;
        private string _name;
        private Guid _electionId;

        public PollingStationActor()
        {
            Command<CreatePollingStation>(cmd =>
            {
                Persist(new PollingStationCreated(cmd.PollingStationId, cmd.Name, cmd.ElectionId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new PollingStationDetails(_pollingStationId, _name, _electionId));
                });
            });

            Command<GetPollingStation>(cmd =>
            {
                Sender.Tell(new PollingStationDetails(_pollingStationId, _name, _electionId));
            });

            Recover<PollingStationCreated>(Apply);
        }

        private void Apply(PollingStationCreated evt)
        {
            _pollingStationId = evt.PollingStationId;
            _name = evt.Name;
            _electionId = evt.ElectionId;
        }
    }
}
