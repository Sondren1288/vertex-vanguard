using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseObserver
{
    private readonly List<(object Event, Delegate Handler)> eventSubscriptions = new();

    protected void Subscribe<T>(Event<T> eventObject, Action<T> handler)
    {
        Logger.Info($"Subscribing to event: {eventObject.GetType()}");
        eventObject.AddListener(handler);
        eventSubscriptions.Add((eventObject, handler));
    }

    public virtual void Initialize()
    {
        Logger.Info($"Initializing observer: {GetType().Name}");
        RegisterEvents();
    }

    public virtual void Cleanup()
    {
        Logger.Success($"Cleaning up observer: {GetType().Name}");
        foreach (var subscription in eventSubscriptions)
        {
            // Get the generic Event<T> type
            var eventType = subscription.Event.GetType();
            var handlerType = subscription.Handler.GetType();

            // Use reflection to call RemoveListener with the correct types
            var removeListenerMethod = eventType.GetMethod("RemoveListener");
            if (removeListenerMethod != null)
            {
                removeListenerMethod.Invoke(subscription.Event, new[] { subscription.Handler });
            }
        }
        eventSubscriptions.Clear();
    }

    protected abstract void RegisterEvents();
}