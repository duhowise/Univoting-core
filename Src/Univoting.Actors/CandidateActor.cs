using System;
using Akka.Actor;
using Akka.Persistence;
using Univoting.Models;
using Univoting.Actors.Messages;

namespace Univoting.Actors
{
    public class CandidateActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"candidate-{_candidateId}";
        private Guid _candidateId;
        private string _firstName;
        private string _lastName;
        private Guid _positionId;
        private Guid _electionId;
        private int? _priorityNumber;

        public CandidateActor()
        {
            Command<CreateCandidate>(cmd =>
            {
                Persist(new CandidateCreated(cmd.CandidateId, cmd.FirstName, cmd.LastName, cmd.PositionId, cmd.ElectionId, cmd.PriorityNumber), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new CandidateDetails(_candidateId, _firstName, _lastName, _positionId, _electionId, _priorityNumber));
                });
            });

            Command<GetCandidate>(cmd =>
            {
                Sender.Tell(new CandidateDetails(_candidateId, _firstName, _lastName, _positionId, _electionId, _priorityNumber));
            });

            Recover<CandidateCreated>(Apply);
        }

        private void Apply(CandidateCreated evt)
        {
            _candidateId = evt.CandidateId;
            _firstName = evt.FirstName;
            _lastName = evt.LastName;
            _positionId = evt.PositionId;
            _electionId = evt.ElectionId;
            _priorityNumber = evt.PriorityNumber;
        }
    }
}
