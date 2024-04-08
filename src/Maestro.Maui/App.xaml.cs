using Android.Content;
using Android.Gms.Auth.Api.SignIn;

namespace Maestro.Maui;

public partial class App : Application
{
    private string[] _scopes = { "email", "profile", "openid" };

    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }

    protected override async void OnStart()
    {
        await LoginAsync();
    }

    public async Task<string?> LoginAsync()
    {

#if __ANDROID__
        GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
            .RequestIdToken("667018776899-1qmvf04gemv85q95uve01l87jer9cs5m.apps.googleusercontent.com")
            .RequestEmail()
            .Build();

        GoogleSignInClient signInClient = GoogleSignIn.GetClient(Android.App.Application.Context, gso);

        // In your sign-in button click handler
        Intent signInIntent = signInClient.SignInIntent;
        ((MainActivity)Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!).StartActivityForResult(signInIntent, 1);
#endif

        return null;
    }
}
