using System.Text;
using System.Windows;
using System.Windows.Media;
using MQTTnet;

namespace iotechgiristaskclient
{
    public partial class MainWindow : Window
    {
        private IMqttClient? _mqttClient;
        private int? _lampState = null;      // başlangıçta değersiz lamba state
        private string? _randomNumber = null; // başlangıçta değersiz rastgele sayı  

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
                var factory = new MqttClientFactory();
                _mqttClient = factory.CreateMqttClient();

                _mqttClient.ApplicationMessageReceivedAsync += args =>
                {
                    var topic = args.ApplicationMessage.Topic;
                    var payload = args.ApplicationMessage.ConvertPayloadToString();

                    Dispatcher.Invoke(() =>
                    {
                        if (topic == "iotech/lamp/state")
                        {
                            if (int.TryParse(payload, out int state))
                            {
                                _lampState = state;
                                UpdateLampButton();
                            }
                        }
                        else if (topic == "iotech/text/state")
                        {
                            // serverden son gelen rastgele sayı değerini eşitle
                            _randomNumber = payload;
                            BtnText.Content = string.IsNullOrEmpty(_randomNumber) 
                                ? "Rastgele Sayı: (Boş)" 
                                : _randomNumber;
                        }
                    });

                    return Task.CompletedTask;
                };

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1883)
                    .Build();

                await _mqttClient.ConnectAsync(options);
                TxtStatus.Text = "server'a bağlandı (localhost:1883)";

                // Server state topic'lerine abone ol
                await _mqttClient.SubscribeAsync("iotech/lamp/state");
                await _mqttClient.SubscribeAsync("iotech/text/state");

                // Açılışta sunucudan mevcut değerleri talep et
                await SendMessageAsync("iotech/get_state", "");
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Bağlantı hatası: {ex.Message}";
            }
        }

        private void UpdateLampButton()
        {
            if (_lampState == 1)
            {
                // Açık renkli (Açık Sarı)
                BtnLamp.Background = new SolidColorBrush(Color.FromRgb(255, 235, 59));
                BtnLamp.Foreground = Brushes.Black;
                BtnLamp.Content = "Lamba: AÇIK";
            }
            else if (_lampState == 0)
            {
                // Koyu renkli (Koyu Gri)
                BtnLamp.Background = new SolidColorBrush(Color.FromRgb(66, 66, 66));
                BtnLamp.Foreground = Brushes.White;
                BtnLamp.Content = "Lamba: KAPALI";
            }
            else
            {
                BtnLamp.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                BtnLamp.Foreground = Brushes.White;
                BtnLamp.Content = "Lamba: Durum Bekleniyor";
            }
        }

        // lamba butonu
        private async void BtnLamp_Click(object sender, RoutedEventArgs e)
        {
            // lamba state 0 ise bir 1 ile 0 olarak değiştirilmesi talebini gönder
            int targetState = (_lampState == 0) ? 1 : 0;
            await SendMessageAsync("iotech/lamp/set", targetState.ToString());
        }

        // text butonu
        private async void BtnText_Click(object sender, RoutedEventArgs e)
        {
            // Server'dan random sayı üretmesini talep et
            await SendMessageAsync("iotech/text/random", "");
        }

        private async Task SendMessageAsync(string topic, string payload)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected) return;

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await _mqttClient.PublishAsync(msg);
        }

        private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mqttClient != null)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
            }
        }
    }
}