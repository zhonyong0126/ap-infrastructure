using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Ap.Event.Rabbitmq
{
    static class Extensions
    {
        public static string DeclareStsStyleExchange(this IModel channel)
        {
            channel.ExchangeDeclare(Consts.StsStyleExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
            return Consts.StsStyleExchangeName;
        }
    }
}
