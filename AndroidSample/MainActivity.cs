using Android.App;
using Android.Widget;
using Android.OS;

namespace XamarinScanditSDKSampleAndroid
{
    [Activity (Label = "XamarinScanditSDKSampleAndroid", MainLauncher = true)]
    public class MainActivity : Activity
    {

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Main);

            // obter botão de acordo o layout
            Button button = FindViewById<Button> (Resource.Id.myButton);
            
            button.Click += delegate {
                // iniciar para escanear
                StartActivity(typeof(ScanActivity));
            };
        }
    }
}


