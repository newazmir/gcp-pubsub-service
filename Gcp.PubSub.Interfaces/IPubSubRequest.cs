using MediatR;

namespace Gcp.PubSub.Interfaces
{
    public interface IPubSubRequest : IRequest<Unit>, IPubSubMessageBase
    {
    }

    public interface IPubSubRequest<out T> : IRequest<T>, IPubSubMessageBase
    {
    }
}
