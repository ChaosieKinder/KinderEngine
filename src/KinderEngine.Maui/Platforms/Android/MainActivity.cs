using Android.App;
using Android.Content.PM;
using Android.OS;

namespace KinderEngine.Maui.Platforms.Android
{
    public class KinderEngineMainActivity : MauiAppCompatActivity
    {
        public static KinderEngineMainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;

            base.OnCreate(savedInstanceState);
        }
    }
}
