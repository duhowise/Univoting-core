using System;
using System.Collections.Generic;
using Akka.Actor;
using Univoting.Models;
using Univoting.Models.Messages;

namespace Univoting.Actors
{
    public class ElectionActor : ReceivePersistentActor
    {
        public override string PersistenceId => $"election-{_electionId}";
        private Guid _electionId;
        private string _name;
        private string _description;
        private readonly List<Guid> _voters = new();
        private readonly List<Guid> _positions = new();
        private readonly List<VoterDetails> _pendingVoters = new();
        private readonly Dictionary<string, VoterDetails> _voterIndex = new();
        private readonly List<AdminAccount> _admins = new();

        private ICancelable _liveViewQueryTask;

        public ElectionActor()
        {
            // Periodically query LiveViewActor for stats
            var liveView = Context.ActorSelection("/user/liveview");
            _liveViewQueryTask = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), liveView, new Univoting.Actors.GetLiveStats(), Self);

            Command<CreateElection>(cmd =>
            {
                Persist(new ElectionCreated(cmd.ElectionId, cmd.Name, cmd.Description), evt =>
                {
                    Apply(evt);
                    Sender.Tell(new ElectionDetails(cmd.ElectionId, cmd.Name, cmd.Description));
                });
            });

            Command<ImportVotersFromExcel>(cmd =>
            {
                // Simulate Excel parsing: extract (FullName, IndexNumber, Faculty), generate random password
                // In production, delegate to a service
                var imported = new List<VoterDetails>();
                // ...parse cmd.ExcelFile...
                // For demo, add a fake entry
                imported.Add(new VoterDetails(Guid.NewGuid(), "John Doe", "123456", VotingStatus.Pending));
                _pendingVoters.AddRange(imported);
                Sender.Tell(new BulkVoterImportResult(true, $"Imported {imported.Count} voters."));
            });

            Command<AddManualVoter>(cmd =>
            {
                var voter = new VoterDetails(Guid.NewGuid(), cmd.FullName, cmd.IndexNumber, VotingStatus.Pending);
                _pendingVoters.Add(voter);
                Sender.Tell(new BulkVoterImportResult(true, "Manual voter added."));
            });

            Command<GetPendingVoterImports>(_ =>
            {
                Sender.Tell(new PendingVoterList(_pendingVoters));
            });

            Command<ConfirmBulkVoterSave>(_ =>
            {
                foreach (var voter in _pendingVoters)
                {
                    if (_voterIndex.ContainsKey(voter.IdentificationNumber))
                        continue;
                    _voterIndex[voter.IdentificationNumber] = voter;
                    _voters.Add(voter.VoterId);
                }
                _pendingVoters.Clear();
                Sender.Tell(new BulkVoterImportResult(true, "All voters saved."));
            });

            Command<FindVoter>(cmd =>
            {
                var found = _voterIndex.Values.FirstOrDefault(v => v.Name == cmd.SearchTerm || v.IdentificationNumber == cmd.SearchTerm);
                if (found != null)
                    Sender.Tell(new VoterFound(found.Name, found.IdentificationNumber, "Faculty?", "password123"));
                else
                    Sender.Tell(new VoterNotFound("Voter not found."));
            });

            Command<ResetVoterPassword>(cmd =>
            {
                if (_voterIndex.TryGetValue(cmd.IndexNumber, out var voter))
                {
                    var newPassword = Guid.NewGuid().ToString("N").Substring(0, 8);
                    // In production, update password in voter record
                    Sender.Tell(new PasswordResetResult(true, newPassword, "Password reset."));
                }
                else
                {
                    Sender.Tell(new PasswordResetResult(false, null, "Voter not found."));
                }
            });

            Command<GetAllVoters>(_ =>
            {
                Sender.Tell(new AllVotersList(_voterIndex.Values.ToList()));
            });

            Command<CreateAdminAccount>(cmd =>
            {
                // Only one Chairman or President
                if ((cmd.Role == "Chairman" || cmd.Role == "President") && _admins.Any(a => a.Role == cmd.Role))
                {
                    Sender.Tell(new AdminAccountCreated(false, $"{cmd.Role} already exists."));
                    return;
                }
                _admins.Add(new AdminAccount(cmd.FullName, cmd.Username, cmd.Password, cmd.Role));
                Sender.Tell(new AdminAccountCreated(true, "Admin account created."));
            });

            Command<AuthenticateAdmin>(cmd =>
            {
                var admin = _admins.FirstOrDefault(a => a.Username == cmd.Username && a.Password == cmd.Password);
                if (admin != null)
                    Sender.Tell(new AdminAuthenticationResult(true, "Login successful."));
                else
                    Sender.Tell(new AdminAuthenticationResult(false, "Invalid credentials."));
            });

            Command<AddPosition>(cmd =>
            {
                Persist(new PositionAdded(cmd.ElectionId, cmd.PositionId, cmd.Name), evt =>
                {
                    Apply(evt);
                });
            });

            Command<GetElection>(cmd =>
            {
                Sender.Tell(new ElectionDetails(_electionId, _name, _description));
            });

            Recover<ElectionCreated>(Apply);
            Recover<VoterAdded>(Apply);
            Recover<PositionAdded>(Apply);

        // Helper admin class
        private record AdminAccount(string FullName, string Username, string Password, string Role);
        }

        private void Apply(ElectionCreated evt)
        {
            _electionId = evt.ElectionId;
            _name = evt.Name;
            _description = evt.Description;
        }
        private void Apply(VoterAdded evt)
        {
            _voters.Add(evt.VoterId);
        }
        private void Apply(PositionAdded evt)
        {
            _positions.Add(evt.PositionId);
        }
    }
}
