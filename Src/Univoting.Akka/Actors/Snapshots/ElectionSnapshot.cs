using Univoting.Akka.Actors.States;

namespace Univoting.Akka.Actors.Snapshots;

public class ElectionSnapshot
{
    public Guid ElectionId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[]? Logo { get; set; }
    public string? BrandColour { get; set; }
    public Dictionary<string, PositionState> Positions { get; set; } = new();
    public Dictionary<string, VoterState> Voters { get; set; } = new();
    public Dictionary<string, ModeratorState> Moderators { get; set; } = new();
    public Dictionary<string, DepartmentState> Departments { get; set; } = new();
    public Dictionary<string, PollingStationState> PollingStations { get; set; } = new();
    public int EventCount { get; set; }
}
