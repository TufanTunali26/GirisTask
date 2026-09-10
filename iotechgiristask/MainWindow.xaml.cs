using System.Text;
using System.Windows;
using System.Windows.Media;
using MQTTnet;
using MQTTnet.Server;

namespace iotechgiristask
{
    public partial class MainWindow : Window
    {
        private MqttServer? _mqttServer;
        private int _lampState = 0; // 0 kapalı 1 açık 
        private bool _isUpdatingText = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var mqttFactory = new MqttServerFactory();
                var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .WithDefaultEndpointPort(1883)
                    .Build();

                _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

                // Clientlerden gelen mesajları yakalama       
                _mqttServer.InterceptingPublishAsync += async args =>
                {
                    var topic = args.ApplicationMessage.Topic;
                    var payload = args.ApplicationMessage.ConvertPayloadToString();

                    if (topic == "iotech/lamp/set")
                    {
                        if (int.TryParse(payload, out int newState))
                        {
                            await Dispatcher.InvokeAsync(async () =>
                            {
                                await SetLampStateAsync(newState);
                            });
                        }
                    }
                    else if (topic == "iotech/text/random")
                    {
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            GenerateRandomNumber();
                        });
                    }
                    else if (topic == "iotech/get_state")
                    {
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            await BroadcastLampStateAsync();
                            await BroadcastTextStateAsync();
                        });
                    }
                };

                await _mqttServer.StartAsync();
                TxtStatus.Text = "MQTT Server 1883 portunda çalışıyor.";

                // başlangıç durumuna göre grafik oluştur
                if (string.IsNullOrWhiteSpace(TxtInput.Text) || TxtInput.Text == "0")
                {
                    GenerateRandomNumber();
                }

                UpdateLampUi();
                await BroadcastLampStateAsync();
                await BroadcastTextStateAsync();
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Hata: {ex.Message}";
            }
        }

        // lamba state değiştir  ve clientlere bildir
        private async Task SetLampStateAsync(int newState)
        {
            _lampState = newState;
            UpdateLampUi();
            await BroadcastLampStateAsync();
        }

        private void UpdateLampUi()
        {
            if (_lampState == 1)
            {
                LampGraphic.Fill = new SolidColorBrush(Color.FromRgb(255, 235, 59)); // Açık Sarı
                TxtLampStatus.Text = "Lamba: AÇIK";
            }
            else
            {
                LampGraphic.Fill = new SolidColorBrush(Color.FromRgb(66, 66, 66)); // Koyu Gri
                TxtLampStatus.Text = "Lamba: KAPALI";
            }
        }

        // lamba durumunu yayına çıkar 
        private async Task BroadcastLampStateAsync()
        {
            if (_mqttServer == null) return;

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic("iotech/lamp/state")
                .WithPayload(_lampState.ToString())
                .WithRetainFlag(true)
                .Build();

            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(msg));
        }

        // text durumunu yayına çıkar
        private async Task BroadcastTextStateAsync()
        {
            if (_mqttServer == null) return;

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic("iotech/text/state")
                .WithPayload(TxtInput.Text)
                .WithRetainFlag(true)
                .Build();

            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(msg));
        }

        // server  üzerindeki lamba butonu
        private async void BtnToggleLamp_Click(object sender, RoutedEventArgs e)
        {
            int next = (_lampState == 1) ? 0 : 1;
            await SetLampStateAsync(next);
        }

        // server random text oluşturucu buton
        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomNumber();
        }

        private void GenerateRandomNumber()
        {
            _isUpdatingText = true;
            TxtInput.Text = Random.Shared.Next(100, 1000).ToString();
            _isUpdatingText = false;
            _ = BroadcastTextStateAsync();
        }

        // server  klavyeden text giriş butonu
        private async void TxtInput_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdatingText) return;
            await BroadcastTextStateAsync();
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mqttServer != null)
            {
                await _mqttServer.StopAsync();
                _mqttServer.Dispose();
            }
        }
    }
}