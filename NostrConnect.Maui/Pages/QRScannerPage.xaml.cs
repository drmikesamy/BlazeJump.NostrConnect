using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using BlazeJump.Tools.Services.Connections;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Identity;
using NostrConnect.Maui.Services;
using BlazeJump.Tools.Services.Persistence;
using BlazeJump.Tools.Models.NostrConnect;
using BlazeJump.Tools.Models;
using BlazeJump.Tools.Builders;
using NostrConnect.Maui.Services.Identity;

namespace NostrConnect.Maui.Pages;

public partial class QRScannerPage : ContentPage
{
    private readonly INativeIdentityService _identityService;
    private readonly INostrDataService _dataService;
    private bool _isProcessing = false;

    private readonly CameraBarcodeReaderView _cameraView;
    private readonly Label _statusLabel;
    private readonly ActivityIndicator _processingIndicator;

    public QRScannerPage(INativeIdentityService identityService, INostrDataService dataService)
    {
        _identityService = identityService;
        _dataService = dataService;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
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
                _statusLabel.Text = $"Scanned: {scannedData.Substring(0, Math.Min(100, scannedData.Length))}...";

                if (!scannedData.StartsWith("nostrconnect://", StringComparison.OrdinalIgnoreCase))
                {
                    await DisplayAlert("Invalid QR Code",
                        "Not a Nostr Connect QR code.",
                        "OK");
                    await Navigation.PopModalAsync();
                    return;
                }

                var uriBuilder = NostrConnectUriBuilder.Parse(scannedData);

                if (string.IsNullOrEmpty(uriBuilder.GetClientPubKey()) || string.IsNullOrEmpty(uriBuilder.GetRelays().FirstOrDefault()))
                {
                    await DisplayAlert("Invalid Connection",
                        $"Missing required data.",
                        "OK");
                    await Navigation.PopModalAsync();
                    return;
                }

                if (uriBuilder.GetRelays() != null)
                {
                    foreach (var relayUrl in uriBuilder.GetRelays())
                    {
                        try
                        {
                            var relayInfo = new RelayInfo
                            {
                                Url = relayUrl,
                                IsReadEnabled = true,
                                IsWriteEnabled = true
                            };
                            await _dataService.AddRelayAsync(relayInfo);
                            Console.WriteLine($"Added relay from QR code: {relayUrl}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to add relay {relayUrl}: {ex.Message}");
                        }
                    }
                }

                await Navigation.PopModalAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _identityService.OnQrConnectReceived(uriBuilder.GetClientPubKey(), uriBuilder.GetRelays(), uriBuilder.GetSecret(), uriBuilder.GetPermissions());
                    }
                    catch (Exception ex)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await DisplayAlert("Connection Error", $"Failed to establish connection: {ex.Message}", "OK");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed: {ex.Message}", "OK");
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
