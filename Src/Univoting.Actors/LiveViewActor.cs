using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace Univoting.Actors
{
    public class LiveViewActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly Dictionary<Guid, HashSet<Guid>> _votesByPosition = new();
        private readonly Dictionary<Guid, HashSet<Guid>> _skipsByPosition = new();
        private ICancelable _refreshTask;

        public LiveViewActor()
        {
            Receive<GetLiveStats>(_ =>
            {
                // Tally on demand
                var stats = new Dictionary<Guid, (int votes, int skips)>();
                var allPositionIds = _votesByPosition.Keys.Union(_skipsByPosition.Keys).ToHashSet();
                foreach (var posId in allPositionIds)
                {
                    var votes = _votesByPosition.TryGetValue(posId, out var vset) ? vset.Count : 0;
                    var skips = _skipsByPosition.TryGetValue(posId, out var sset) ? sset.Count : 0;
                    stats[posId] = (votes, skips);
                }
                Sender.Tell(new LiveStats(stats));
            });

            // Subscribe to event stream for live updates
            Context.System.EventStream.Subscribe(Self, typeof(Univoting.Actors.Messages.ConfirmVote));
            Context.System.EventStream.Subscribe(Self, typeof(Univoting.Actors.Messages.ConfirmSkip));

            Receive<Univoting.Actors.Messages.ConfirmVote>(msg =>
            {
                if (!_votesByPosition.TryGetValue(msg.PositionId, out var set))
                {
                    set = new HashSet<Guid>();
                    _votesByPosition[msg.PositionId] = set;
                }
                set.Add(msg.VoteId);
            });

            Receive<Univoting.Actors.Messages.ConfirmSkip>(msg =>
            {
                if (!_skipsByPosition.TryGetValue(msg.PositionId, out var set))
                {
                    set = new HashSet<Guid>();
                    _skipsByPosition[msg.PositionId] = set;
                }
                set.Add(msg.SkippedVoteId);
            });
        }

    
        public ITimerScheduler Timers { get; set; }
    }
    public record LiveStats(Dictionary<Guid, (int votes, int skips)> Stats);
}
