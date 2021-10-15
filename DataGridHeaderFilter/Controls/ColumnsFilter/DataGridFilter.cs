using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Tongyu.Smart.Client.Controls
{
    public static class DataGridFilter
    {
        #region CurrentDataGridItemSource Attached
        public static IEnumerable GetCurrentDataGridItemSource(DependencyObject obj)
        {
            return (IEnumerable)obj.GetValue(CurrentDataGridItemSourceProperty);
        }

        public static void SetCurrentDataGridItemSource(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(CurrentDataGridItemSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for CurrentDataGridItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentDataGridItemSourceProperty =
            DependencyProperty.RegisterAttached("CurrentDataGridItemSource", typeof(IEnumerable), typeof(DataGridFilter), new PropertyMetadata(null, OnCurrentDataGridItemSourceChanged));

        public static void OnCurrentDataGridItemSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var datagrid = sender as DataGrid;
            datagrid?.GetFilter()?.Enable(true.Equals(args.NewValue == null ? false : args.NewValue));
        }
        #endregion

        #region Filter Attached

        public static DataGridFilterHost GetFilter(this DataGrid dataGrid)
        {
            var value = (DataGridFilterHost)dataGrid.GetValue(FilterProperty);
            if(value==null)
            {
                value = new DataGridFilterHost(dataGrid);
                dataGrid.SetValue(FilterProperty, value);
            }
            return value;
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterHost), typeof(DataGridFilterHost));


        #endregion
    }
}
