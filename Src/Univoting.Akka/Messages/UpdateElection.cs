namespace Univoting.Akka.Messages;

public record UpdateElection(string ElectionId, string? Name, string? Description, byte[]? Logo, string? BrandColour) : VotingCommand;