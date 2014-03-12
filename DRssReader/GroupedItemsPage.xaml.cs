using DRssReader.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;

using Windows.System;
using Windows.UI.ApplicationSettings;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.Storage;
using Callisto.Controls;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace DRssReader
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedItemsPage : DRssReader.Common.LayoutAwarePage
    {
        public GroupedItemsPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
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
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            RSSDataSource.AllGroups.Clear(); 
            if (!ConnectedToInternet())
            {
                //получаем папку с именем Data в локальной папке приложения
                var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
                   ("Data", CreationCollisionOption.OpenIfExists);

                //получаем список файлов в папке Data
                var cachedFeeds = await localFolder.GetFilesAsync();

                //получаем список всех файлов, имя которых config.xml
                var feedsToLoad = from feeds in cachedFeeds
                                  where feeds.Name.EndsWith(".rss")
                                  select feeds;

                //нам возращается IEnumrable - а он гарантирует тольок один проход
                //копируем в массив                
                var feedsEntries = feedsToLoad as StorageFile[] ?? feedsToLoad.ToArray();

                if (feedsEntries.Any())
                {
                    this.DefaultViewModel["Groups"] = RSSDataSource.AllGroups;

                    foreach (var feed in feedsEntries)
                    {
                        await RSSDataSource.AddGroupForFeedAsync(feed);
                    }

                    zommedOutView.ItemsSource = groupedItemsViewSource.View.CollectionGroups;

                    OfflineMode.Visibility = Visibility.Visible;
                }
                else
                {
                    //var msg = new MessageDialog("The program need an internet connection to work. Please check it and restart the porgram.");
                    var msg = new MessageDialog("Программе требуется интернет соединение для работы. Пожалуйста проверьте ваше интернет соединение и перезапустите программу.");
                    await msg.ShowAsync();
                }

            }
            else
            {

                this.DefaultViewModel["Groups"] = RSSDataSource.AllGroups;
                OfflineMode.Visibility = Visibility.Collapsed;

                var feeds = await App.ReadSettings();

                foreach (var feed in feeds)
                {
                    try
                    {
                        await RSSDataSource.AddGroupForFeedAsync(feed.url, feed.id, feed.title);
                        RSSDataSource.UpdateTile();
                    }
                    catch { }
                }
                
                zommedOutView.ItemsSource = groupedItemsViewSource.View.CollectionGroups;
                
            }
        }


        private bool ConnectedToInternet()
        {
            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null)
            {
                return false;
            }
            var level = profile.GetNetworkConnectivityLevel();
            return level == NetworkConnectivityLevel.InternetAccess;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= Settings_CommandsRequested;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += Settings_CommandsRequested;
            base.OnNavigatedTo(e);
        }

        private void Settings_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            //var viewPrivacyPage = new SettingsCommand("ABOUT_ID", "About", cmd =>
            var viewAboutPage = new SettingsCommand("ABOUT_ID", "О программе", cmd =>
            {
                SettingsFlyout flyout = new SettingsFlyout();
                //flyout.HeaderText = "About"; // NB: should be in a resource again.  
                flyout.HeaderText = "О программе"; // NB: should be in a resource again.  

                flyout.Content = new StackPanel();
                var privacyText = new TextBlock()
                {
                    Text = "  PC Новости Rss представляет собой\n"
                    + "клиент для чтения новостей с сайтов\n"
                    //Text = "  PC News Rss представляет собой\n"
                    //+ "клиент для чтения новостей с сайтов\n"
 //                   + "www.feedburner.com и www.3dnews.ru.\n\n\n"
                    //+ "This is my privacy policy.\n\n"
                    //+ "  PC News Rss uses the internet only to\n"
                    //+ "receive data from www.feedburner.com\n"
                    //+ "and www.3dnews.ru.\n\n"
                    //+ "  The information about you or your\n"
                    //+ "device will not be stored or distributed.\n"
                };
                //var privacyLinkButton = new Button()
                //{
                //    Content = "link to see privacy websites"
                //};

                //privacyLinkButton.Click += Button_Click_1;

                privacyText.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                //privacyLinkButton.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                ((StackPanel)flyout.Content).Children.Add(privacyText);
                //((StackPanel)flyout.Content).Children.Add(privacyLinkButton);
                //-----------------------------------------------------------------------------------------------------------//
                //flyout.Content = new TextBlock()
                //{
                //    Text = "This is my privacy policy\n"
                //    + "This is my privacy policy\n"
                //    + "http://www.dennis.co.uk/privacy\n"
                    
                //};
                //flyout.Content = new Button()
                //{

                //};
                //var goToPrivacyPage1 = new SettingsCommand("PrivacyPolicy", "Privacy policy", tmp =>
                //{
                //    Launcher.LaunchUriAsync(new Uri("http://www.3dnews.ru/copyright/", UriKind.Absolute));
                //    //Launcher.LaunchUriAsync(new Uri("http://www.google.com/policies/privacy/", UriKind.Absolute));
                //    Launcher.LaunchUriAsync(new Uri("http://www.dennis.co.uk/privacy", UriKind.Absolute));
                //});
                //args.Request.ApplicationCommands.Add(goToPrivacyPage1);

                flyout.FontSize = 15;
                flyout.Width = (double)SettingsFlyout.SettingsFlyoutWidth.Wide;
                flyout.IsOpen = true;    

                //Launcher.LaunchUriAsync(new Uri("http://www.3dnews.ru/copyright/", UriKind.Absolute));
                //Launcher.LaunchUriAsync(new Uri("http://www.google.com/policies/privacy/", UriKind.Absolute));
                //Launcher.LaunchUriAsync(new Uri("http://www.dennis.co.uk/privacy", UriKind.Absolute));
            });

            var viewPrivacyPage = new SettingsCommand("PRIVACY_ID", "Соглашение о конфиденциальности", cmd =>
            {
                SettingsFlyout flyout = new SettingsFlyout();
                //flyout.HeaderText = "About"; // NB: should be in a resource again.  
                flyout.HeaderText = "Соглашение о конфиденциальности"; // NB: should be in a resource again.  

                flyout.Content = new StackPanel();
                var privacyText = new TextBlock()
                {
                    //Text = "  PC News Rss представляет собой\n"
                    //+ "клиент для чтения новостей с сайтов\n"


                    Text = "  Privacy Policy\n\n"
                    + "This application does not collect\n"
                    + "or store any private data.\n"
                    + "To provide data for you it requires\n"
                    + "internet connection and uses 3rd-party\n"
                    + "services as data-sources. \n\n\n"
                    + "  Соглашение о конфиденциальности\n\n"
                    + "Данное приложение не собирает и\n"
                    + "не хранит персональные данные.\n"
                    + "Для предоставления Вам информации\n"
                    + "приложение требует интернет-\n"
                    + "-соединение и использует сторонние\n"
                    + "ресурсы, как источники данных.\n"
                };
                var privacyLinkButton = new Button()
                {
                    //Content = "link to see privacy websites"
                    Content = "смотреть на сайте"
                };

                privacyLinkButton.Click += Button_Click_1;

                privacyText.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                ((StackPanel)flyout.Content).Children.Add(privacyText);
                ((StackPanel)flyout.Content).Children.Add(privacyLinkButton);


                flyout.FontSize = 15;
                flyout.Width = (double)SettingsFlyout.SettingsFlyoutWidth.Wide;
                flyout.IsOpen = true;

            });

            args.Request.ApplicationCommands.Add(viewAboutPage);
            args.Request.ApplicationCommands.Add(viewPrivacyPage);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //await Launcher.LaunchUriAsync(new Uri("http://www.3dnews.ru/copyright/", UriKind.Absolute));
            //Launcher.LaunchUriAsync(new Uri("http://www.google.com/policies/privacy/", UriKind.Absolute));
            await Launcher.LaunchUriAsync(new Uri("https://www.dropbox.com/s/qsz3r6ulmx11y8e/PrivacyPolicy.txt?v=0mcn", UriKind.Absolute));
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var group = (sender as FrameworkElement).DataContext;

            //проверяем, что группа отображает сообщения из twitter-а
            //мы точно значем, что из 3-х групп, только у одной есть это слово в названии
            //у той, которая относится к твиттеру
            if (((RSSDataGroup)group).Title.Contains("Twitter"))
            {
                //переходим на странцу для отображения списка твитов и передаем ID группы
                this.Frame.Navigate(typeof(TwitterPage), ((RSSDataGroup)group).UniqueId);
            }
            else
            {
                // Navigate to the appropriate destination page, configuring the new page
                // by passing required information as a navigation parameter
                this.Frame.Navigate(typeof(GroupDetailPage), ((RSSDataGroup)group).UniqueId);
            }
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((RSSDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
        }
    }
}
