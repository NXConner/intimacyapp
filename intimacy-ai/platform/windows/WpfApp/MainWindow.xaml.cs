using System; using System.Windows;
namespace WpfApp { public partial class MainWindow : Window { public MainWindow(){ InitializeComponent(); } void OnHello(object s, RoutedEventArgs e){ MessageBox.Show("Windows Hello stub"); } void OnTray(object s, RoutedEventArgs e){ MessageBox.Show("Tray icon stub"); } void OnNotify(object s, RoutedEventArgs e){ MessageBox.Show("Notification stub"); } } }
