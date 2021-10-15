using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tongyu.Smart.Models;

namespace Tongyu.Smart.Client.Controls
{
    /// <summary>
    /// PopupColumnFilter.xaml 的交互逻辑
    /// </summary>
    public partial class PopupColumnFilter : Popup
    {
        #region Private Property
        private Cursor _cursor;

        protected DataGridFilterHost FilterHost { get; private set; }
        protected DataGrid DataGrid { get; private set; }
        protected DataGridColumnHeader ColumnHeader { get; private set; }

        //是否为初次加载，设置此属性是因为Load事件会在IsOpen=true时重新加载，在Theme变更时也会重新加载
        private bool IsLoad = false;
        //点击全选按钮时，CheckboxListView的ItemsChanged不重复Filter
        private bool isAllSelected = false;

        /// <summary>
        /// list DataContext
        /// </summary>
        protected ObservableCollection<ColumnFilter> columnFilters { get; private set; }

        //Item Source Path
        public string PropertyPath { get; private set; }

        /// <summary>
        /// 选中的列
        /// </summary>
        protected List<ColumnFilter> selectedColumnFilters { get; private set; }

        //全选个数
        public string AllControlContent { get; private set; } = "0";
        #endregion

        #region Ctor
        public PopupColumnFilter()
        {
            InitializeComponent();
            this.Loaded += DataGridFilterColumnControl_Loaded;
        }
        #endregion

        #region Private Motheds

        #region SetDataConText

        private void SetDataContext()
        {
            if (string.IsNullOrEmpty(PropertyPath))
                return;

            if (DataGrid.ItemsSource == null)
            {
                return;
            }

            if (!IsLoad)
            {
                SetSelectedFilter();
            }
            IsLoad = false;

            if (columnFilters == null)
            {
                columnFilters = new ObservableCollection<ColumnFilter>();
            }
            else
            {
                columnFilters.Clear();
            }

            int count = 0;
            foreach (var item in DataGrid.ItemsSource)
            {
                count++;
                BindingOperations.SetBinding(this, _cellValueProperty, new Binding(PropertyPath) { Source = item });
                var propertyValue = GetValue(_cellValueProperty);

                //--------------------------读取上次数据记录-----------------------------
                var first = columnFilters.FirstOrDefault(x => x.Title == propertyValue.ToString());
                if (first == null)
                {
                    bool isSelected = false;
                    var obj = selectedColumnFilters?.FirstOrDefault(x => x.Title == propertyValue.ToString());
                    if (obj != null)
                    {
                        isSelected = true;
                    }

                    ColumnFilter columnFilter = new ColumnFilter(isSelected, propertyValue.ToString(), 1);
                    columnFilters.Add(columnFilter);
                }
                else
                {
                    first.Count++;
                }
                //--------------------------------------------------------------------------------                
            }

            AllControlContent = count == 0 ? "0" : count.ToString();
            _AllCountText.DataContext = AllControlContent;
            checkedListView.DataContext = columnFilters;
        }

        private void SetSelectedFilter()
        {
            if (selectedColumnFilters == null) selectedColumnFilters = new List<ColumnFilter>();
            selectedColumnFilters.Clear();
            //读取上次勾选记录
            if (checkedListView.SelectedItems != null && checkedListView.SelectedItems.Count > 0)
            {
                selectedColumnFilters.Clear();
                foreach (var item in checkedListView.SelectedItems)
                {
                    selectedColumnFilters.Add(item as ColumnFilter);
                }
            }
        }
        #endregion

        #region Event Handle
        private void DataGridFilterColumnControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (FilterHost == null)
            {
                ColumnHeader = this.FindColumnHeader<DataGridColumnHeader>();
                if (ColumnHeader == null)
                    return;

                DataGrid = ColumnHeader.FindColumnHeader<DataGrid>();
                if (DataGrid == null)
                    return;

                FilterHost = DataGrid.GetFilter();
            }

            PropertyPath = ColumnHeader?.Column.SortMemberPath;
            _AllCountText.DataContext = AllControlContent;            

