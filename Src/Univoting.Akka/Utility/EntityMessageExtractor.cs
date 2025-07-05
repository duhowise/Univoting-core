using Akka.Cluster.Sharding;

namespace Univoting.Akka.Utility;

public abstract class EntityMessageExtractor:IMessageExtractor
{
    protected readonly Func<object, string?> _extractEntityId;
    protected readonly Func<object, object> _extractEntityMessage;
    protected readonly string _shardName;

    protected EntityMessageExtractor(
        Func<object, string?> extractEntityId,
        Func<object, object> extractEntityMessage,
        string shardName)
    {
        _extractEntityId = extractEntityId;
        _extractEntityMessage = extractEntityMessage;
        _shardName = shardName;
    }

    public virtual string? ExtractEntityId(object message) => _extractEntityId(message);
    public virtual object ExtractEntityMessage(object message) => _extractEntityMessage(message);
    public virtual string ShardName => _shardName;
    public string? EntityId(object message)
    {
return _extractEntityId(message);    }

    public object? EntityMessage(object message)
    {
        return _extractEntityMessage(message);
    }

    public string? ShardId(object message)
    {
        return _extractEntityId(message);
    }

    public string ShardId(string entityId, object? messageHint = null)
    {
        return _extractEntityId(entityId)!;
    }
}