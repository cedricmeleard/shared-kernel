using MediatR;
using SharedKernel.Results;

namespace SharedKernel.Messaging.Abstractions.Queries;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}