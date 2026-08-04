using MediatR;
using SharedKernel.Results;

namespace SharedKernel.Messaging.Abstractions.Commands;

public interface ICommand : IRequest<Result>, IBaseCommand
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}