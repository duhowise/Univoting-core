namespace Univoting.Akka.Messages;

public record AddDepartment(Guid ElectionId, string DepartmentId, string Name) : VotingCommand;