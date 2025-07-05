using Akka.Actor;
using Akka.Bank.Console.Utility;
using Univoting.Akka.Messages;
using Univoting.Akka.Actors.MessageExtractors;

namespace Univoting.Akka.Actors;

public class VotingSupervisorActor : UntypedActor
{
    private readonly IActorRef _electionsParent;

    public VotingSupervisorActor()
    {
        _electionsParent = Context.ActorOf(
            GenericChildPerEntityParent.Props(
                new ElectionMessageExtractor(),
                id => Props.Create(() => new ElectionActor(id))
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
