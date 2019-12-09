﻿using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using Helsenorge.Messaging.Abstractions;

namespace Helsenorge.Messaging.ServiceBus
{
    internal static class ServiceBusLoggingExtensions
    {
        private static readonly Action<ILogger, QueueType, string, int, int, string, Exception> StartReceive;
        private static readonly Action<ILogger, QueueType, string, int, int, string, Exception> EndReceive;
        private static readonly Action<ILogger, QueueType, string, int, int, string, string, Exception> StartSend;
        private static readonly Action<ILogger, QueueType, string, int, int, string, string, Exception> EndSend;
        private static readonly Action<ILogger, string, int, int, string, string, Exception> ResponseTime;
        private static readonly Action<ILogger, QueueType, string, int, Exception> LogTimeout;

        private static readonly Action<ILogger, string, string, int, int, string, Exception> BeforeNotificationHandler;
        private static readonly Action<ILogger, string, string, int, int, string, Exception> AfterNotificationHandler;

        private static readonly Action<ILogger, string, string, string, string, int, string, Exception> BeforeValidatingCertificate;
        private static readonly Action<ILogger, string, string, string, string, int, string, Exception> AfterValidatingCertificate;

        private static readonly Action<ILogger, string, string, string, int, int, string, Exception> BeforeEncryptingPayload;
        private static readonly Action<ILogger, string, string, string, int, int, string, Exception> AfterEncryptingPayload;

        private static readonly Action<ILogger, string, int, int, string, Exception> BeforeFactoryPoolCreateMessage;
        private static readonly Action<ILogger, string, int, int, string, Exception> AfterFactoryPoolCreateMessage;

        private static readonly Action<ILogger, string, Exception> ExternalReportedError;
        private static readonly Action<ILogger, string, Exception> RemoveMessageFromQueueNormal;
        private static readonly Action<ILogger, string, Exception> RemoveMessageFromQueueError;

        public static void LogStartReceive(this ILogger logger, QueueType queueType, IncomingMessage message)
        {
            StartReceive(logger, queueType, message.MessageFunction, message.FromHerId, message.ToHerId, message.MessageId, null);
        }
        public static void LogEndReceive(this ILogger logger, QueueType queueType, IncomingMessage message)
        {
            EndReceive(logger, queueType, message.MessageFunction, message.FromHerId, message.ToHerId, message.MessageId, null);
        }

        public static void LogStartSend(this ILogger logger, QueueType queueType, string function, int fromHerId, int toHerId, string messageId, string userId, XDocument xml)
        {
            StartSend(logger, queueType, function, fromHerId, toHerId, messageId, userId, null);
            if (xml != null)
            {
                logger.LogDebug(xml.ToString());
            }
        }
        public static void LogEndSend(this ILogger logger, QueueType queueType, string function, int fromHerId, int toHerId, string messageId, string userId)
        {
            EndSend(logger, queueType, function, fromHerId, toHerId, messageId, userId, null);
        }

        public static void LogResponseTime(this ILogger logger, string messageFunction, int fromHerId, int toHerId, string messageId, string responseTimeMs)
        {
            ResponseTime(logger, messageFunction, fromHerId, toHerId, messageId, responseTimeMs, null);
        }

        public static void LogBeforeNotificationHandler(this ILogger logger, string notificationHandler, string messageFunction, int fromHerId, int toHerId, string messageId)
        {
            BeforeNotificationHandler(logger, notificationHandler, messageFunction, fromHerId, toHerId, messageId, null);
        }
        public static void LogAfterNotificationHandler(this ILogger logger, string notificationHandler, string messageFunction, int fromHerId, int toHerId, string messageId)
        {
            AfterNotificationHandler(logger, notificationHandler, messageFunction, fromHerId, toHerId, messageId, null);
        }

        public static void LogBeforeValidatingCertificate(this ILogger logger, string messageFunction, string thumbprint, string subject, string keyUsage, int ownerHerId, string messageId)
        {
            BeforeValidatingCertificate(logger, messageFunction, thumbprint, subject, keyUsage, ownerHerId, messageId, null);
        }
        public static void LogAfterValidatingCertificate(this ILogger logger, string messageFunction, string thumbprint, string subject, string keyUsage, int ownerHerId, string messageId)
        {
            AfterValidatingCertificate(logger, messageFunction, thumbprint, subject, keyUsage, ownerHerId, messageId, null);
        }

        public static void LogBeforeEncryptingPayload(this ILogger logger, string messageFunction, string thumbprint, string subject, int fromHerId, int toHerId, string messageId)
        {
            BeforeEncryptingPayload(logger, messageFunction, thumbprint, subject, fromHerId, toHerId, messageId, null);
        }
        public static void LogAfterEncryptingPayload(this ILogger logger, string messageFunction, string thumbprint, string subject, int fromHerId, int toHerId, string messageId)
        {
            AfterEncryptingPayload(logger, messageFunction, thumbprint, subject, fromHerId, toHerId, messageId, null);
        }

        public static void LogBeforeFactoryPoolCreateMessage(this ILogger logger, string messageFunction, int fromHerId, int toHerId, string messageId)
        {
            BeforeFactoryPoolCreateMessage(logger, messageFunction, fromHerId, toHerId, messageId, null);
        }
        public static void LogAfterFactoryPoolCreateMessage(this ILogger logger, string messageFunction, int fromHerId, int toHerId, string messageId)
        {
            AfterFactoryPoolCreateMessage(logger, messageFunction, fromHerId, toHerId, messageId, null);
        }

