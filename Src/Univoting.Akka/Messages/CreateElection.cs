namespace Univoting.Akka.Messages;

public record CreateElection(string ElectionId, string Name, string Description, byte[]? Logo, string? BrandColour) : VotingCommand;