using MediatR;
using Messaging.Abstractions.Commands;
using Messaging.Abstractions.Queries;
using Results;

namespace Messaging.Implementation;

public sealed class Sender(ISender mediatorSender) : Abstractions.ISender
{
    public Task<Result> Send(ICommand command,
        CancellationToken cancellationToken = default) =>
        mediatorSender.Send(command, cancellationToken);

    public Task<Result<TResponse>> Send<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default) =>
        mediatorSender.Send(command, cancellationToken);

    public Task<Result<TResponse>> Send<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default) =>
        mediatorSender.Send(query, cancellationToken);
}