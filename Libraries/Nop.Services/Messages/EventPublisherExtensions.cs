﻿using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Core;

namespace Nop.Services.Messages
{
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Publishes the newsletter subscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterSubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailSubscribedEvent(email));
        }

        public static void EntityTokensAdded<T, U>(this IEventPublisher eventPublisher, T entity, System.Collections.Generic.IList<U> tokens) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityTokensAddedEvent<T, U>(entity, tokens));
        }

        /// <summary>
        /// Publishes the newsletter unsubscribe event.
        /// </summary>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="email">The email.</param>
        public static void PublishNewsletterUnsubscribe(this IEventPublisher eventPublisher, string email)
        {
            eventPublisher.Publish(new EmailUnsubscribedEvent(email));
        }
    }
}