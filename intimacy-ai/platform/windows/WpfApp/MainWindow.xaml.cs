using System; using System.Net.Http; using System.Net.Http.Json; using System.Threading.Tasks; using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
namespace WpfApp { public partial class MainWindow : Window { private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("http://localhost:5087") }; private const string ApiKey = "dev-key"; private HubConnection? _hub; public MainWindow(){ InitializeComponent(); }
  async void OnConnect(object s, RoutedEventArgs e){ try { _hub = new HubConnectionBuilder().WithUrl("http://localhost:5087/hubs/analysis").WithAutomaticReconnect().Build(); _hub.On<string>("analysisStarted", id => Dispatcher.Invoke(()=> StatusText.Text = $"Status: started {id}")); _hub.On<string>("analysisCompleted", id => Dispatcher.Invoke(()=> StatusText.Text = $"Status: completed {id}")); await _hub.StartAsync(); StatusText.Text = "Status: connected"; } catch (Exception ex){ StatusText.Text = $"Status: error {ex.Message}"; } }
  async void OnHello(object s, RoutedEventArgs e){ MessageBox.Show("Windows Hello demo"); await PostAnalytics("windows-hello"); }
  async void OnTray(object s, RoutedEventArgs e){ MessageBox.Show("Tray icon demo"); await PostAnalytics("tray"); }
  async void OnNotify(object s, RoutedEventArgs e){ MessageBox.Show("Notification demo"); await PostAnalytics("notification"); }
  private async Task PostAnalytics(string feature){ try { var req = new HttpRequestMessage(HttpMethod.Post, "/api/analytics"); req.Headers.Add("X-API-Key", ApiKey); req.Content = JsonContent.Create(new { anonymousUserId = "wpf", featureUsed = feature, usageDurationSeconds = 1, platform = "windows", appVersion = "0.0.1" }); var res = await _http.SendAsync(req); } catch {} }
} }
