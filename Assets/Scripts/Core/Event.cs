using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Event<T>
{
    private readonly HashSet<Action<T>> listeners = new();

    public void AddListener(Action<T> listener)
    {
        listeners.Add(listener);
        Logger.Info($"Added listener to {GetType().Name}, total listeners: {listeners.Count}");
    }

    public void RemoveListener(Action<T> listener)
    {
        listeners.Remove(listener);
        Logger.Info($"Removed listener from {GetType().Name}, total listeners: {listeners.Count}");
    }

    public void ClearListeners()
    {
        listeners.Clear();
        Logger.Info($"Cleared all listeners from {GetType().Name}");
    }

    public void Invoke(T data)
    {
        Logger.Info($"Invoking {GetType().Name} with {listeners.Count} listeners");
        var listeners2 = this.listeners.ToList();
        foreach (var listener in listeners2)
        {
            listener.Invoke(data);
        }
    }
}