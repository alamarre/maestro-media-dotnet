using Android.Content.PM;
using Android.Util;
using Java.Security;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Maestro.Maui; // Replace YourApp with the namespace of your application
using Application = Android.App.Application;
using Android.Content;

[assembly: Dependency(typeof(AppSignatureHelper))]
namespace Maestro.Maui
{
    public class AppSignatureHelper
    {
        public static string GetAppSignature()
        {
            try
            {
                Context context = Application.Context;
                PackageInfo? packageInfo = context?.PackageManager?.GetPackageInfo(context.PackageName!, PackageInfoFlags.Signatures);
                var signature = packageInfo!.Signatures![0];
                MessageDigest sha1 = MessageDigest.GetInstance("SHA1");
                sha1.Update(signature.ToByteArray());
                byte[] digest = sha1.Digest();
                StringBuilder hexString = new StringBuilder();
                foreach (byte b in digest)
                {
                    string hex = $"{b:X2}"; // Converts to hex string
                    hexString.Append(hex);
                }
                return hexString.ToString().ToLowerInvariant();
            }
            catch (Exception ex)
            {
                // Handle the exception accordingly
                return null;
            }
        }
    }
}
