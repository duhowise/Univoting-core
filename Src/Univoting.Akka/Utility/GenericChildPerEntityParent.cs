// -----------------------------------------------------------------------
//  <copyright file="GenericChildPerEntityParent.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using ActorProps = Akka.Actor.Props;

namespace Univoting.Akka.Utility;

/// <summary>
/// A generic "child per entity" parent actor.
/// </summary>
/// <remarks>
/// Intended for simplifying unit tests where we don't want to use Akka.Cluster.Sharding.
/// </remarks>
public sealed class GenericChildPerEntityParent : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
    public static Props Props(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        return ActorProps.Create(() => new GenericChildPerEntityParent(extractor, propsFactory));
    }
    
    /*
     * Re-use Akka.Cluster.Sharding's infrastructure here to keep things simple.
     */
    private readonly IMessageExtractor _extractor;
    private readonly Func<string, Props> _propsFactory;

    public GenericChildPerEntityParent(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _propsFactory = propsFactory ?? throw new ArgumentNullException(nameof(propsFactory));
        
        ReceiveAny(HandleMessage);
    }
    
    private void HandleMessage(object message)
    {
        try
        {
            var entityId = _extractor.EntityId(message);
            if (string.IsNullOrEmpty(entityId))
            {
                _log.Warning("Unable to extract entity ID from message {MessageType}. Message will be dropped.", message.GetType().Name);
                Sender.Tell(new Status.Failure(new InvalidOperationException(
                    $"Unable to extract entity ID from message of type {message.GetType().Name}")));
                return;
            }

            var entityMessage = _extractor.EntityMessage(message);
            if (entityMessage == null)
            {
                _log.Warning("Unable to extract entity message from {MessageType} for entity {EntityId}. Message will be dropped.", message.GetType().Name, entityId);
                Sender.Tell(new Status.Failure(new InvalidOperationException(
                    $"Unable to extract entity message from {message.GetType().Name}")));
                return;
            }

            var childActor = Context.Child(entityId).GetOrElse(() => 
            {
                _log.Debug("Creating new child actor for entity {EntityId}", entityId);
                return Context.ActorOf(_propsFactory(entityId), entityId);
            });

            childActor.Forward(entityMessage);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error processing message {MessageType}: {ErrorMessage}", message.GetType().Name, ex.Message);
            Sender.Tell(new Status.Failure(ex));
        }
    }
}