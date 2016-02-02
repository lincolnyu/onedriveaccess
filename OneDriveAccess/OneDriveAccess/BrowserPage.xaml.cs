using Microsoft.OneDrive.Sdk;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDriveAccess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        private bool _suppressChangedEvent;

        public BrowserPage()
        {
            InitializeComponent();

            DataContext = this;
        }

        public OneDriveConnector Connector { get; private set; }

        public BrowserPageParameter Parameter { get; private set; }

        public ObservableCollection<FileItemViewModel> CurrentItems { get; } = new ObservableCollection<FileItemViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            Parameter = (BrowserPageParameter)args.Parameter;
        }

        private async void PageOnLoaded(object sender, RoutedEventArgs e)
        {
            Connector = new OneDriveConnector();
            await Connector.Connect(Parameter.ClientId, Parameter.ReturnUrl);
            UpdateForDrive2();
        }

        private void UpdateForDrive2()
        {
            CurrentItems.Clear();
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.LocalRoot));
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.SharedRoot));
        }

        private async Task UpdateForShared()
        {
            var drive = Connector.Client.Drive;
            var shared = drive.Shared;
            var sharedCollection = await shared.Request().GetAsync();
            CurrentItems.Clear();
            foreach (var sharedItem in sharedCollection)
            {
                CurrentItems.Add(new FileItemViewModel(sharedItem)
                {
                    Name = sharedItem.Name,
                    Author = sharedItem.CreatedBy.User.DisplayName,
                    Path = sharedItem.ParentReference.Path
                });
            }
        }

        private async Task UpdateForDrive()
        {
            var drive = Connector.Client.Drive;
            await UpdateList(drive);
        }

        private async Task UpdateForRoot()
        {
            CurrentItems.Clear();
            var root = Connector.Client.Drive.Root;
            var items = await root.Children.Request().GetAsync();
            if (items != null)
            {
                foreach (var item in items)
                {
                    CurrentItems.Add(new FileItemViewModel(item) { Name = item.Name });
                }
            }
        }

        private async Task UpdateForPath(string path)
        {
            var root = Connector.Client.Drive.Root;
            var parent = root.ItemWithPath(path);
            await UpdateList(parent);
        }

        private async Task UpdateList(IDriveRequestBuilder drb)
        {
            CurrentItems.Clear();
            var items = await drb.Items.Request().GetAsync();
            if (items != null)
            {
                foreach (var item in items)
                {
                    CurrentItems.Add(new FileItemViewModel(item) { Name = item.Name });
                }
            }
        }

        private async Task UpdateList(IItemRequestBuilder irb)
        {
            CurrentItems.Clear();
            var items = await irb.Children.Request().GetAsync();
            if (items != null)
            {
                foreach (var item in items)
                {
                    CurrentItems.Add(new FileItemViewModel(item) { Name = item.Name });
                }
            }
        }

        private void UpdateList(Item parent)
        {
            CurrentItems.Clear();
            var items = parent.Children;
            if (items != null)
            {
                foreach (var item in items)
                {
                    CurrentItems.Add(new FileItemViewModel(item) { Name = item.Name });
                }
            }
        }

        private async void SelectedFileItemChanged(object sender, SelectionChangedEventArgs args)
        {
            if (_suppressChangedEvent)
            {
                return;
            }
            _suppressChangedEvent = true;
            var newSelection = args.AddedItems.Cast<FileItemViewModel>().FirstOrDefault();
            if (newSelection != null)
            {
                switch (newSelection.SpecialType)
                {
                    case FileItemViewModel.SpecialTypes.Normal:
                        UpdateList(newSelection.Item);
                        break;
                    case FileItemViewModel.SpecialTypes.LocalRoot:
                        await UpdateForRoot();
                        break;
                    case FileItemViewModel.SpecialTypes.SharedRoot:
                        await UpdateForShared();
                        break;
                }
            }
            _suppressChangedEvent = false;
        }
    }
}
