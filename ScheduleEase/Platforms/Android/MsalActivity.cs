using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace ScheduleEase.Platforms.Android
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = "msal34aea1ef-ac95-4474-b079-205b0da5308c")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}
