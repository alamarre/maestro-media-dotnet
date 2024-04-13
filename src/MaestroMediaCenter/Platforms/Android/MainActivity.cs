using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.OS;

namespace Maestro.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        // Check that the result was from the Google Sign-In intent
        if (requestCode == 1)
        {
            // The Task returned from this call is always completed, no need to await it
            GoogleSignInResult result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
            HandleSignInResult(result);
        }
    }

    private void HandleSignInResult(GoogleSignInResult result)
    {
        if (result.IsSuccess)
        {
            // Successfully signed in
            GoogleSignInAccount account = result.SignInAccount;
            // Use the account information to sign in to your backend or proceed in the app
            Console.WriteLine($"Email: {account.Email}, Token: {account.IdToken}");
        }
        else
        {
            var sig = AppSignatureHelper.GetAppSignature();
            Console.WriteLine(sig);
            // Sign in failed, handle error
        }
    }
}

/*[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Android.Content.Intent.ActionView },
              Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
              DataScheme = "ca.omny.videos", DataPath = "/auth/")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "ca.omny.videos";

}
*/
