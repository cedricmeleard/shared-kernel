using SharedKernel.Messaging.Abstractions.Commands;
using SharedKernel.Messaging.Abstractions.Queries;
using SharedKernel.Results;

namespace SharedKernel.Messaging.Abstractions;

public interface ISender
{
    Task<Result> Send(ICommand command,
        CancellationToken cancellationToken = default);

    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command,
        CancellationToken cancellationToken = default);

    Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query,
        CancellationToken cancellationToken = default);
}