            FilterHost.AddColumnControl(this);

            SetTheme();

            this.Opened += PopupColumnFilter_Opened;

            SetDataContext();
            FilterHost.Filter();
        }

        private void PopupColumnFilter_Opened(object sender, EventArgs e)
        {
            SetDataContext();
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            checkedListView.UnselectAll();
        }

        #region AllViewControlEventHandle
        private void _contentAllView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AllViewEventHandle();
            e.Handled = true;
        }

        private void AllViewEventHandle()
        {
            isAllSelected = true;
            if (_contentAllView.IsSelected)
            {
                //列表全选
                checkedListView.UnselectAll();
            }
            else
            {
                checkedListView.SelectAll();
            }
        }

        private void _AllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            AllViewEventHandle();
        }

        #endregion
        #endregion

        #region Filter

        private void CurrentFilter()
        {
            SetSelectedFilter();
            FilterHost?.Filter();
        }

        private void OnCheckItem(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(columnFilters.Where(x => x.IsSelected).Count().ToString());
                      
            int count = columnFilters.Where(x => x.IsSelected).Count();

            if (isAllSelected)
            {
                //全选
                if (count == columnFilters.Count)
                {
                    CurrentFilter();

                    isAllSelected = false;
                    _AllCheckBox.IsChecked = true;
                    _contentAllView.IsSelected = true;
                }
                //全不选
                else if (count == 0)
                {
                    CurrentFilter();

                    isAllSelected = false;
                    _AllCheckBox.IsChecked = false;
                    _contentAllView.IsSelected = false;
                }
            }
            else
            {
                if (count != 0 && count != columnFilters.Count)
                {
                    _contentAllView.IsSelected = false;
                    _AllCheckBox.IsChecked = null;
                }
                else if (count == columnFilters.Count)
                {
                    _contentAllView.IsSelected = true;
                    _AllCheckBox.IsChecked = true;
                }
                else if (count == 0)
                {
                    _contentAllView.IsSelected = false;
                    _AllCheckBox.IsChecked = false;
                }

                CurrentFilter();
            }
        }

        #region Filter Motheds
        /// <summary>
        /// 筛选方法
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Matches(object item)
        {
            if (FilterHost == null)
            {
                return true;
            }
            return IsMatch(GetCellContent(item));
        }

        public bool IsMatch(object value)
        {
            if (value == null)
                return false;

            int count = columnFilters?.Where(x => x.IsSelected) == null ? 0 : columnFilters.Where(x => x.IsSelected).Count();
            //Debug.WriteLine($"IsSelected:{ columnFilters.Where(x => x.IsSelected).Count()}  selectedColumnFilters:{selectedColumnFilters.Count}");
            if (selectedColumnFilters == null || selectedColumnFilters.Count <= 0 || count == 0 || columnFilters?.Count == count)
            {
                return true;
            }

            //if (selectedColumnFilters.FirstOrDefault(x => x.Title == "All" && x.Tag == ColumnFilterTag.All)!=null);
            //{
            //    return true;
            //}

            //List<ColumnFilter> filterValues = new List<ColumnFilter>();
            //foreach (var item in checkedListView.SelectedItems)
            //{
            //    ColumnFilter str = item as ColumnFilter;
            //    if (str != null)
            //        filterValues.Add(str);
            //}

            return selectedColumnFilters.Find(f => f.Title.IndexOf(value.ToString(), StringComparison.CurrentCultureIgnoreCase) >= 0) == null ? false : true;
        }

        private static readonly DependencyProperty _cellValueProperty =
            DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterColumnControl));

        protected object GetCellContent(object item)
        {
            if (string.IsNullOrEmpty(PropertyPath))
                return null;

            BindingOperations.SetBinding(this, _cellValueProperty, new Binding(PropertyPath) { Source = item });
            var propertyValue = GetValue(_cellValueProperty);
            BindingOperations.ClearBinding(this, _cellValueProperty);

            return propertyValue;
        }
        #endregion
        #endregion

        #region SearchBoxCommand
        private ICommand _textBoxButtonCmd;
        public ICommand TextBoxButtonCmd
        {
            get
            {
                if (_textBoxButtonCmd == null)
                {
                    //_textBoxButtonCmd = new DelegateCommand<string>(TextBoxButtonCmdExecute);
                    _textBoxButtonCmd = new SimpleCommand(o => true, TextBoxButtonCmdExecute);
                }
                return _textBoxButtonCmd;
            }
        }

        private void TextBoxButtonCmdExecute(object obj)
        {
            try
            {
                string condition = string.Empty;
                if (obj is string str)
                {
                    condition = str;
                }

                checkedListView.Items.Filter = CreatePredicate(condition);
            }
            catch
            {

            }
        }

        private Predicate<object> CreatePredicate(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return item => true;
            }
            return item =>
            {
                ColumnFilter filters = item as ColumnFilter;
                if (filters == null) return true;
                return filters.Title.IndexOf(condition, StringComparison.CurrentCultureIgnoreCase) >= 0;
            };
        }

        #endregion

        #region Rasize


        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _cursor = Cursor;
            Cursor = Cursors.SizeNWSE;
        }

        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Cursor = _cursor;
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double yAdjust = _popupContentGrid.Height + e.VerticalChange;
            double xAdjust = _popupContentGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or heigth    
            xAdjust = (_popupContentGrid.ActualWidth + xAdjust) > PoupuWin.MinWidth ? xAdjust : PoupuWin.MinWidth;
            yAdjust = (_popupContentGrid.ActualHeight + yAdjust) > PoupuWin.MinHeight ? yAdjust : PoupuWin.MinHeight;

              xAdjust = xAdjust < 300 ? 300 : xAdjust;
              yAdjust = yAdjust < 300 ? 300 : yAdjust;

            _popupContentGrid.Width = xAdjust;
            _popupContentGrid.Height = yAdjust;
        }
        #endregion

        #region Theme
        private void SetTheme(string themeName="Dark")
        {
            if (themeName == "Dark")
            {
                this._popupBorder.Background = searchTextBox.Background;
                _popupContentGrid.Background = searchTextBox.Background;
                _popupGridBtn.Background = searchTextBox.Background;
            }
            else
            {
                this._popupBorder.Background = searchTextBox.Background;
                _popupContentGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
                _popupGridBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
            }
        }
        #endregion

        #endregion

        #region public Motheds
        public int GetSelectedItemsCount()
        {
            return selectedColumnFilters == null || selectedColumnFilters.Count <= 0 ? 0 : selectedColumnFilters.Count;
        }

        public List<ColumnFilter>  GetSelectedItems()
        {
            var list = columnFilters.Where(x => x.IsSelected).ToList();

            if (list == null || list.Count == 0)
                return null;
            return list;
        }

        public void SetSelectedItems(List<ColumnFilter> columnFilters)
        {
            if (columnFilters != null && columnFilters.Count>0)
            {
                IsLoad = true;
                selectedColumnFilters = columnFilters;

                //这里设置空是为了在打开DataGrid页面时，filterControl属性FillColor就会触发，从而起到初始化时的效果
                _AllCheckBox.IsChecked = null;
            }
        }
        #endregion        
    }

    /// <summary>
    /// 用于序列化
    /// </summary>
    public class ColumnFilterGroups
    {
        public string SortMemberPath { get; set; }

        public List<ColumnFilter> ColumnFilters { get; set; } = new List<ColumnFilter>();
    }

    public class ColumnFilter: BindableBase
    {
        private bool _IsSelected;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; RaisePropertyChanged(nameof(IsSelected)); }
        }

        public string Title { get; set; }

        public int Count { get; set; }

        public ColumnFilterTag Tag { get; set; }

        public ColumnFilter()
        {

        }
        public ColumnFilter(bool isSelected, string title, int count,ColumnFilterTag tag=ColumnFilterTag.Single)
        {
            this.IsSelected = isSelected;
            this.Title = title;
            this.Count = count;
            this.Tag = tag;
        }
    }

    public enum ColumnFilterTag
    { 
        All=0,
        Single = 1
    }
}
