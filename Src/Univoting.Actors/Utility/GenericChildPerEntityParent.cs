using System;
using Akka.Actor;
using Akka.Cluster.Sharding;

namespace Univoting.Actors.Utility
{
    /// <summary>
    /// A generic "child per entity" parent actor for local/test use.
    /// </summary>
    public sealed class GenericChildPerEntityParent : ReceiveActor
    {
        public static Props Props(IMessageExtractor extractor, Func<string, Props> propsFactory)
        {
            return Akka.Actor.Props.Create(() => new GenericChildPerEntityParent(extractor, propsFactory));
        }

        private readonly IMessageExtractor _extractor;
        private readonly Func<string, Props> _propsFactory;

        public GenericChildPerEntityParent(IMessageExtractor extractor, Func<string, Props> propsFactory)

            // Special handling for skipped vote aggregation
            Receive<Univoting.Actors.Messages.GetSkippedVoteCountsPerPosition>(msg =>
            {
                var children = Context.GetChildren();
                var originalSender = Sender;
                var positionCounts = new Dictionary<Guid, int>();
                int expected = 0, received = 0;

                if (!children.Any())
                {
                    originalSender.Tell(new Univoting.Actors.Messages.SkippedVoteCountsPerPosition(positionCounts));
                    return;
                }

                Context.System.Scheduler.ScheduleTellOnceCancelable(
                    TimeSpan.FromSeconds(3), Self, new AggregationTimeout(originalSender), Self);

                expected = children.Count();

                void HandleResponse(Univoting.Actors.Messages.SkippedVotePosition resp)
                {
                    if (positionCounts.ContainsKey(resp.PositionId))
                        positionCounts[resp.PositionId]++;
                    else
                        positionCounts[resp.PositionId] = 1;
                    received++;
                    if (received == expected)
                    {
                        originalSender.Tell(new Univoting.Actors.Messages.SkippedVoteCountsPerPosition(positionCounts));
                        Context.UnbecomeStacked();
                    }
                }

                void HandleTimeout(AggregationTimeout timeout)
                {
                    if (timeout.OriginalSender == originalSender)
                    {
                        originalSender.Tell(new Univoting.Actors.Messages.SkippedVoteCountsPerPosition(positionCounts));
                        Context.UnbecomeStacked();
                    }
                }

                BecomeStacked(() =>
                {
                    Receive<Univoting.Actors.Messages.SkippedVotePosition>(resp => HandleResponse(resp));
                    Receive<AggregationTimeout>(timeout => HandleTimeout(timeout));
                    ReceiveAny(_ => { });
                });

                foreach (var child in children)
                {
                    child.Tell(new Univoting.Actors.Messages.GetSkippedVotePosition());
                }
            });
        {
            _extractor = extractor;
            _propsFactory = propsFactory;

            // Special handling for vote aggregation
            Receive<Univoting.Actors.Messages.GetVoteCountsPerPosition>(msg =>
            {
                var children = Context.GetChildren();
                var originalSender = Sender;
                var positionCounts = new Dictionary<Guid, int>();
                int expected = 0, received = 0;

                if (!children.Any())
                {
                    originalSender.Tell(new Univoting.Actors.Messages.VoteCountsPerPosition(positionCounts));
                    return;
                }

                Context.System.Scheduler.ScheduleTellOnceCancelable(
                    TimeSpan.FromSeconds(3), Self, new AggregationTimeout(originalSender), Self);

                expected = children.Count();

                void HandleResponse(Univoting.Actors.Messages.VotePosition resp)
                {
                    if (positionCounts.ContainsKey(resp.PositionId))
                        positionCounts[resp.PositionId]++;
                    else
                        positionCounts[resp.PositionId] = 1;
                    received++;
                    if (received == expected)
                    {
                        originalSender.Tell(new Univoting.Actors.Messages.VoteCountsPerPosition(positionCounts));
                        Context.UnbecomeStacked();
                    }
                }

                void HandleTimeout(AggregationTimeout timeout)
                {
                    if (timeout.OriginalSender == originalSender)
                    {
                        originalSender.Tell(new Univoting.Actors.Messages.VoteCountsPerPosition(positionCounts));
                        Context.UnbecomeStacked();
                    }
                }

                BecomeStacked(() =>
                {
                    Receive<Univoting.Actors.Messages.VotePosition>(resp => HandleResponse(resp));
                    Receive<AggregationTimeout>(timeout => HandleTimeout(timeout));
                    ReceiveAny(_ => { });
                });

                foreach (var child in children)
                {
                    child.Tell(new Univoting.Actors.Messages.GetVotePosition());
                }
            });

            // Default: forward to child
            ReceiveAny(o =>
            {
                var result = _extractor.EntityId(o);
                if (result is null) return;
                Context.Child(result).GetOrElse(() => Context.ActorOf(_propsFactory(result), result)).Forward(_extractor.EntityMessage(o));
            });
        }

        // Helper for aggregation timeout
        private record AggregationTimeout(IActorRef OriginalSender);
    }
}
