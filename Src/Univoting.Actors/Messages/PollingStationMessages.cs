using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreatePollingStation(Guid PollingStationId, string Name, Guid ElectionId);
    // Events
    public record PollingStationCreated(Guid PollingStationId, string Name, Guid ElectionId);
    // Queries
    public record GetPollingStation(Guid PollingStationId);
    // Responses
    public record PollingStationDetails(Guid PollingStationId, string Name, Guid ElectionId);
}
