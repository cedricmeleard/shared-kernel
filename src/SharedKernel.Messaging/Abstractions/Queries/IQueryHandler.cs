using MediatR;
using Results;

namespace Messaging.Abstractions.Queries;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}