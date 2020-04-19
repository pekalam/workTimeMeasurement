﻿using System;
using System.Linq;
using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Modularity;

namespace Infrastructure.Messaging
{
    public class UiEvent<T> : PubSubEvent<T>
    {
        private T _previousPayload;
        private bool _hasPrevious = false;

        public override SubscriptionToken Subscribe(Action<T> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive,
            Predicate<T> filter)
        {
            var token = base.Subscribe(action, threadOption, keepSubscriberReferenceAlive, filter);
            if (_hasPrevious)
            {
                var subscription = Subscriptions.First(sub => sub.SubscriptionToken == token);
                subscription.GetExecutionStrategy()(new object[] { _previousPayload });
            }

            return token;
        }

        public override void Publish(T payload)
        {
            _previousPayload = payload;
            _hasPrevious = true;
            base.Publish(payload);
        }
    }

    public class AppStartedEvent : UiEvent<MetroWindow>
    {
    }

    public class InfrastructureModuleLoaded : UiEvent<IModule>
    {

    }
}
