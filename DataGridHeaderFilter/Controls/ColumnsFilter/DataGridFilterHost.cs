using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Tongyu.Smart.Models;

namespace Tongyu.Smart.Client.Controls
{
    /// <summary>
    /// 此类为DataGrid的所有扩展操作类,
    /// </summary>
    public sealed class DataGridFilterHost
    {
        #region Ctor
        public DataGridFilterHost(DataGrid dataGrid)
        {
            this._dataGrid = dataGrid;
            _window = Window.GetWindow(dataGrid);
            _window.Unloaded += _window_Unloaded;

            Deserialize();
        }
        #endregion

        #region public Property
        /// <summary>
        /// 序列化集合
        /// </summary>
        public List<ColumnFilterGroups> FilterGroups { get; private set; } = new List<ColumnFilterGroups>();
        #endregion

        #region private Property
        private readonly Window _window;
        private readonly DataGrid _dataGrid;

        //DataGrid中的所有列
        private readonly List<PopupColumnFilter> _filterColumnControls = new List<PopupColumnFilter>();
        #endregion

        #region 序列化

        private void Serialize()
        {
            try
            {
                foreach (var itemControl in _filterColumnControls)
                {
                    List<ColumnFilter> columnFilters = itemControl.GetSelectedItems();
                    if (columnFilters != null)
                    {
                        var list = columnFilters.Where(x => x.Tag != ColumnFilterTag.All).ToList();
                        if (list?.Count > 0)
                        {
                            var first = FilterGroups.FirstOrDefault(x => x.SortMemberPath == itemControl.PropertyPath);
                            if (first == null)
                            {
                                ColumnFilterGroups groups = new ColumnFilterGroups();
                                groups.SortMemberPath = itemControl.PropertyPath;
                                groups.ColumnFilters = list;
                                FilterGroups.Add(groups);
                            }
                            else
                            {
                                first.ColumnFilters = list;
                            }
                        }
                    }
                    else
                    {
                       var first = FilterGroups.FirstOrDefault(x => x.SortMemberPath == itemControl.PropertyPath);
                        if (first != null)
                            FilterGroups.Remove(first);
                    }
                }

                if (FilterGroups.Count > 0)
                {
                    var obj = XmlSerializerExtension.Serialize(FilterGroups, @".\DataGridColumnFilter.config");
                }
                else
                {
                    if (File.Exists(@".\DataGridColumnFilter.config"))
                    {
                        File.Delete(@".\DataGridColumnFilter.config");
                    }
                }
            }
            catch (Exception)
            {

            }
          
        }

        private void Deserialize()
        {
            try
            {
                if (File.Exists(@".\DataGridColumnFilter.config"))
                {
                    var obj = XmlSerializerExtension.Deserialize<List<ColumnFilterGroups>>(@".\DataGridColumnFilter.config");
                    if (obj?.Count > 0 && FilterGroups != null)
                    {
                        FilterGroups.Clear();
                    }
                    FilterGroups = obj;
                }
            }
            catch (Exception)
            {
                File.Delete(@".\DataGridColumnFilter.config");
            }          
        }
        #endregion

        #region Event Handle
        private void _window_Unloaded(object sender, RoutedEventArgs e)
        {
            //序列化
            Serialize();
        }
        #endregion

        #region Public Motheds
        //启动筛选
        internal void Enable(bool value)
        {
            //筛选DataGrid
            if (value && _filterColumnControls?.Any() == true)
                Filter();                
        }

        internal void AddColumnControl(PopupColumnFilter dataGridFilterColumn)
        {
            var item = FilterGroups?.FirstOrDefault(x => x.SortMemberPath == dataGridFilterColumn.PropertyPath);
            dataGridFilterColumn.SetSelectedItems(item?.ColumnFilters);
            if (_filterColumnControls != null || _filterColumnControls.First(x => x == dataGridFilterColumn) == null)
                _filterColumnControls.Add(dataGridFilterColumn);
        }
        #endregion

        #region Filter

        internal void Filter()
        {
            var filterColumnControls = _filterColumnControls.Where(col => col.GetSelectedItemsCount()>0);
            _dataGrid.Items.Filter = CreatePredicate(filterColumnControls?.ToList());
        }

        private Predicate<object> CreatePredicate(List<PopupColumnFilter> columnControls)
        {
            if (columnControls == null || columnControls.Count <= 0)
            {
                return item => true;
            }
            if (columnControls?.Any() != true)
            {
                return item => columnControls.All(filter => true);
            }
            return item => columnControls.All(filter => filter.Matches(item));
        }
        #endregion
    }
}
