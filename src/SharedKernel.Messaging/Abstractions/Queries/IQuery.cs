using MediatR;
using SharedKernel.Results;

namespace SharedKernel.Messaging.Abstractions.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}