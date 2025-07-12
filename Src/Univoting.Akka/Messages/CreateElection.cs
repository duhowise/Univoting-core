namespace Univoting.Akka.Messages;

public record CreateElection(Guid ElectionId, string Name, string Description, byte[]? Logo, string? BrandColour) : VotingCommand;