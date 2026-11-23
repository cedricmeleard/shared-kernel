namespace SharedKernel.Results;

public sealed record Success
{
    public static readonly Success Instance = new();
}

public sealed record Success<TValue>(TValue Value);
