using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of an interceptor dealing with the request of the client.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IClientRequestInterceptor<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Called when a request was received.
		/// </summary>
		/// <param name="context">The context of the relay task.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous interception.</returns>
		Task OnRequestReceivedAsync(IRelayContext<TRequest, TResponse> context);
	}
}