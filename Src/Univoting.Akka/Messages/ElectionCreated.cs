namespace Univoting.Akka.Messages;

public record ElectionCreated(Guid ElectionId, string Name, string Description, byte[]? Logo, string? BrandColour) : VotingEvent;