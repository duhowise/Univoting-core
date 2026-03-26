using Akka.Actor;
using Akka.Configuration;

namespace Univoting.Actors
{
    public static class AkkaBootstrapper
    {
        public static ActorSystem StartAkka()
        {
            var config = ConfigurationFactory.ParseString(@"
                akka.persistence.journal.plugin = ""akka.persistence.journal.inmem""
                akka.persistence.snapshot-store.plugin = ""akka.persistence.snapshot-store.inmem""
            ");
            var system = ActorSystem.Create("UnivotingSystem", config);
            return system;
        }
    }
}
