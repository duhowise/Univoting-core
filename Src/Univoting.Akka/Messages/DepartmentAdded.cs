namespace Univoting.Akka.Messages;

public record DepartmentAdded(Guid ElectionId, string DepartmentId, string Name) : VotingEvent;