namespace Authentication.Abstractions;

public interface IUserContext
{
    Guid UserId
    {
        get;
    }

    string Email
    {
        get;
    }

    IReadOnlyList<string> Roles
    {
        get;
    }

    bool IsAuthenticated
    {
        get;
    }
}