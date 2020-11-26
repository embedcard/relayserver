using System;

namespace Thinktecture.Relay.Docker.Configuration
{
	public class RabbitMqConfig
	{
		public const string SECTION_NAME = "RabbitMq";
		public String Host { get; set; }
		public String Username { get; set; }
		public String VirtualHost { get; set; }
		public int Port { get; set; }
		public String Password { get; set; }

		public String ClusterHosts { get; set; }
		public String ConnectionString => $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
	}
}
