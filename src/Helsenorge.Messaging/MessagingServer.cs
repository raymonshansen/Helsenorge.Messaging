﻿/* 
 * Copyright (c) 2020, Norsk Helsenett SF and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the MIT license
 * available at https://raw.githubusercontent.com/helsenorge/Helsenorge.Messaging/master/LICENSE
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Helsenorge.Messaging.Abstractions;
using Helsenorge.Messaging.ServiceBus.Receivers;
using Helsenorge.Registries.Abstractions;
using Microsoft.Extensions.Logging;

namespace Helsenorge.Messaging
{
    /// <summary>
    /// Default implementation for <see cref="IMessagingServer"/>
    /// This must be hosted as a singleton in order to leverage connection pooling against the message bus
    /// </summary>
    public sealed class MessagingServer : MessagingCore, IMessagingServer, IMessagingNotification
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentBag<MessageListener> _listeners = new ConcurrentBag<MessageListener>();
        private readonly ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Action<IncomingMessage> _onAsynchronousMessageReceived;
        private Action<IncomingMessage> _onAsynchronousMessageReceivedStarting;
        private Action<IncomingMessage> _onAsynchronousMessageReceivedCompleted;

        private Func<IncomingMessage, XDocument> _onSynchronousMessageReceived;
        private Action<IncomingMessage> _onSynchronousMessageReceivedCompleted;
        private Action<IncomingMessage> _onSynchronousMessageReceivedStarting;

        private Action<IMessagingMessage> _onErrorMessageReceived;
        private Action<IncomingMessage> _onErrorMessageReceivedStarting;

        private Action<IMessagingMessage, Exception> _onUnhandledException;
        private Action<IMessagingMessage, Exception> _onHandledException;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="logger">Logger used for generic messages</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        [Obsolete("This constructor is replaced by ctor(MessagingSettings, ILoggerFactory, ICollaborationProtocolRegistry, IAddressRegistry) and will be removed in a future version")]
        public MessagingServer(
            MessagingSettings settings,
            ILogger logger,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry) : base(settings, collaborationProtocolRegistry, addressRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="logger">Logger used for generic messages</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        /// <param name="certificateStore">Reference to an implementation of <see cref="ICertificateStore"/>.</param>
        [Obsolete("This constructor is replaced by ctor(MessagingSettings, ILoggerFactory, ICollaborationProtocolRegistry, IAddressRegistry, ICertificateStore) and will be removed in a future version")]
        public MessagingServer(
            MessagingSettings settings,
            ILogger logger,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry,
            ICertificateStore certificateStore) : base(settings, collaborationProtocolRegistry, addressRegistry, certificateStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="logger">Logger used for generic messages</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        /// <param name="certificateStore">Reference to an implementation of <see cref="ICertificateStore"/>.</param>
        /// <param name="certificateValidator">Reference to an implementation of <see cref="ICertificateValidator"/>.</param>
        /// <param name="messageProtection">Reference to an implementation of <see cref="IMessageProtection"/>.</param>
        [Obsolete("This constructor is replaced by ctor(MessagingSettings, ILoggerFactory, ICollaborationProtocolRegistry, IAddressRegistry, ICertificateStore, ICertificateValidator, IMessageProtection) and will be removed in a future version")]
        public MessagingServer(
            MessagingSettings settings,
            ILogger logger,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry,
            ICertificateStore certificateStore,
            ICertificateValidator certificateValidator,
            IMessageProtection messageProtection) : base(settings, collaborationProtocolRegistry, addressRegistry, certificateStore, certificateValidator, messageProtection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        public MessagingServer(
            MessagingSettings settings,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry) : base(settings, collaborationProtocolRegistry, addressRegistry)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger(nameof(MessagingServer));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        /// <param name="certificateStore">Reference to an implementation of <see cref="ICertificateStore"/>.</param>
        public MessagingServer(
            MessagingSettings settings,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry,
            ICertificateStore certificateStore) : base(settings, collaborationProtocolRegistry, addressRegistry, certificateStore)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger(nameof(MessagingServer));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Set of options to use</param>
        /// <param name="loggerFactory">Logger Factory</param>
        /// <param name="collaborationProtocolRegistry">Reference to the collaboration protocol registry</param>
        /// <param name="addressRegistry">Reference to the address registry</param>
        /// <param name="certificateStore">Reference to an implementation of <see cref="ICertificateStore"/>.</param>
        /// <param name="certificateValidator">Reference to an implementation of <see cref="ICertificateValidator"/>.</param>
        /// <param name="messageProtection">Reference to an implementation of <see cref="IMessageProtection"/>.</param>
        public MessagingServer(
            MessagingSettings settings,
            ILoggerFactory loggerFactory,
            ICollaborationProtocolRegistry collaborationProtocolRegistry,
            IAddressRegistry addressRegistry,
            ICertificateStore certificateStore,
            ICertificateValidator certificateValidator,
            IMessageProtection messageProtection) : base(settings, collaborationProtocolRegistry, addressRegistry, certificateStore, certificateValidator, messageProtection)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger(nameof(MessagingServer));
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Messaging Server starting up");

            for (var i = 0; i < Settings.ServiceBus.Asynchronous.ProcessingTasks; i++)
            {
                _listeners.Add(new AsynchronousMessageListener(ServiceBus, _loggerFactory.CreateLogger($"AsyncListener_{i}"), this));
            }
            for (var i = 0; i < Settings.ServiceBus.Synchronous.ProcessingTasks; i++)
            {
                _listeners.Add(new SynchronousMessageListener(ServiceBus, _loggerFactory.CreateLogger($"SyncListener_{i}"), this));
            }
            for (var i = 0; i < Settings.ServiceBus.Error.ProcessingTasks; i++)
            {
                _listeners.Add(new ErrorMessageListener(ServiceBus, _loggerFactory.CreateLogger($"ErrorListener_{i}"), this));
            }
            foreach (var listener in _listeners)
            {
                _tasks.Add(Task.Factory.StartNew(() => listener.Start(_cancellationTokenSource.Token)));
            }
        }
        /// <summary>
        /// Stops the server
        /// </summary>
        /// <param name="timeout">The amount of time we wait for thigns to shut down</param>
        public void Stop(TimeSpan timeout)
        {
            _logger.LogInformation("Messaging Server shutting down");
            _cancellationTokenSource.Cancel();
            
            Task.WaitAll(_tasks.ToArray(), timeout);
            
            // when all the listeners have shut down, close down the messaging infrastructure
            ServiceBus.SenderPool.Shutdown(_logger);
            ServiceBus.ReceiverPool.Shutdown(_logger);
            ServiceBus.FactoryPool.Shutdown(_logger);
        }

        /// <summary>
        /// Registers a delegate that should be called when we have enough information to process the message. This is where the main processing logic hooks in.
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterAsynchronousMessageReceivedCallback(Action<IncomingMessage> action) => _onAsynchronousMessageReceived = action;

        void IMessagingNotification.NotifyAsynchronousMessageReceived(IncomingMessage message)
        {
            _logger.LogDebug("NotifyAsynchronousMessageReceived");
            _onAsynchronousMessageReceived?.Invoke(message);
        } 
        /// <summary>
        /// Registers a delegate that should be called as we start processing a message
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterAsynchronousMessageReceivedStartingCallback(Action<IncomingMessage> action) => _onAsynchronousMessageReceivedStarting = action;

        void IMessagingNotification.NotifyAsynchronousMessageReceivedStarting(IncomingMessage message)
        {
            _logger.LogDebug("NotifyAsynchronousMessageReceivedStarting");
            _onAsynchronousMessageReceivedStarting?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called when we are finished processing the message.
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterAsynchronousMessageReceivedCompletedCallback(Action<IncomingMessage> action) => _onAsynchronousMessageReceivedCompleted = action;

        void IMessagingNotification.NotifyAsynchronousMessageReceivedCompleted(IncomingMessage message)
        {
            _logger.LogDebug("NotifyAsynchronousMessageReceivedCompleted");
            _onAsynchronousMessageReceivedCompleted?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called when we receive an error message
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterErrorMessageReceivedCallback(Action<IMessagingMessage> action) => _onErrorMessageReceived = action;

        /// <summary>
        /// Registers a delegate that should be called as we start processing a message
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterErrorMessageReceivedStartingCallback(Action<IncomingMessage> action) => _onErrorMessageReceivedStarting = action;

        void IMessagingNotification.NotifyErrorMessageReceivedStarting(IncomingMessage message)
        {
            _logger.LogDebug("NotifyErrorMessageReceivedStarting");
            _onErrorMessageReceivedStarting?.Invoke(message);
        }

        void IMessagingNotification.NotifyErrorMessageReceived(IMessagingMessage message)
        {
            _logger.LogDebug("NotifyErrorMessageReceived");
            _onErrorMessageReceived?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called when we have enough information to process the message. This is where the main processing logic hooks in.
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterSynchronousMessageReceivedCallback(Func<IncomingMessage, XDocument> action) => _onSynchronousMessageReceived = action;

        XDocument IMessagingNotification.NotifySynchronousMessageReceived(IncomingMessage message)
        {
            _logger.LogDebug("NotifySynchronousMessageReceived");
            return _onSynchronousMessageReceived?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called when we are finished processing the message.
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterSynchronousMessageReceivedCompletedCallback(Action<IncomingMessage> action) => _onSynchronousMessageReceivedCompleted = action;

        void IMessagingNotification.NotifySynchronousMessageReceivedCompleted(IncomingMessage message)
        {
            _logger.LogDebug("NotifySynchronousMessageReceivedCompleted");
            _onSynchronousMessageReceivedCompleted?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called as we start processing a message
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterSynchronousMessageReceivedStartingCallback(Action<IncomingMessage> action) => _onSynchronousMessageReceivedStarting = action;

        void IMessagingNotification.NotifySynchronousMessageReceivedStarting(IncomingMessage message)
        {
            _logger.LogDebug("NotifySynchronousMessageReceivedStarting");
            _onSynchronousMessageReceivedStarting?.Invoke(message);
        }
        /// <summary>
        /// Registers a delegate that should be called when we have an handled exception
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterHandledExceptionCallback(Action<IMessagingMessage, Exception> action) => _onHandledException = action;

        void IMessagingNotification.NotifyHandledException(IMessagingMessage message, Exception ex)
        {
            _logger.LogDebug("NotifyHandledException");
            _onHandledException?.Invoke(message, ex);
        }
        /// <summary>
        /// Registers a delegate that should be called when we have an unhandled exception
        /// </summary>
        /// <param name="action">The delegate that should be called</param>
        public void RegisterUnhandledExceptionCallback(Action<IMessagingMessage, Exception> action) => _onUnhandledException = action;

        void IMessagingNotification.NotifyUnhandledException(IMessagingMessage message, Exception ex)
        {
            _logger.LogDebug("NotifyUnhandledException");
            _onUnhandledException?.Invoke(message, ex);
        }
    }
}
