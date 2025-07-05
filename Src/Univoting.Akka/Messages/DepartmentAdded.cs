namespace Univoting.Akka.Messages;

public record DepartmentAdded(string ElectionId, string DepartmentId, string Name) : VotingEvent;