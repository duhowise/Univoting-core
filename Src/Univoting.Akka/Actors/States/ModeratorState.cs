using Univoting.Akka.Models;

namespace Univoting.Akka.Actors.States;

public class ModeratorState
{
    public string ModeratorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Badge Badge { get; set; }
}
