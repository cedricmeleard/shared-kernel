namespace SharedKernel.Results;

public interface IResult<out TSelf>
    where TSelf : IResult<TSelf>
{
    static abstract TSelf Failure(Error error);
}
