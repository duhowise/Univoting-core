namespace Univoting.Akka.Messages;

public record ElectionUpdated(string ElectionId, string? Name, string? Description, byte[]? Logo, string? BrandColour) : VotingEvent;