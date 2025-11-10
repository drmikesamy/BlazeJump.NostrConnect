using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using NostrConnect.Shared.Services;

#if ANDROID
using Android.Util;
#endif

namespace NostrConnect.Maui.Pages;

public class QRScannerPage : ContentPage
{
    private readonly INostrConnectService _nostrConnectService;
    private readonly IKeyStorageService _keyStorageService;
    private bool _isProcessing = false;
    private const string TAG = "NostrConnect";
    
    private readonly CameraBarcodeReaderView _cameraView;
    private readonly Label _statusLabel;
    private readonly ActivityIndicator _processingIndicator;
    private List<string> _statusMessages = new();

    private void LogInfo(string message)
    {
        Console.WriteLine(message);
#if ANDROID
        Log.Info(TAG, message);
#endif
    }

    public QRScannerPage(INostrConnectService nostrConnectService, IKeyStorageService keyStorageService)
    {
        _nostrConnectService = nostrConnectService;
        _keyStorageService = keyStorageService;
        
        Title = "Scan QR Code";
        BackgroundColor = Color.FromArgb("#1a1a1a");

        _statusLabel = new Label
        {
            Text = "Ready to scan...",
            FontSize = 11,
            TextColor = Colors.LightGray,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Start,
            LineBreakMode = LineBreakMode.WordWrap,
            FontFamily = "monospace"
        };

        _processingIndicator = new ActivityIndicator
        {
            IsRunning = false,
            IsVisible = false,
            Color = Colors.LimeGreen
        };

        _cameraView = new CameraBarcodeReaderView
        {
            IsDetecting = true,
            CameraLocation = CameraLocation.Rear,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional | BarcodeFormats.TwoDimensional,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true,
                TryInverted = true
            }
        };
        _cameraView.BarcodesDetected += OnBarcodesDetected;

        var closeButton = new Button
        {
            Text = "✕",
            FontSize = 24,
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            WidthRequest = 40,
            HeightRequest = 40
        };
        closeButton.Clicked += async (s, e) => await Navigation.PopModalAsync();

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = 60 },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = 120 }
            },
            Children =
            {
                new Grid
                {
                    BackgroundColor = Color.FromArgb("#2a2a2a"),
                    Padding = new Thickness(15, 10),
                    Children =
                    {
                        new Label
                        {
                            Text = "Scan Nostr Connect QR Code",
                            FontSize = 20,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Start
                        },
                        closeButton.Apply(b => 
                        {
                            b.VerticalOptions = LayoutOptions.Center;
                            b.HorizontalOptions = LayoutOptions.End;
                        })
                    }
                }.Apply(g => Grid.SetRow(g, 0)),
                
                _cameraView.Apply(c => Grid.SetRow(c, 1)),
                
                new StackLayout
                {
                    BackgroundColor = Color.FromArgb("#2a2a2a"),
                    Padding = new Thickness(15, 10),
                    Spacing = 5,
                    Children =
                    {
                        new Label
                        {
                            Text = "Position the QR code within the frame",
                            FontSize = 14,
                            TextColor = Colors.White,
                            HorizontalTextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 5)
                        },
                        new ScrollView
                        {
                            Content = _statusLabel,
                            HeightRequest = 100,
                            BackgroundColor = Color.FromArgb("#1a1a1a"),
                            Padding = new Thickness(10, 5)
                        },
                        _processingIndicator
                    }
                }.Apply(s => Grid.SetRow(s, 2))
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission Required", 
                    "Camera permission is required to scan QR codes.", 
                    "OK");
                await Navigation.PopModalAsync();
                return;
            }
        }

        _cameraView.IsDetecting = true;
        _statusLabel.Text = "Ready to scan...";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cameraView.IsDetecting = false;
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing || e.Results.Length == 0)
            return;

        _isProcessing = true;
        _cameraView.IsDetecting = false;

        var barcode = e.Results[0];
        var scannedData = barcode.Value;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _processingIndicator.IsVisible = true;
            _processingIndicator.IsRunning = true;

            try
            {
                if (!scannedData.StartsWith("nostrconnect://", StringComparison.OrdinalIgnoreCase))
                {
                    await DisplayAlert("Invalid QR Code", 
                        "This QR code does not contain a Nostr Connect connection string.", 
                        "OK");
                    await Navigation.PopModalAsync();
                    return;
                }

                var uri = new Uri(scannedData);
                var webPubKey = uri.Host; // The web app's pubkey is the host part

                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var relay = query["relay"];
                var secret = query["secret"];
                var metadata = query["metadata"];

                if (string.IsNullOrEmpty(webPubKey))
                {
                    await DisplayAlert("Invalid Connection",
                        "Missing public key in connection string.",
                        "OK");
                    await Navigation.PopModalAsync();
                    return;
                }
                
                var keyPair = await _keyStorageService.GetStoredKeyPairAsync();
                if (keyPair == null)
                {
                    await DisplayAlert("No Keys Found", 
                        "Please generate your Nostr identity keys first.", 
                        "OK");
                    await Navigation.PopModalAsync();
                    return;
                }

                var sessionId = $"{webPubKey}_{keyPair.PublicKey}";
                
                var success = await _nostrConnectService.ConnectSessionAsync(
                    webPubKey, 
                    keyPair.PublicKey, 
                    relay, 
                    secret);
                
                if (success)
                {
                    await DisplayAlert("Event Published", 
                        $"Connection event published to relay.\n\nWaiting for web app to confirm receipt...\n\nCheck the status log for details.\n\nWeb App: {webPubKey.Substring(0, 16)}...\nYour Key: {keyPair.PublicKey.Substring(0, 16)}...", 
                        "OK");
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Connection Failed", 
                        "Failed to publish connection event to relay.\n\nCheck the status log for details.", 
                        "OK");
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to process connection: {ex.Message}\n\nCheck the status log for details.", 
                    "OK");
                await Navigation.PopModalAsync();
            }
            finally
            {
                _processingIndicator.IsRunning = false;
                _processingIndicator.IsVisible = false;
            }
        });
    }
}

public static class ViewExtensions
{
    public static T Apply<T>(this T view, Action<T> action) where T : VisualElement
    {
        action(view);
        return view;
    }
}
