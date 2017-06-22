using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ap.Event.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ap.Event.Rabbitmq
{
    class RabbitMqbasedEventSubcriber : IEventSubscriber
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private readonly ManualResetEventSlim _pasueTicket = new ManualResetEventSlim(true);

        public RabbitMqbasedEventSubcriber(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _connectionString = config.GetConnectionString(Consts.RabbitmqEventConnectionString);
            _connectionFactory = new ConnectionFactory()
            {
                Uri = _connectionString
            };
            _logger = loggerFactory.CreateLogger("Ap.Events.RabbitMqbasedEventSubcriber");

            Task.Factory.StartNew(DoSubscribe, TaskCreationOptions.LongRunning);
        }

        private readonly object _initializingLock = new object();

        private void InitializeConnection()
        {
            if (null != _connection)
            {
                return;
            }

            lock (_initializingLock)
            {
                if (null != _connection)
                {
                    return;
                }

                var tmpConn = _connectionFactory.CreateConnection();
                tmpConn.ConnectionShutdown += HandleConnectionShutdown;
                /**
                * Volatile.Write(ref _connection,tmpConn)等价于Thread.MemoryBarrier();_connection=tmpConn;
                * Thread.MemoryBarrier()即内存栅栏。作用有两个：
                * 1. 保tmpConn=_connectionFactory.CreateConnection()代码一定在_connection=tmpConn之前执行，防止因CPU和IL指令优化而可能导致顺序和预期的不一致。
                * 2. _connection=tmpConn 保证_connection的更改被刷新的内存中，而不是仅在于CPU缓存中。
                */
                Volatile.Write(ref _connection, tmpConn);
            }
        }

        private void HandleConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            var conn = sender as IConnection;
            conn.ConnectionShutdown -= HandleConnectionShutdown;

            _logger.LogCritical(1, $"Subscriber is shudown. Reply code:{e.ReplyCode}, Reply text:{e.ReplyText}.");

            Thread.Sleep(TimeSpan.FromSeconds(30));

            _logger.LogCritical(1, $"Trying to reconnect.");

            _pasueTicket.Reset();
            _connection = null;
            _pasueTicket.Set();
            Resubscribe();
        }

        private void Resubscribe()
        {
            _logger.LogCritical("Resubscribing.");

            var items = _subscibers.Values;
            foreach (var i in items)
            {
                _subscibingQueue.Add(i);
            }
        }

        readonly BlockingCollection<ISubscriber> _subscibingQueue = new BlockingCollection<ISubscriber>();
        readonly ConcurrentDictionary<Guid, ISubscriber> _subscibers = new ConcurrentDictionary<Guid, ISubscriber>();

        Task<Guid> IEventSubscriber.SubscribeAsync<TEventArgs>(string queue, IEvent<TEventArgs> @event,
            Func<TEventArgs, Task> callback)
        {
            var subscriber = new Subscriber<TEventArgs>(queue, queue + ".tag", @event.Key, callback,
                new Dictionary<string, object>());
            var token = Guid.NewGuid();
            _subscibers.TryAdd(token, subscriber);
            _subscibingQueue.Add(subscriber);

            _logger.LogTrace("Subscribing request enqueued.");

            return Task.FromResult(token);
        }

        readonly IDictionary<RuntimeTypeHandle, Action<ISubscriber>> _subcribeActions =
            new Dictionary<RuntimeTypeHandle, Action<ISubscriber>>();

        Action<ISubscriber> GetOrAddSubscribeAction(RuntimeTypeHandle t,
            Func<RuntimeTypeHandle, Action<ISubscriber>> valueFactory)
        {
            Action<ISubscriber> result;
            if (_subcribeActions.TryGetValue(t, out result))
            {
                return result;
            }
            result = valueFactory(t);
            _subcribeActions.Add(t, result);
            return result;
        }

        void DoSubscribe()
        {
            var tp = typeof(RabbitMqbasedEventSubcriber);
            MethodInfo subscribeFunc = tp.GetMethod("Subscribe");

            foreach (var s in _subscibingQueue.GetConsumingEnumerable())
            {
                _logger.LogTrace("Handling subscription request.");
                _pasueTicket.Wait();
                var action = GetOrAddSubscribeAction(s.EventArgsTypeHandle, t =>
                {
                    var genericedFunc = subscribeFunc.MakeGenericMethod(Type.GetTypeFromHandle(s.EventArgsTypeHandle));
                    var subscriberParamExpr = Expression.Parameter(typeof(ISubscriber), "subscriber");
                    var callExpr = Expression.Call(Expression.Constant(this), genericedFunc,
                        new[] { subscriberParamExpr });
                    var result =
                        Expression.Lambda(callExpr, false, subscriberParamExpr).Compile() as Action<ISubscriber>;
                    return result;
                });
                action(s);
            }
        }

        //TODO：临时将该方法设置为public, 否则通过反射GetMethod时无法获取到。具体原因待查。
        public void Subscribe<TEventArgs>(ISubscriber subscriber) where TEventArgs : IEventArgs
        {
            InitializeConnection();

            _logger.LogTrace($"Subscribing for Queue:{subscriber.QueueName}, RoutingKey:{subscriber.RoutingKey}");

            var channel = _connection.CreateModel();
            var exchangeName = channel.DeclareStsStyleExchange();
            channel.QueueDeclare(subscriber.QueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(subscriber.QueueName, exchange: exchangeName, routingKey: subscriber.RoutingKey);
            channel.BasicConsume(subscriber.QueueName, noAck: false, consumerTag: subscriber.ConsumerTag, noLocal: true,
                exclusive: false, arguments: new Dictionary<string, object>()
                ,
                consumer: new Consumer<TEventArgs>(_logger, channel, subscriber.QueueName,
                    subscriber.Callback as Func<TEventArgs, Task>));

            _logger.LogTrace(
                $"Subscribing success for Queue:{subscriber.QueueName}, RoutingKey:{subscriber.RoutingKey}");
        }

        public void Unsubscribe(Guid token)
        {
            ISubscriber s;
            _subscibers.TryRemove(token, out s);
        }

        private class Subscriber<TEventArgs> : ISubscriber where TEventArgs : IEventArgs
        {
            public Subscriber(string queueName, string consumerTag, string routingKey, Func<TEventArgs, Task> callback,
                IDictionary<string, object> arguments)
            {
                QueueName = queueName;
                RoutingKey = routingKey;
                ConsumerTag = consumerTag;
                Callback = callback;
                Arguments = arguments;
                EventArgsTypeHandle = typeof(TEventArgs).TypeHandle;
            }

            public string QueueName { get; private set; }
            public string RoutingKey { get; private set; }
            public string ConsumerTag { get; private set; }
            public Delegate Callback { get; private set; }
            public IDictionary<string, object> Arguments { get; private set; }
            public RuntimeTypeHandle EventArgsTypeHandle { get; private set; }
        }

        private class Consumer<TEventArgs> : IBasicConsumer where TEventArgs : IEventArgs
        {
            readonly IModel _model;
            readonly ILogger _logger;
            readonly string _queueName;
            Func<TEventArgs, Task> _callback;

            public Consumer(ILogger logger, IModel model, string queueName, Func<TEventArgs, Task> callback)
            {
                _logger = logger;
                _model = model;
                _callback = callback;
                _queueName = queueName;
            }

            IModel IBasicConsumer.Model
            {
                get { return _model; }
            }

            event EventHandler<ConsumerEventArgs> IBasicConsumer.ConsumerCancelled
            {
                add { _logger.LogDebug("Calling IBasicConsumer.ConsumerCancelled add."); }

                remove { _logger.LogDebug("Calling IBasicConsumer.ConsumerCancelled remove."); }
            }

            void IBasicConsumer.HandleBasicCancel(string consumerTag)
            {
                _callback = null;
                _logger.LogTrace(
                    $"Calling IBasicConsumer.HandleBasicCancel with queue:{_queueName} consumerTag:{consumerTag}");
            }

            void IBasicConsumer.HandleBasicCancelOk(string consumerTag)
            {
                _logger.LogTrace(
                    $"Calling IBasicConsumer.HandleBasicCancelOk with queue:{_queueName} consumerTag:{consumerTag}");
            }

            void IBasicConsumer.HandleBasicConsumeOk(string consumerTag)
            {
                _logger.LogTrace(
                    $"Calling IBasicConsumer.HandleBasicConsumeOk with queue:{_queueName} consumerTag:{consumerTag}");
            }

            private static readonly JsonSerializer _jsonSerializer = JsonSerializer.Create();

            private int DeserailizationFilureCount = 0;

            void IBasicConsumer.HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
                string exchange, string routingKey, IBasicProperties properties, byte[] body)
            {
                _logger.LogInformation(
                    $"An new message arrives. exchange:{exchange} queue:{_queueName} routingKey:{routingKey}.");
                _logger.LogTrace(
                    $"Calling IBasicConsumer.HandleBasicDeliver with queue:{_queueName} consumerTag:{consumerTag}");

                var callback = _callback;
                if (null == callback)
                {
                    _logger.LogError("_callback is null.");
                    return;
                }

                var jsonString = Encoding.UTF8.GetString(body).Trim();
                _logger.LogTrace($"Parsed json string is {jsonString} with ASCII.");

                _logger.LogTrace("Deserializing.");
                TEventArgs args = default(TEventArgs);
                try
                {
                    var jsonReader = new JsonTextReader(new StringReader(jsonString));
                    args = _jsonSerializer.Deserialize<TEventArgs>(jsonReader);
                    _logger.LogTrace("Deserializing completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(1, ex, "Has error occurred during the deserializing.");

                    //失败超过3次，则不再重新入队消息
                    if (Interlocked.Increment(ref DeserailizationFilureCount) >= 4)
                    {
                        Interlocked.Exchange(ref DeserailizationFilureCount, 4); //避免一直增涨下去造成溢出
                        _model.BasicAck(deliveryTag, multiple: false);
                    }
                    else
                    {
                        _model.BasicReject(deliveryTag, requeue: true);
                    }
                    return;
                }

                Task.Run(async () =>
                {
                    _logger.LogTrace("Calling callback.");
                    try
                    {
                        await callback(args);
                        _model.BasicAck(deliveryTag, multiple: false);
                        _logger.LogTrace("Called callback.");
                    }
                    catch (Exception ex)
                    {
                        _model.BasicReject(deliveryTag, requeue: true);
                        _logger.LogCritical(0, ex, ex.Message);
                        throw;
                    }
                });
            }

            void IBasicConsumer.HandleModelShutdown(object model, ShutdownEventArgs reason)
            {
                _logger.LogTrace("Calling IBasicConsumer.HandleModelShutdown");
            }
        }
    }
}
