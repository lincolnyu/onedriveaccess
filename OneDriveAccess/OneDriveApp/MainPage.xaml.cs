using OneDriveAccess;
using System.Reflection.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneDriveApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        private void BrowseOnClick(object sender, RoutedEventArgs args)
        {
            var clientId = "000000004C17DEAE";
            var returnUrl = "";
            var parameter = new BrowserPageParameter(clientId, returnUrl);
            Frame.Navigate(typeof(BrowserPage), parameter);
        }
    }
}
