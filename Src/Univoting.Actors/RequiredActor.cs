using Akka.Actor;

namespace Univoting.Actors
{
    public class RequiredActor<TActor> : IRequiredActor<TActor> where TActor : class
    {
        public IActorRef Ref { get; }
        public RequiredActor(IActorRef actorRef) => Ref = actorRef;
    }
}
