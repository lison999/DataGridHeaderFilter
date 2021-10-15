using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGridHeaderFilter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Random _rand = new Random();
        public MainWindow()
        {
            InitializeComponent();

            _dataGrid.DataContext = Items;
        }

        public IEnumerable<DataItem> Items
        {
            get
            {
                return Enumerable.Range(0, 500).Select(index => new DataItem(_rand, index)).ToArray();
            }
        }
    }

    public class DataItem
    {
        private static string[] _samples = new[] { "lorem", "ipsum", "dolor", "sit", "amet" };

        private static string[] _clientId = new[] { "lorem-2021", "ipsum-2021", "dolor-2021", "sit-2021", "amet-2021" };

        private static string[] _TradeSymbol = new[] { "600519", "600660", "600606", "612126", "312126", "300621", "315621" };

        public DataItem(Random rand, int index)
        {
            Flag = rand.Next(2) == 0;
            Index = index;
            ClientId = _samples[rand.Next(_samples.Length)];
            FundAccount = _clientId[rand.Next(_samples.Length)];
            TradeSymbol = _TradeSymbol[rand.Next(_samples.Length)];         
        }

        public bool Flag { get; private set; }
        public int Index { get; private set; }
        public string ClientId { get; set; }
        public string FundAccount { get; set; }
        public string TradeSymbol { get; set; }     
    }
}
