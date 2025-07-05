// -----------------------------------------------------------------------
//  <copyright file="GenericChildPerEntityParent.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Cluster.Sharding;

namespace Akka.Bank.Console.Utility;

/// <summary>
/// A generic "child per entity" parent actor.
/// </summary>
/// <remarks>
/// Intended for simplifying unit tests where we don't want to use Akka.Cluster.Sharding.
/// </remarks>
public sealed class GenericChildPerEntityParent : ReceiveActor
{
    public static Props Props(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        return Akka.Actor.Props.Create(() => new GenericChildPerEntityParent(extractor, propsFactory));
    }
    
    /*
     * Re-use Akka.Cluster.Sharding's infrastructure here to keep things simple.
     */
    private readonly IMessageExtractor _extractor;
    private Func<string, Props> _propsFactory;

    public GenericChildPerEntityParent(IMessageExtractor extractor, Func<string, Props> propsFactory)
    {
        _extractor = extractor;
        _propsFactory = propsFactory;
        
        ReceiveAny(o =>
        {
            var result = _extractor.EntityId(o);
            if (result is null) return;
            Context.Child(result).GetOrElse(() => Context.ActorOf(propsFactory(result), result)).Forward(_extractor.EntityMessage(o));
        });
    }
}