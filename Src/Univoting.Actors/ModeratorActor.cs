using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class ModeratorActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"moderator-{_moderatorId}";
        private Guid _moderatorId;
        private string _name;
        private Badge _badge;
        private Guid _electionId;

        public ModeratorActor()
        {
            Command<CreateModerator>(cmd =>
            {
                Persist(new ModeratorCreated(cmd.ModeratorId, cmd.Name, cmd.Badge, cmd.ElectionId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new ModeratorDetails(_moderatorId, _name, _badge, _electionId));
                });
            });

            Command<GetModerator>(cmd =>
            {
                Sender.Tell(new ModeratorDetails(_moderatorId, _name, _badge, _electionId));
            });

            Recover<ModeratorCreated>(Apply);
        }

        private void Apply(ModeratorCreated evt)
        {
            _moderatorId = evt.ModeratorId;
            _name = evt.Name;
            _badge = evt.Badge;
            _electionId = evt.ElectionId;
        }
    }
}
