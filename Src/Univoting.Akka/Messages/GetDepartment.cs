namespace Univoting.Akka.Messages;

public record GetDepartment(string DepartmentId) : VotingCommand;