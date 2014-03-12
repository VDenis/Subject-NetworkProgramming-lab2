using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DRssReader.Data;

namespace DRssReader
{
    class MyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template1 { get; set; }
        public DataTemplate Template2 { get; set; }
        public DataTemplate Template3 { get; set; }
        
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            RSSDataItem dataItem = item as RSSDataItem;

            string temp = dataItem.Group.Title.ToString();
            if (dataItem.Group.Title.Contains("3D Новости") || dataItem.Group.Title.Contains("Игромания Железо"))
            {
                return Template1;
            }
            else
                if (dataItem.Group.Title.Contains("witter"))
                {
                    return Template3;
                }
                else
                {
                    return Template2;
                }

            //return base.SelectTemplateCore(item, container);
        }
    }
}
