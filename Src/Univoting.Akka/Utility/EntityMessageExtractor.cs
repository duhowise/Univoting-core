using Akka.Cluster.Sharding;

namespace Univoting.Akka.Utility;

public abstract class EntityMessageExtractor : IMessageExtractor
{
    protected readonly Func<object, string?> _extractEntityId;
    protected readonly Func<object, object> _extractEntityMessage;
    protected readonly string _shardName;

    protected EntityMessageExtractor(
        Func<object, string?> extractEntityId,
        Func<object, object> extractEntityMessage,
        string shardName)
    {
        _extractEntityId = extractEntityId ?? throw new ArgumentNullException(nameof(extractEntityId));
        _extractEntityMessage = extractEntityMessage ?? throw new ArgumentNullException(nameof(extractEntityMessage));
        _shardName = shardName ?? throw new ArgumentNullException(nameof(shardName));
    }

    public virtual string? ExtractEntityId(object message) => _extractEntityId(message);
    public virtual object ExtractEntityMessage(object message) => _extractEntityMessage(message);
    public virtual string ShardName => _shardName;
    
    public string? EntityId(object message)
    {
        try
        {
            return _extractEntityId(message);
        }
        catch (Exception)
        {
            // Log error in production systems
            return null;
        }
    }

    public object? EntityMessage(object message)
    {
        try
        {
            return _extractEntityMessage(message);
        }
        catch (Exception)
        {
            // Log error in production systems
            return null;
        }
    }

    public string? ShardId(object message)
    {
        var entityId = EntityId(message);
        return string.IsNullOrEmpty(entityId) ? null : entityId;
    }

    public string ShardId(string entityId, object? messageHint = null)
    {
        return !string.IsNullOrEmpty(entityId) ? entityId : string.Empty;
    }
}