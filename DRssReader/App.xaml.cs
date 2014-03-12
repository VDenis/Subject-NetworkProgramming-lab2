using DRssReader.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage;
using System.Xml.Linq;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace DRssReader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        public static async Task<bool> CopyConfigToLocalFolder()
        {
            //получаем папку с именем Data в локальной папке приложения
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);

            //получаем список файлов в папке Data
            var files = await localFolder.GetFilesAsync();

            //получаем список всех файлов, имя которых config.xml
            var config = from file in files
                         where file.Name.Equals("config.xml")
                         select file;


            //нам возращается IEnumrable - а он гарантирует тольок один проход
            //копируем в массив - если не беспокоитесь об этом - просто уберите эту строчку
            //а в условии проверяйте config.Count()
            // if (config.Count() == 0) { }
            var configEntries = config as StorageFile[] ?? config.ToArray();

            //то же самое, что config.Count() == 0, но гарантиует от странных ошибок
            //т.е. в целом мы проверили, что файла config.xml нет в подпапке Data
            //папки локальных данных приложения

            //получаем папку Data из установленого приложения
            var dataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
            //получаем файл сonfig.xml
            var configFile = await dataFolder.GetFileAsync("config.xml");

            if (!configEntries.Any())
            {
                //копируем его в локальную папку данных
                await configFile.CopyAsync(localFolder);
                return true;
            }
            else
            {
                var lastModifiedOfCEntries = await configEntries[0].GetBasicPropertiesAsync();
                var lastModifiedOfCFile = await configFile.GetBasicPropertiesAsync();

                if (lastModifiedOfCEntries.DateModified < lastModifiedOfCFile.DateModified)
                {
                    //копируем его в локальную папку данных
                    //await configFile.CopyAsync(localFolder);
                    await configFile.CopyAndReplaceAsync(configEntries[0]);
                    return true;
                }
            }



            return false;
        }

        public static async Task<IEnumerable<Feed>> ReadSettings()
        {
            //получаем папку в которой находится наш файл конфигурации
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
              ("Data", CreationCollisionOption.OpenIfExists);

            //получаем список файлов в папке Data
            var files = await localFolder.GetFilesAsync();

            //получаем список всех файлов, имя которых config.xml
            var config = from file in files
                         where file.Name.Equals("config.xml")
                         select file;


            //нам возращается IEnumrable - а он гарантирует тольок один проход
            //копируем в массив - если не беспокоитесь об этом - просто уберите эту строчку
            //а в условии проверяйте config.Count()
            // if (config.Count() == 0) { }
            var configEntries = config as StorageFile[] ?? config.ToArray();

            //то же самое, что config.Count() == 0, но гарантиует от странных ошибок
            //т.е. в целом мы проверили, что файла config.xml нет в подпапке Data
            //папки локальных данных приложения
            //if (!configEntries.Any())
                await CopyConfigToLocalFolder();

            //получаем конфигурационный файл
            var configFile = await localFolder.GetFileAsync("config.xml");
            //считываем его как текст
            var configText = await FileIO.ReadTextAsync(configFile);
            //загружаем его как XML
            XElement configXML = XElement.Parse(configText);

            //разбираем XML инициализируя данным массив
            var feeds =
                from feed in configXML.Descendants("feed")
                select new Feed
                {
                    id = feed.Element("id").Value,
                    title = feed.Element("title").Value,
                    url = feed.Element("url").Value,
                    description = feed.Element("description").Value,
                    type = feed.Element("type").Value,
                    view = feed.Element("view").Value,
                    policy = feed.Element("policy").Value
                };

            //отдаем наружу массив с конфигурацией RSS потоков
            return feeds;
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            // event in OnWindowCreated to speed up searches once the application is already running

            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // If the app does not contain a top-level frame, it is possible that this 
            // is the initial launch of the app. Typically this method and OnLaunched 
            // in App.xaml.cs can call a common method.
            if (frame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                frame = new Frame();
                DRssReader.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await DRssReader.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (DRssReader.Common.SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
            }

            frame.Navigate(typeof(MySearchResultsPage), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}
