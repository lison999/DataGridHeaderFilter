using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Tongyu.Smart.Client.Controls
{
    [TemplatePart(Name = "PART_ColumnIdBtn", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PopupFilterControl", Type = typeof(PopupColumnFilter))]
    public class DataGridFilterColumnControl:Control
    {
        #region Ctor
       
        static DataGridFilterColumnControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridFilterColumnControl), new FrameworkPropertyMetadata(typeof(DataGridFilterColumnControl)));
        }

        #endregion

        #region Event Handle
        private void PART_ColumnIdBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!PART_PopupFilterControl.IsOpen)
                PART_PopupFilterControl.IsOpen = true;
        }
        #endregion

        #region Private Property 
        private Button PART_ColumnIdBtn;
        private PopupColumnFilter PART_PopupFilterControl;
        #endregion

        #region public Property


        public Brush FillColor
        {
            get { return (Brush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FillColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register("FillColor", typeof(Brush), typeof(DataGridFilterColumnControl), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty IsAllCheckedProperty =
          DependencyProperty.Register("IsAllChecked", typeof(bool?), typeof(DataGridFilterColumnControl), new PropertyMetadata(false,onIsAllCheckedChanged));

        private static void onIsAllCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridFilterColumnControl current = d as DataGridFilterColumnControl;
            current.IsAllCheckedChanged(e.NewValue);         
        }

        private void IsAllCheckedChanged(object b)
        {
            if (b == null || true.Equals(b))
                FillColor = Brushes.Red;
            else
                FillColor = Brushes.Black;
        }
        #endregion

        #region Override
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_ColumnIdBtn = GetTemplateChild("PART_ColumnIdBtn") as Button;
            PART_PopupFilterControl = GetTemplateChild("PART_PopupFilterControl") as PopupColumnFilter;

            PART_ColumnIdBtn.Click += PART_ColumnIdBtn_Click;

            BindingOperations.SetBinding(this, IsAllCheckedProperty, new Binding("IsChecked") { Source = PART_PopupFilterControl._AllCheckBox });
           
        }
        #endregion
    }
    public static class Ext
    {
        internal static T FindColumnHeader<T>(this DependencyObject dependencyObject) where T : class
        {
            while (dependencyObject != null)
            {
                var target = dependencyObject as T;
                if (target != null)
                    return target;

                dependencyObject = LogicalTreeHelper.GetParent(dependencyObject) ?? VisualTreeHelper.GetParent(dependencyObject);
            }
            return null;
        }

        public static T GetValue<T>(this DependencyObject self, DependencyProperty property)
        {
            Contract.Requires(self != null);
            Contract.Requires(property != null);

            return self.GetValue(property).SafeCast<T>();
        }
        public static T SafeCast<T>(this object value)
        {
            return (value == null) ? default(T) : (T)value;
        }
    }
}
