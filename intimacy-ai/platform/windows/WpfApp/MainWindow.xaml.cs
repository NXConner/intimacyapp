using System; using System.Net.Http; using System.Net.Http.Json; using System.Threading.Tasks; using System.Windows;
namespace WpfApp { public partial class MainWindow : Window { private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("http://localhost:5087") }; private const string ApiKey = "dev-key"; public MainWindow(){ InitializeComponent(); }
  async void OnHello(object s, RoutedEventArgs e){ MessageBox.Show("Windows Hello stub"); await PostAnalytics("windows-hello"); }
  async void OnTray(object s, RoutedEventArgs e){ MessageBox.Show("Tray icon stub"); await PostAnalytics("tray"); }
  async void OnNotify(object s, RoutedEventArgs e){ MessageBox.Show("Notification stub"); await PostAnalytics("notification"); }
  private async Task PostAnalytics(string feature){ try { var req = new HttpRequestMessage(HttpMethod.Post, "/api/analytics"); req.Headers.Add("X-API-Key", ApiKey); req.Content = JsonContent.Create(new { anonymousUserId = "wpf", featureUsed = feature, usageDurationSeconds = 1, platform = "windows", appVersion = "0.0.1" }); var res = await _http.SendAsync(req); } catch {} }
} }
