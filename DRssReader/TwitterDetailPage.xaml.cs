using DRssReader.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace DRssReader
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class TwitterDetailPage : DRssReader.Common.LayoutAwarePage
    {
        public TwitterDetailPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            try
            {
                // TODO: Create an appropriate data model for your problem domain to replace the sample data
                var item = RSSDataSource.GetItem((String)navigationParameter);
                this.DefaultViewModel["Group"] = item.Group;
                this.DefaultViewModel["Items"] = item.Group.Items;
                this.flipView.SelectedItem = item;
            }
            finally { }
            //catch { this.Frame.Navigate(typeof(GroupedItemsPage)); }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (RSSDataItem)this.flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.UniqueId;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataTransferManager.GetForCurrentView().DataRequested += Share_DataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataTransferManager.GetForCurrentView().DataRequested -= Share_DataRequested;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Launcher.LaunchUriAsync(new Uri("http://www.3dnews.ru/copyright/", UriKind.Absolute));
            var linkFullNews = (RSSDataItem)(flipView.SelectedItem);

            await Launcher.LaunchUriAsync(new Uri(linkFullNews.UniqueId, UriKind.Absolute));
            //Launcher.LaunchUriAsync(new Uri("http://www.google.com/policies/privacy/", UriKind.Absolute));
            //Launcher.LaunchUriAsync(new Uri("http://www.dennis.co.uk/privacy", UriKind.Absolute));
            //throw new NotImplementedException();
        }

        private void Share_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var selectedItem = (RSSDataItem)this.flipView.SelectedItem;

            args.Request.Data.Properties.Title = selectedItem.Title;
            args.Request.Data.Properties.Description = selectedItem.Group.ToString();
            args.Request.Data.SetText(selectedItem.Content);
            //args.Request.Data.SetText(selectedItem.Content);
            args.Request.Data.SetUri(new Uri(selectedItem.UniqueId));


            //// Because we are making async calls in the DataRequested event handler,
            ////  we need to get the deferral first.
            //DataRequestDeferral deferral = args.Request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            //try
            //{
            //    StorageFile thumbnailFile = await Package.Current.InstalledLocation.GetFileAsync(selectedItem.Image.ToString());
            //    args.Request.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(thumbnailFile);
            //    StorageFile imageFile = await Package.Current.InstalledLocation.GetFileAsync(selectedItem.Image.ToString());
            //    args.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            //}
            //finally
            //{
            //    deferral.Complete();
            //}

            ////args.Request.Data.SetBitmap(selectedItem.Image);
            ////args.Request.Data.SetUri(selectedItem.
        }
    }
}
