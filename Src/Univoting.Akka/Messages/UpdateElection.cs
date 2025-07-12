namespace Univoting.Akka.Messages;

public record UpdateElection(Guid ElectionId, string? Name, string? Description, byte[]? Logo, string? BrandColour) : VotingCommand;