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
            var succeeded = await Connector.Connect();
            if (succeeded)
            {
                await UpdateForDrive2();
            }
        }

        private async Task UpdateForDrive2()
        {
            CurrentItems.Clear();
            var root = await Connector.Client.Drive.Root.Request().GetAsync();
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.LocalRoot, root));
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.Shared));
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.Special));
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.Shares));
        }

        private async Task UpdateForSpecial()
        {
            var specials = await Connector.Client.Drive.Special.Request().GetAsync();
            CurrentItems.Clear();
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.OneLevelUp));
            foreach (var special in specials)
            {
                CurrentItems.Add(new FileItemViewModel(special)
                {
                    Name = special.Name,
                });
            }
        }

        private async Task UpdateForShares()
        {
            var sharesCollection = await Connector.Client.Shares.Request().GetAsync();
            CurrentItems.Clear();
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.OneLevelUp));
            foreach (var share in sharesCollection)
            {
                CurrentItems.Add(new FileItemViewModel(share)
                {
                    Name = share.Name,
                    Author = share.Owner.User.DisplayName
                });
            }
        }

        private async Task UpdateForShared()
        {
            var drive = Connector.Client.Drive;
            var shared = drive.Shared;
            var sharedCollection = await shared.Request().GetAsync();
            CurrentItems.Clear();
            CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.OneLevelUp));
            foreach (var sharedItem in sharedCollection)
            {
                CurrentItems.Add(new FileItemViewModel(sharedItem)
                {
                    Name = sharedItem.Name,
                    Author = sharedItem.CreatedBy.User.DisplayName,
                });
            }
        }

        private async Task UpdateForDrive()
        {
            var drive = Connector.Client.Drive;
            await UpdateList(drive);
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

        private async Task UpdateList(FileItemViewModel currDir)
        {
            var currDirItem = currDir.Item;
            var parentDirItem = currDir.ParentItem;
            CurrentItems.Clear();
            var items = await Connector.Client.Drive.Items[currDirItem.Id].Children.Request().GetAsync();
            if (items != null)
            {
                CurrentItems.Add(new FileItemViewModel(FileItemViewModel.SpecialTypes.OneLevelUp, parentDirItem));
                foreach (var item in items)
                {
                    CurrentItems.Add(new FileItemViewModel(item) { Name = item.Name, ParentItem = currDirItem });
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
                    case FileItemViewModel.SpecialTypes.LocalRoot:
                        await UpdateList(newSelection);
                        break;
                    case FileItemViewModel.SpecialTypes.OneLevelUp:
                        if (newSelection.Item != null)
                        {
                            await UpdateList(newSelection);
                        }
                        else
                        {
                            await UpdateForDrive2();
                        }
                        break;
                    case FileItemViewModel.SpecialTypes.Shared:
                        await UpdateForShared();
                        break;
                    case FileItemViewModel.SpecialTypes.Shares:
                        await UpdateForShares();
                        break;
                    case FileItemViewModel.SpecialTypes.Special:
                        await UpdateForSpecial();
                        break;
                }
            }
            _suppressChangedEvent = false;
        }

        private async void SignOutOnClicked(object sender, RoutedEventArgs args)
        {
            await Connector.SignOut();
        }
    }
}
