using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Ap.Event.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ap.Event.Rabbitmq
{
    class RabbitMqBasedEventPublisher : IEventPublisher
    {
        private readonly object _initializingLock = new object();
        private readonly string _connectionString;
        private IConnection _connection;
        private readonly ILogger _logger;

        public RabbitMqBasedEventPublisher(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _connectionString = config.GetConnectionString(Consts.RabbitmqEventConnectionString);
            _connectionFactory = new ConnectionFactory()
            {
                Uri = _connectionString
            };
            _logger = loggerFactory.CreateLogger("Ap.Events.RabbitMqBasedEventPublisher");
        }
        readonly IConnectionFactory _connectionFactory;
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
                /**
                * Volatile.Write(ref _connection,tmpConn)等价于Thread.MemoryBarrier();_connection=tmpConn;
                * Thread.MemoryBarrier()即内存栅栏。作用有两个：
                * 1. 保tmpConn=_connectionFactory.CreateConnection()代码一定在_connection=tmpConn之前执行，防止因CPU和IL指令优化而可能导致顺序和预期的不一致。
                * 2. _connection=tmpConn 保证_connection的更改被刷新的内存中，而不是仅在于CPU缓存中。
                */
                Volatile.Write(ref _connection, tmpConn);
            }
        }

        Task IEventPublisher.PublishAsync<TEventArgs>(IEvent<TEventArgs> @event)
        {
            //在后台线程中执行发送操作
            Task.Run(() =>
            {
                InitializeConnection();
                var channel = _connection.CreateModel();
                var exchangeName = channel.DeclareStsStyleExchange();
                var properties = channel.CreateBasicProperties();
                properties.ContentType = "Application/Json";
                properties.ContentEncoding = "utf8";
                properties.DeliveryMode = 2;//Peristent
                properties.AppId = "AP-STS";
                channel.BasicPublish(exchangeName, routingKey: @event.Key, mandatory: false, basicProperties: properties
                    , body: ConvertArgsToBytes<TEventArgs>(@event.Args));
                channel.Close();
            }).ContinueWith(t =>
            {
                var ex = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);
            return Task.FromResult(1);
        }

        private JsonSerializer _jsonSerializer = JsonSerializer.Create();
        private byte[] ConvertArgsToBytes<TEventArgs>(TEventArgs args) where TEventArgs : IEventArgs
        {
            var sb = new StringBuilder(256);
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = _jsonSerializer.Formatting;
                _jsonSerializer.Serialize(writer, args);
                var jsonText = sw.ToString();
                var result = Encoding.UTF8.GetBytes(jsonText);
                return result;
            }
        }
    }
}
