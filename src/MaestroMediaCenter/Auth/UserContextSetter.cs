namespace Maestro.Auth;

public class UserContextSetter : IUserContextSetter, IUserContextProvider
{
    private static readonly AsyncLocal<UserContext?> _userContext = new();

    UserContext? IUserContextProvider.GetUserContext()
    {
        return _userContext.Value;
    }

    void IUserContextSetter.SetUserContext(UserContext userContext)
    {
        _userContext.Value = userContext;
    }
}
