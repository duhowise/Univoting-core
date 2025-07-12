using Akka.Actor;
using Univoting.Akka.Utility;
using Univoting.Akka.Messages;
using Univoting.Akka.Actors.MessageExtractors;

namespace Univoting.Akka.Actors;

/// <summary>
/// Simple voting supervisor actor.
/// DEPRECATED: Use EnhancedVotingSupervisorActor for new implementations.
/// This actor routes all commands to ElectionActor instances only.
/// </summary>
[Obsolete("Use EnhancedVotingSupervisorActor for better separation of concerns and scalability")]
public class VotingSupervisorActor : UntypedActor
{
    private readonly IActorRef _electionsParent;

    public VotingSupervisorActor()
    {
        _electionsParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new ElectionMessageExtractor(),
                id => Props.Create(() => new ElectionActor(Guid.Parse(id)))
            ),
            "elections-parent");
    }

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case VotingCommand:
                _electionsParent.Forward(message);
                break;
            default:
                Unhandled(message);
                break;
        }
    }
}
