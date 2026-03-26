using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateDepartment(Guid DepartmentId, string Name, Guid ElectionId);
    // Events
    public record DepartmentCreated(Guid DepartmentId, string Name, Guid ElectionId);
    // Queries
    public record GetDepartment(Guid DepartmentId);
    // Responses
    public record DepartmentDetails(Guid DepartmentId, string Name, Guid ElectionId);
}
