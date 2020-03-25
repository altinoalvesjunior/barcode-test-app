using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using ScanditBarcodePicker.Android;
using ScanditBarcodePicker.Android.Recognition;

namespace XamarinScanditSDKSampleAndroid
{
    [Activity (Label = "ScanActivity")]
    public class ScanActivity : Activity, IOnScanListener, IDialogInterfaceOnCancelListener
    {
        public static string appKey = "LICENÇA AQUI";

        private const int CameraPermissionRequest = 0;

        private BarcodePicker barcodePicker;
        private bool deniedCameraAccess = false;
        private bool paused = true;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            ScanditLicense.AppKey = appKey;

            InitializeAndStartBarcodeScanning();
        }

        protected override void OnPause()
        {
            base.OnPause();

            //Chamada do GC.Collect, Garbage Collect não funcionou
            GC.Collect();
            barcodePicker.StopScanning();
            paused = true;
        }

        void GrantCameraPermissionsThenStartScanning()
        {
            if (CheckSelfPermission(Manifest.Permission.Camera) != (int)Permission.Granted)
            {
                if (deniedCameraAccess == false)
                {
                    // It's pretty clear for why the camera is required. We don't need to give a
                    // detailed reason.
                    RequestPermissions(new String[] { Manifest.Permission.Camera }, CameraPermissionRequest);
                }

            }
            else
            {
                Console.WriteLine("starting scanning");
                barcodePicker.StartScanning();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == CameraPermissionRequest)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    deniedCameraAccess = false;
                    if (!paused)
                    {
                        barcodePicker.StartScanning();
                    }
                }
                else
                {
                    deniedCameraAccess = true;
                }
                return;
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();

            paused = false;
            // Manipula as permissões para o Marshmallow e para a frente.
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                GrantCameraPermissionsThenStartScanning();
            }
            else
            {                barcodePicker.StartScanning();
            }
        }

        void InitializeAndStartBarcodeScanning()
        {
            ScanSettings settings = ScanSettings.Create ();
            int[] symbologiesToEnable = new int[] {
                Barcode.SymbologyEan13,
                Barcode.SymbologyEan8,
                Barcode.SymbologyUpca,
                Barcode.SymbologyDataMatrix,
                Barcode.SymbologyQr,
                Barcode.SymbologyCode39,
                Barcode.SymbologyCode128,
                Barcode.SymbologyInterleaved2Of5,
                Barcode.SymbologyUpce
            };

            foreach (int symbology in symbologiesToEnable)
            {
                settings.SetSymbologyEnabled (symbology, true);
            }

            SymbologySettings symSettings = settings.GetSymbologySettings(Barcode.SymbologyCode128);
            short[] activeSymbolCounts = new short[] {
                7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
            };
            symSettings.SetActiveSymbolCounts(activeSymbolCounts);
            barcodePicker = new BarcodePicker (this, settings);
            barcodePicker.SetOnScanListener (this);
            SetContentView (barcodePicker);
        }

        public void DidScan(IScanSession session)
        {
            if (session.NewlyRecognizedCodes.Count > 0) {
                Barcode code = session.NewlyRecognizedCodes [0];
                Console.WriteLine ("barcode scanned: {0}, '{1}'", code.SymbologyName, code.Data);

                // Mesmo problema no Garbage Collector lá de cima
                GC.Collect ();

                // Para de escanear na própria sessão
                session.StopScanning ();

                //Edição da UI, NÃO MEXER, DA ÚLTIMA VEZ DEU MERDA
                RunOnUiThread (() => {
                    AlertDialog alert = new AlertDialog.Builder (this)
                        .SetTitle (code.SymbologyName + " Barcode Detected")
                        .SetMessage (code.Data)
                        .SetPositiveButton("OK", delegate {
                            barcodePicker.StartScanning ();
                        })
                        .SetOnCancelListener(this)
                        .Create ();

                    alert.Show ();
                });
            }
        }

        public void OnCancel(IDialogInterface dialog) {
            barcodePicker.StartScanning ();
        }

        public override void OnBackPressed ()
        {
            base.OnBackPressed ();
            Finish ();
        }
    }
}
