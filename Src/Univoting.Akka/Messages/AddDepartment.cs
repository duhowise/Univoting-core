namespace Univoting.Akka.Messages;

public record AddDepartment(string ElectionId, string DepartmentId, string Name) : VotingCommand;