        public static void LogExternalReportedError(this ILogger logger, string message)
        {
            ExternalReportedError(logger, message, null);
        }

        public static void LogRemoveMessageFromQueueNormal(this ILogger logger, string id)
        {
            RemoveMessageFromQueueNormal(logger, id, null);
        }
        public static void LogRemoveMessageFromQueueError(this ILogger logger, string id)
        {
            RemoveMessageFromQueueError(logger, id, null);
        }

        public static void LogTimeoutError(this ILogger logger, QueueType queueType, string messageId, int toHerId)
        {
            LogTimeout(logger, queueType, messageId, toHerId, null);
        }

        static ServiceBusLoggingExtensions()
        {
            StartReceive = LoggerMessage.Define<QueueType, string, int, int, string>(
                LogLevel.Information,
                EventIds.ServiceBusReceive,
                "Start-ServiceBusReceive{QueueType}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");

            EndReceive = LoggerMessage.Define<QueueType, string, int, int, string>(
                LogLevel.Information,
                EventIds.ServiceBusReceive,
                "End-ServiceBusReceive{QueueType}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");

            StartSend = LoggerMessage.Define<QueueType, string, int, int, string, string>(
                LogLevel.Information,
                EventIds.ServiceBusSend,
                "Start-ServiceBusSend{QueueType}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId} PersonalId: {UserId}");

            EndSend = LoggerMessage.Define<QueueType, string, int, int, string, string>(
                LogLevel.Information,
                EventIds.ServiceBusSend,
                "End-ServiceBusSend{QueueType}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId} PersonalId: {UserId}");

            ResponseTime = LoggerMessage.Define<string, int, int, string, string>(
               LogLevel.Information,
               EventIds.NotificationHandler,
               "ResponseTime {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId} ResponseTime: {ResponseTimeMs} ms");

            LogTimeout = LoggerMessage.Define<QueueType, string, int>(
                LogLevel.Error,
                EventIds.SynchronousCallTimeout,
                "MUG-000030 Error Synchronous call {queueType} {messageId} timed out against HerId: {toHerId}.");

            ExternalReportedError = LoggerMessage.Define<string>(
                LogLevel.Error,
                EventIds.ExternalReportedError,
                "{Message}");

            RemoveMessageFromQueueNormal = LoggerMessage.Define<string>(
                LogLevel.Information,
                EventIds.RemoveMessageFromQueue,
                "Removing processed message {MessageId} from queue");

            RemoveMessageFromQueueError = LoggerMessage.Define<string>(
                LogLevel.Information,
                EventIds.RemoveMessageFromQueue,
                "Removing message {MessageId} from queue after reporting error");

            BeforeNotificationHandler = LoggerMessage.Define<string, string, int, int, string>(
                LogLevel.Information,
                EventIds.NotificationHandler,
                "Begin-{NotificationHandler}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");

            AfterNotificationHandler = LoggerMessage.Define<string, string, int, int, string>(
                LogLevel.Information,
                EventIds.NotificationHandler,
                "After-{NotificationHandler}: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");

            BeforeValidatingCertificate = LoggerMessage.Define<string, string, string, string, int, string>(
                LogLevel.Information,
                EventIds.CertificateValidation,
                "Before-CertificateValidation: {MessageFunction} Thumbprint: {Thumbprint} Subject: {Subject} Key Usage: {KeyUsage} Owner HerId: {OwnerHerId} MessageId: {MessageId}");
            AfterValidatingCertificate = LoggerMessage.Define<string, string, string, string, int, string>(
                LogLevel.Information,
                EventIds.CertificateValidation,
                "After-CertificateValidation: {MessageFunction} Thumbprint: {Thumbprint} Subject: {Subject} Key Usage: {KeyUsage} Owner HerId: {OwnerHerId} MessageId: {MessageId}");

            BeforeEncryptingPayload = LoggerMessage.Define<string, string, string, int, int, string>(
                LogLevel.Information,
                EventIds.EncryptPayload,
                "Before-EncryptingPayload: {MessageFunction} Thumbprint: {Thumbprint} Subject: {Subject} FromHerId: {FromHerId} ToHerId: {ToHerId}  MessageId: {MessageId}");
            AfterEncryptingPayload = LoggerMessage.Define<string, string, string, int, int, string>(
                LogLevel.Information,
                EventIds.EncryptPayload,
                "After-EncryptingPayload: {MessageFunction} Thumbprint: {Thumbprint} Subject: {Subject} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");

            BeforeFactoryPoolCreateMessage = LoggerMessage.Define<string, int, int, string>(
                LogLevel.Information,
                EventIds.FactoryPoolCreateEmptyMessage,
                "Before-FactoryPoolCreateMessage: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId}  MessageId: {MessageId}");
            AfterFactoryPoolCreateMessage = LoggerMessage.Define<string, int, int, string>(
                LogLevel.Information,
                EventIds.FactoryPoolCreateEmptyMessage,
                "After-FactoryPoolCreateMessage: {MessageFunction} FromHerId: {FromHerId} ToHerId: {ToHerId} MessageId: {MessageId}");
        }

        public static void LogException(this ILogger logger, string message, Exception ex)
        {
            var sbe = ex as MessagingException;

            logger.LogCritical(sbe?.EventId ?? 0, ex, message);
        }
    }
}