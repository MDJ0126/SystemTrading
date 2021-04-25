using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrading.Forms
{
    public partial class SearchStock : Form
    {
        public Action<StockInfo> onSelectStock;

        public SearchStock()
        {
            InitializeComponent();
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            SetSearchList();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSearchList();
            }
        }

        private void SetSearchList()
        {
            if (!string.IsNullOrEmpty(searchTextBox.Text))
            {
                List<StockInfo> results = StockListManager.Instance.SearchStockInfos(searchTextBox.Text);
                if (results != null && results.Count > 0)
                {
                    searchListView.Items.Clear();
                    for (int i = 0; i < results.Count; i++)
                    {
                        ListViewItem listViewItem = new ListViewItem(results[i].tradingSymbol);
                        listViewItem.SubItems.Add(results[i].Name);
                        listViewItem.SubItems.Add("");
                        searchListView.Items.Add(listViewItem);
                    }
                }
            }
        }

        private void searchListView_DoubleClick(object sender, EventArgs e)
        {
            string tradingSymbol = searchListView.FocusedItem.SubItems[0].Text;
            if (!string.IsNullOrEmpty(tradingSymbol))
            {
                StockInfo stockInfo = StockListManager.Instance.GetStockInfo(tradingSymbol);
                if (stockInfo != null)
                {
                    StockListManager.Instance.RequestStockInfo(stockInfo, (result) =>
                    {
                        if (result)
                            onSelectStock?.Invoke(stockInfo);
                    });
                    Close();
                }
            }
        }
    }
}
