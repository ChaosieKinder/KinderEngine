using Android.Content;
using Android.Views;
using Android.Runtime;
using Android.Content.PM;
using KinderEngine.Maui.Platforms.Android;

namespace KinderEngine.Maui.Services
{
    public partial class DeviceOrientationService
    {
        public DeviceOrientation GetOrientation()
        {
            IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            SurfaceOrientation orientation = windowManager.DefaultDisplay.Rotation;
            bool isLandscape = orientation == SurfaceOrientation.Rotation90 || orientation == SurfaceOrientation.Rotation270;
            return isLandscape ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
        }

        public void SetOrientation(DeviceOrientation orientation)
        {
            switch (orientation)
            {
                case DeviceOrientation.Landscape:
                    KinderEngineMainActivity.Instance.RequestedOrientation = ScreenOrientation.Landscape;
                    break;
                case DeviceOrientation.Portrait:
                    KinderEngineMainActivity.Instance.RequestedOrientation = ScreenOrientation.Portrait;
                    break;
            }
        }
    }
}
