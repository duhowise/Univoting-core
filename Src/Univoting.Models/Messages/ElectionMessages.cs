using System;

namespace Univoting.Models.Messages
{
    // Commands
    public record CreateElection(Guid ElectionId, string Name, string Description);
    public record AddPosition(Guid ElectionId, Guid PositionId, string Name);
    public record ImportVotersFromExcel(byte[] ExcelFile);
    public record AddManualVoter(string FullName, string IndexNumber, string Faculty);
    public record ConfirmBulkVoterSave();
    public record ResetVoterPassword(string IndexNumber);
    public record CreateAdminAccount(string FullName, string Username, string Password, string Role);
    public record AuthenticateAdmin(string Username, string Password);
    // ... add more as needed

    // Events
    public record ElectionCreated(Guid ElectionId, string Name, string Description);
    public record VoterAdded(Guid ElectionId, Guid VoterId, string Name, string IdentificationNumber, string Faculty, string Password);
    public record PositionAdded(Guid ElectionId, Guid PositionId, string Name);
    public record AdminAccountCreated(bool Success, string Message);
    // ... add more as needed

    // Queries
    public record GetElection(Guid ElectionId);
    public record GetElectionResult(Guid ElectionId);
    public record GetPendingVoterImports();
    public record FindVoter(string SearchTerm);
    public record GetAllVoters();
    // ... add more as needed

    // Responses
    public record ElectionDetails(Guid ElectionId, string Name, string Description);
    public record BulkVoterImportResult(bool Success, string Message);
    public record PendingVoterList(List<VoterDetails> Voters);
    public record VoterFound(string FullName, string IndexNumber, string Faculty, string Password);
    public record VoterNotFound(string Message);
    public record PasswordResetResult(bool Success, string Password, string Message);
    public record AdminAuthenticationResult(bool Success, string Message);
    public record AllVotersList(List<VoterDetails> Voters);
    // ... add more as needed
}
