namespace Univoting.Akka.Messages;

public record ElectionUpdated(Guid ElectionId, string? Name, string? Description, byte[]? Logo, string? BrandColour) : VotingEvent;