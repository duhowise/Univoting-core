using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class DepartmentActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"department-{_departmentId}";
        private Guid _departmentId;
        private string _name;
        private Guid _electionId;

        public DepartmentActor()
        {
            Command<CreateDepartment>(cmd =>
            {
                Persist(new DepartmentCreated(cmd.DepartmentId, cmd.Name, cmd.ElectionId), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new DepartmentDetails(_departmentId, _name, _electionId));
                });
            });

            Command<GetDepartment>(cmd =>
            {
                Sender.Tell(new DepartmentDetails(_departmentId, _name, _electionId));
            });

            Recover<DepartmentCreated>(Apply);
        }

        private void Apply(DepartmentCreated evt)
        {
            _departmentId = evt.DepartmentId;
            _name = evt.Name;
            _electionId = evt.ElectionId;
        }
    }
}
