using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Protocols.RabbitMq;
using Thinktecture.Relay.Transport;

// ReSharper disable once CheckNamespace; (extension methods on IServiceCollection namespace)
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for the <see cref="IRelayServerBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayServerBuilderExtensions
	{
		/// <summary>
		/// Adds the routing based on Rabbit MQ.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</param>
		/// <param name="configure">An optional configure callback for setting the <see cref="RabbitMqOptions"/>.</param>
		/// <param name="useServerRouting">Enables Rabbit MQ for server-to-server communication.</param>
		/// <param name="useTenantRouting">Enables Rabbit MQ for server-to-tenant communication.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayServerBuilder{TRequest,TResponse}"/> instance.</returns>
		public static IRelayServerBuilder<TRequest, TResponse> AddRabbitMqRouting<TRequest, TResponse>(
			this IRelayServerBuilder<TRequest, TResponse> builder, Action<RabbitMqOptions> configure = null, bool useServerRouting = true,
			bool useTenantRouting = true)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
		{
			builder.Services.TryAddSingleton<IServerDispatcher<TResponse>, ServerDispatcher<TResponse>>();
			builder.Services.TryAddSingleton<IServerHandler<TResponse>, ServerHandler<TResponse>>();

			if (configure != null)
			{
				builder.Services.Configure(configure);
			}

			if (useServerRouting)
			{
				builder.Services.TryAddSingleton<IServerDispatcher<TResponse>, ServerDispatcher<TResponse>>();
				builder.Services.TryAddSingleton<IServerHandler<TResponse>, ServerHandler<TResponse>>();
			}

			if (useTenantRouting)
			{
				builder.Services.TryAddSingleton<ITenantDispatcher<TRequest>, TenantDispatcher<TRequest>>();
				builder.Services.TryAddSingleton<ITenantHandlerFactory<TRequest, TResponse>, TenantHandlerFactory<TRequest, TResponse>>();
			}

			builder.Services.TryAddSingleton<IConnection>(provider =>
			{
				var relayServerContext = provider.GetRequiredService<RelayServerContext>();
				var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>();

				var factory = new ConnectionFactory
				{
					DispatchConsumersAsync = true,
					EndpointResolverFactory = endpoints => new RoundRobinEndpointResolver(endpoints),
					Uri = new Uri(options.Value.Uri),
				};

				return factory.CreateConnection(
					AmqpTcpEndpoint.ParseMultiple(options.Value.ClusterHosts ?? factory.Uri.Host),
					$"RelayServer {relayServerContext.OriginId}");
			});
			builder.Services.AddSingleton<ModelFactory>();

			return builder;
		}
	}
}