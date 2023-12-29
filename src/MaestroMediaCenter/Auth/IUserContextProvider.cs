using Maestro.Auth;

namespace Maestro.Auth;
public interface IUserContextProvider {
    UserContext? GetUserContext();
}