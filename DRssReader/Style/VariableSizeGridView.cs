using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using DRssReader.Data;
using Windows.UI.Xaml;

namespace DRssReader
{
    class VariableSizeGridView : GridView 
    {
        private int rowVal;
        private int colVal;

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            RSSDataItem dataItem = item as RSSDataItem;
            int index = -1;

            int group = -1;

            if (dataItem.Group.UniqueId.Contains("twitter"))
            {
                group = 1;
            }

            if (group < 1)
            {

                if (dataItem != null)
                {
                    index = dataItem.Group.Items.IndexOf(dataItem);

                }

                colVal = 2;
                rowVal = 2;

                //if (index == 0)
                //{
                //    colVal = 2;
                //    rowVal = 3;
                //}
                //if (index == 1)
                //{
                //    colVal = 1;
                //    rowVal = 1;
                //}

                //if (index == 2)
                //{
                //    colVal = 2;
                //    rowVal = 1;
                //}

                //if (index == 3)
                //{
                //    colVal = 2;
                //    rowVal = 1;
                //}

                //if (index == 4)
                //{
                //    colVal = 1;
                //    rowVal = 1;
                //}

                //if (index == 5)
                //{
                //    colVal = 1;
                //    rowVal = 1;
                //}

                //if (index == 6)
                //{
                //    colVal = 1;
                //    rowVal = 2;
                //}

                //if (index == 7)
                //{
                //    colVal = 1;
                //    rowVal = 1;
                //}
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);
            }
            else
            {
                colVal = 2;
                rowVal = 1;
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
