using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

using Windows.Web.Syndication;//--for RSS
using System.Threading.Tasks;//--for RSS

using System.Text.RegularExpressions;//--for picture
using System.IO;
using Windows.Storage;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;//--for picture

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace DRssReader.Data
{
    /// <summary>
    /// Base class for <see cref="RSSDataItem"/> and <see cref="RSSDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class RSSDataCommon : DRssReader.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public RSSDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;

        public string GetImagePath
        {
            get
            {
                return _imagePath;
            }
        }

        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(RSSDataCommon._baseUri, this._imagePath));
                }

                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
                
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }

        protected IList<SyndicationLink> _sourceLinks = null;

        public IList<SyndicationLink> GetSourceLinks
        {
            get
            {
                return _sourceLinks;
            }
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class RSSDataItem : RSSDataCommon
    {
        public RSSDataItem(String uniqueId, String title, String subtitle, String imagePath, IList<SyndicationLink> sourceLinks, String description, String content, RSSDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
            this._sourceLinks = sourceLinks;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private RSSDataGroup _group;
        public RSSDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class RSSDataGroup : RSSDataCommon
    {
        public RSSDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<RSSDataItem> _items = new ObservableCollection<RSSDataItem>();
        public ObservableCollection<RSSDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<RSSDataItem> _topItem = new ObservableCollection<RSSDataItem>();
        public ObservableCollection<RSSDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class RSSDataSource
    {
        public static readonly ObservableCollection<RSSDataGroup> AllGroups = new ObservableCollection<RSSDataGroup>();

        public static IEnumerable<RSSDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return AllGroups;
        }

        public static RSSDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static RSSDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private static string GetImageFromPostContents(SyndicationItem item)
        {
            string text2search = "";

            if (item.Content != null) text2search += item.Content.Text;
            if (item.Summary != null) text2search += item.Summary.Text;

            return Regex.Matches(text2search,
                    @"(?<=<img\s+[^>]*?src=(?<q>['""]))(?<url>.+?)(?=\k<q>)",
                    RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Where(m =>
                {
                    Uri url;
                    if (Uri.TryCreate(m.Groups[0].Value, UriKind.Absolute, out url))
                    {
                        string ext = Path.GetExtension(url.AbsolutePath).ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".bmp") return true;
                    }
                    return false;
                })
                .Select(m => m.Groups[0].Value)
                .FirstOrDefault();
        }

        private static string StripHTML(string str) // Метод очищающий html от тегов
        {
            // буфер для хранения результата
            string strippedString;
            try
            {
                string pattern = "<.*?>";
                // удаляем HTML-теги
                strippedString = Regex.Replace(str, pattern, string.Empty);
            }
            catch
            {
                strippedString = string.Empty;
            }
            return strippedString;
        }

        public static async Task<bool> AddGroupForFeedAsync(string feedUrl, string ID, string Title)
        {
            string clearedContent = String.Empty;

            if (RSSDataSource.GetGroup(feedUrl) != null) return false;

            var feed = await new SyndicationClient().RetrieveFeedAsync(new Uri(feedUrl));

            //получаем папку с именем Data в локальной папке приложения
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
              ("Data", CreationCollisionOption.OpenIfExists);
            //получаем/перезаписываем файл с именем "ID".rss
            var fileToSave = await localFolder.CreateFileAsync(ID + ".rss", CreationCollisionOption.ReplaceExisting);

            //сохраняем фид в этот файл
            await feed.GetXmlDocument(SyndicationFormat.Rss20).SaveToFileAsync(fileToSave);

            var feedGroup = new RSSDataGroup(
                uniqueId: feedUrl,
                title: feed.Title != null ? Title : null,
                subtitle: feed.Subtitle != null ? feed.Subtitle.Text : null,
                imagePath: feed.ImageUri != null ? feed.ImageUri.ToString() : null,
                description: null);

            int tempId = 0;

            foreach (var i in feed.Items)
            {
                if (i.Id == "" || i.Id == null)
                {
                    i.Id = " " + feedGroup.Title.ToString() + "dsafghjgefg" + (tempId++).ToString();
                }
                else
                {
                    i.Id += " " + feedGroup.Title.ToString() + "dsafghjgefg";
                }
                
                //string temp = i.NodeValue;

                var tempStringLinks = i.Links;
                var tempUriLinks =  tempStringLinks[0].Uri;

                string imagePath = GetImageFromPostContents(i);

                if (i.Summary != null)
                    //clearedContent = i.Summary.Text;
                        {
                            clearedContent = i.Summary.Text;
                            int posotion = -1;
                            posotion = clearedContent.IndexOf("Читать далее");
                            if (posotion != -1)
                                clearedContent = clearedContent.Remove(posotion);;
                            //clearedContent = clearedContent.Remove(posotion);
                        }
                    //clearedContent = StripHTML(i.Summary.Text);
                //clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Summary.Text);
                else
                    if (i.Content != null)
                        //clearedContent = i.Content.Text;
                        {
                            clearedContent = i.Content.Text;
                            int posotion = -1;
                            posotion = clearedContent.IndexOf("Читать далее");
                            if (posotion != -1)
                                clearedContent = clearedContent.Remove(posotion);
                        }
                clearedContent = StripHTML(clearedContent);
                        //clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Content.Text);
                if (imagePath != null && feedGroup.Image == null)
                    feedGroup.SetImage(imagePath);

                if (imagePath == null) imagePath = "ms-appx:///Assets/DarkGray.png";

                feedGroup.Items.Add(new RSSDataItem(
                    uniqueId: i.Id, title: feedGroup.Title + ": " + i.Title.Text, subtitle: null, imagePath: imagePath,
                    description: null, content: clearedContent, @group: feedGroup, sourceLinks: i.Links));
            }

            AllGroups.Add(feedGroup);
            return true;
        }

        public static async Task<bool> AddGroupForFeedAsync(StorageFile sf)
        {
            string clearedContent = String.Empty;

            if (RSSDataSource.GetGroup(sf.DisplayName) != null) return false;

            var feed = new SyndicationFeed();
            feed.LoadFromXml(await XmlDocument.LoadFromFileAsync(sf));

            var feedGroup = new RSSDataGroup(
                uniqueId: sf.DisplayName,
                title: feed.Title != null ? feed.Title.Text : null,
                subtitle: feed.Subtitle != null ? feed.Subtitle.Text : null,
                imagePath: feed.ImageUri != null ? feed.ImageUri.ToString() : null,
                description: null);

            int tempId = 0;

            foreach (var i in feed.Items)
            {
                if (i.Id == "" || i.Id == null)
                {
                    i.Id = " " + feedGroup.Title.ToString() + "dsafghjgefg" + (tempId++).ToString();
                }
                else
                {
                    i.Id += " " + feedGroup.Title.ToString() + "dsafghjgefg";
                }

                
                //string temp = i.NodeValue;
                string imagePath = GetImageFromPostContents(i);

                if (i.Summary != null)
                {
                    clearedContent = i.Summary.Text;
                    int posotion = -1;
                    posotion = clearedContent.IndexOf("Читать далее");
                    if (posotion != -1)
                        clearedContent = clearedContent.Remove(posotion);
                }
                //clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Summary.Text);
                else
                    if (i.Content != null)
                    {
                        clearedContent = i.Content.Text;
                        int posotion = -1;
                        posotion = clearedContent.IndexOf("Читать далее");
                        if (posotion != -1)
                            clearedContent = clearedContent.Remove(posotion);
                    }
                    //clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Content.Text);
                clearedContent = StripHTML(clearedContent);

                if (imagePath != null && feedGroup.Image == null)
                    feedGroup.SetImage(imagePath);

                if (imagePath == null) imagePath = "ms-appx:///Assets/DarkGray.png";

                feedGroup.Items.Add(new RSSDataItem(
                    uniqueId: i.Id, title: feedGroup.Title + ": " + i.Title.Text, subtitle: null, imagePath: imagePath,
                    description: null, content: clearedContent, @group: feedGroup, sourceLinks: i.Links));
            }

            AllGroups.Add(feedGroup);
            return true;
        }

        public static void UpdateTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
//            var news = RSSDataSource.AllGroups[0].Items.ToList();
//            var xml = new XmlDocument();
//            xml.LoadXml(
//                string.Format(
//@"<?xml version=""1.0"" encoding=""utf-8"" ?>
//<tile>
//<visual >
//<binding template=""TileSquarePeekImageAndText03"">
//<image id=""1"" src=""ms-appx:///Assets/__Logo.png"" alt=""alt text""/>
//<text id=""1"">{0}</text>
//<text id=""2"">{1}</text>
//<text id=""3"">{2}</text>
//<text id=""4"">{3}</text>

//</binding>
//<binding template=""TileWidePeekImageAndText02"">
//<image id=""1"" src=""ms-appx:///Assets/__WideLogo.png"" alt=""alt text""/>
//<text id=""1"">{0}</text>
//<text id=""2"">{1}</text>
//<text id=""3"">{2}</text>
//<text id=""4"">{3}</text>
//</binding>
//</visual>
//</tile>",
//                    news.Count > 0 ? System.Net.WebUtility.HtmlEncode(news[0].Title) : "",
//                    news.Count > 1 ? System.Net.WebUtility.HtmlEncode(news[1].Title) : "",
//                    news.Count > 2 ? System.Net.WebUtility.HtmlEncode(news[2].Title) : "",
//                    news.Count > 3 ? System.Net.WebUtility.HtmlEncode(news[3].Title) : ""));
//            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));
            var news = RSSDataSource.AllGroups[0].Items.ToList();
            var xml = new XmlDocument();
            var xml1 = new XmlDocument();
            var xml2 = new XmlDocument();
            var xml3 = new XmlDocument();

//            string xmlBilder = string.Format(
//@"<?xml version=""1.0"" encoding=""utf-8"" ?>
//<tile>
//<visual>
//<binding template=""TileSquarePeekImageAndText04"">
//<image id=""1"" src=""{0}"" alt=""alt text""/>
//<text id=""1"">{1}</text>
//</binding>
//<binding template=""TileWideSmallImageAndText03"">
//<image id=""1"" src=""{0}"" alt=""alt text""/>
//<text id=""1"">{1}</text>
//</binding>
//</visual>
//</tile>");

            var imageWebPath = news.Count > 0 ? news[0].GetImagePath : @"Assets\__Logo.png";
            var liveTitle = news.Count > 0 ? System.Net.WebUtility.HtmlEncode(news[0].Title) : "";

            var imageWebPath1 = news.Count > 0 ? news[1].GetImagePath : @"Assets\__Logo.png";
            var liveTitle1 = news.Count > 0 ? System.Net.WebUtility.HtmlEncode(news[1].Title) : "";

            var imageWebPath2 = news.Count > 0 ? news[2].GetImagePath : @"Assets\__Logo.png";
            var liveTitle2 = news.Count > 0 ? System.Net.WebUtility.HtmlEncode(news[2].Title) : "";

            var imageWebPath3 = news.Count > 0 ? news[3].GetImagePath : @"Assets\__Logo.png";
            var liveTitle3 = news.Count > 0 ? System.Net.WebUtility.HtmlEncode(news[3].Title) : "";

            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            xml.LoadXml(
                string.Format(
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<tile>
<visual branding=""none"">
<binding template=""TileSquarePeekImageAndText04"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
<binding template=""TileWideSmallImageAndText03"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
</visual>
</tile>", imageWebPath, liveTitle));

            
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));

            xml.LoadXml(
                string.Format(
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<tile>
<visual branding=""none"">
<binding template=""TileSquarePeekImageAndText04"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
<binding template=""TileWideSmallImageAndText03"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
</visual>
</tile>", imageWebPath1, liveTitle1));
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));
            xml.LoadXml(
                string.Format(
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<tile>
<visual branding=""none"">
<binding template=""TileSquarePeekImageAndText04"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
<binding template=""TileWideSmallImageAndText03"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
</visual>
</tile>", imageWebPath2, liveTitle2));
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));
            xml.LoadXml(
                string.Format(
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<tile>
<visual branding=""none"">
<binding template=""TileSquarePeekImageAndText04"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
<binding template=""TileWideSmallImageAndText03"">
<image id=""1"" src=""{0}"" alt=""alt text""/>
<text id=""1"">{1}</text>
</binding>
</visual>
</tile>", imageWebPath3, liveTitle3));
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));

        }
    }


}
