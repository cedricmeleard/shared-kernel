using MediatR;
using Results;

namespace Messaging.Abstractions.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}