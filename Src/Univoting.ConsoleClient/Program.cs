using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Univoting.Actors;

namespace Univoting.ConsoleClient
{
    class Program
    {
        public static async Task Main(string[] args)
            // Register LiveViewActor for real-time dashboard
            var liveViewActor = system.ActorOf(Props.Create(() => new LiveViewActor()), "liveview");
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddAkka("UnivotingSystem", (akka, provider) =>
                    {
                        akka.WithActors((system, registry) =>
                        {
                            // Register GenericChildPerEntityParent for each actor type
                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.CandidateMessageExtractor(),
                                    id => provider.Props<CandidateActor>()),
                                "candidate-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.VoterMessageExtractor(),
                                    id => provider.Props<VoterActor>()),
                                "voter-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.VoteMessageExtractor(),
                                    id => provider.Props<VoteActor>()),
                                "vote-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.SkippedVoteMessageExtractor(),
                                    id => provider.Props<SkippedVoteActor>()),
                                "skippedvote-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.ModeratorMessageExtractor(),
                                    id => provider.Props<ModeratorActor>()),
                                "moderator-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.PositionMessageExtractor(),
                                    id => provider.Props<PositionActor>()),
                                "position-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.DepartmentMessageExtractor(),
                                    id => provider.Props<DepartmentActor>()),
                                "department-parent");

                            system.ActorOf(
                                Univoting.Actors.Utility.GenericChildPerEntityParent.Props(
                                    new Univoting.Actors.Utility.PollingStationMessageExtractor(),
                                    id => provider.Props<PollingStationActor>()),
                                "pollingstation-parent");

                            // LiveViewActor registration
                            system.ActorOf(Akka.Actor.Props.Create(() => new LiveViewActor()), "liveview");
                        });
                    });
                });

            await builder.RunConsoleAsync();
        }
    }
}
