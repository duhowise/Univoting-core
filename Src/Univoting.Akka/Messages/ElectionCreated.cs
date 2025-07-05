namespace Univoting.Akka.Messages;

public record ElectionCreated(string ElectionId, string Name, string Description, byte[]? Logo, string? BrandColour) : VotingEvent;