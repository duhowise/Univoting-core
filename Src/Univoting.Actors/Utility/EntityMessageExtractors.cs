using System;
using Akka.Cluster.Sharding;

namespace Univoting.Actors.Utility
{
    // Example extractor for CandidateActor
    public class CandidateMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { CandidateId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    // Example extractor for VoterActor
    public class VoterMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { VoterId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class VoteMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { VoteId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class SkippedVoteMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { SkippedVoteId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class ModeratorMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { ModeratorId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class PositionMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { PositionId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class DepartmentMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { DepartmentId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class PollingStationMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { PollingStationId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }

    public class PriorityMessageExtractor : IMessageExtractor
    {
        public string EntityId(object message)
        {
            return message switch
            {
                { PriorityId: Guid id } => id.ToString(),
                _ => null
            };
        }
        public object EntityMessage(object message) => message;
        public string ShardId(object message) => "1";
    }
}
