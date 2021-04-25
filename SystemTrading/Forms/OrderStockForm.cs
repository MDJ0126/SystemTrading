using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static StockInfo;

namespace SystemTrading.Forms
{
    public partial class OrderStockForm : Form
    {
        private const int WATCH_MAX = 10;

        public List<StockInfo> _stockInfos = new List<StockInfo>();
        public StockInfo _mainStockInfo = null;

        public OrderStockForm()
        {
            InitializeComponent();
            _stockInfos.Clear();
            FormClosing += OnCloseForm;
            chart.Series["Series1"]["PriceUpColor"] = "Red";
            chart.Series["Series1"]["PriceDownColor"] = "Blue";
            chart.AxisViewChanged += chart_AxisViewChanged;
            UpdateOrderUI();
        }

        private void chart_AxisViewChanged(object sender, ViewEventArgs e)
        {
            if (sender.Equals(chart))
            {
                int startPosition = (int)e.Axis.ScaleView.ViewMinimum;
                int endPosition = (int)e.Axis.ScaleView.ViewMaximum;
                SetChartScaleView(startPosition, endPosition);
            }
        }

        private void SetChartScaleView(int startPosition, int endPosition)
        {
            int? max = null, min = null;
            for (int i = startPosition - 1; i < endPosition; i++)
            {
                if (i >= chart.Series["Series1"].Points.Count) break;
                if (i < 0) i = 0;

                var curMax = (int)chart.Series["Series1"].Points[i].YValues[0];
                var curMin = (int)chart.Series["Series1"].Points[i].YValues[1];

                if (max == null || curMax > max)
                    max = curMax;
                if (min == null || curMin < min)
                    min = curMin;
            }

            chart.ChartAreas[0].AxisY.Maximum = max.Value;
            chart.ChartAreas[0].AxisY.Minimum = min.Value;
        }

        private void OnCloseForm(object sender, FormClosingEventArgs e)
        {
            if (_stockInfos != null)
            {
                _stockInfos.Clear();
                _stockInfos = null;
            }
        }

        /// <summary>
        /// 메인이 되는 UI 업데이트
        /// </summary>
        private void UpdateMainUI()
        {
            if (_mainStockInfo != null)
            {
                // 호가창
                currentPriceLabel.Text = _mainStockInfo.StockPrice.ToString("n0");
                upDownPriceLabel.Text = _mainStockInfo.UpDownPrice.ToString("n0");
                upDownRateLabel.Text = _mainStockInfo.UpDownRate.ToString("0.##") + "%";
                VolumeLabel.Text = _mainStockInfo.tradingVolume.ToString("n0");

                // 호가 리스트
                orderListView.Items.Clear();
                var enumerator = _mainStockInfo.OrderInfos.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string priceStr = enumerator.Current.Value.price.ToString("n0");
                    string countStr = enumerator.Current.Value.count.ToString("n0");
                    bool isUpCall = !priceStr.Contains('-');
                    if (isUpCall)
                    {
                        priceStr = "▲" + priceStr;
                    }
                    else
                    {
                        priceStr = priceStr.Replace('-', '▼');
                    }
                    ListViewItem listViewItem = new ListViewItem(priceStr);
                    listViewItem.SubItems.Add(countStr);
                    listViewItem.ForeColor = isUpCall ? Color.Red : Color.Blue;
                    orderListView.Items.Add(listViewItem);
                }

                // 차트
                chart.Series["Series1"].Points.Clear();
                var chartInfo = _mainStockInfo.ChartInfos[eChartType.분봉];
                for (int i = 0; i < chartInfo.Count; i++)
                {
                    chart.Series["Series1"].Points.AddXY(chartInfo[i].dateTimeStr, chartInfo[i].maxPrice);
                    chart.Series["Series1"].Points[i].YValues[1] = chartInfo[i].minPrice;
                    chart.Series["Series1"].Points[i].YValues[2] = chartInfo[i].startPrice;
                    chart.Series["Series1"].Points[i].YValues[3] = chartInfo[i].endPrice;
                }
                SetChartScaleView(0, chart.Series["Series1"].Points.Count);
                //chart.ChartAreas[0].AxisX.ScaleView.Zoom(0, chart.Series["Series1"].Points.Count);
            }
        }

        /// <summary>
        /// 관심 or 최근 주식 정보 리스트 UI 업데이트
        /// </summary>
        private void UpdateWatchlistUI()
        {
            if (_stockInfos != null && _stockInfos.Count > 0)
            {
                LatestListView.Items.Clear();
                for (int i = 0; i < _stockInfos.Count; i++)
                {
                    ListViewItem listViewItem = new ListViewItem(_stockInfos[i].tradingSymbol);
                    listViewItem.SubItems.Add(_stockInfos[i].Name);
                    listViewItem.SubItems.Add("");
                    LatestListView.Items.Add(listViewItem);
                }
            }
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
                    SetMainStock(stockInfo);
                    var exist = _stockInfos.Find(info => info.Equals(stockInfo));
                    if (exist != null)
                        _stockInfos.Remove(exist);

                    if (_stockInfos.Count >= WATCH_MAX)
                        _stockInfos.RemoveAt(_stockInfos.Count - 1);

                    _stockInfos.Insert(0, stockInfo);
                }
            }
        }

        private void watchListView_DoubleClick(object sender, EventArgs e)
        {
            string tradingSymbol = LatestListView.FocusedItem.SubItems[0].Text;
            if (!string.IsNullOrEmpty(tradingSymbol))
            {
                StockInfo stockInfo = StockListManager.Instance.GetStockInfo(tradingSymbol);
                if (stockInfo != null)
                    SetMainStock(stockInfo);
            }
        }

        private void SetMainStock(StockInfo stockInfo)
        {
            StockListManager.Instance.RequestStockInfo(stockInfo, (result) =>
            {
                if (result)
                {
                    _mainStockInfo = stockInfo;
                    UpdateMainUI();
                    UpdateWatchlistUI();
                    DefaultSetOrder();
                }
            });
        }

        private void orderPriceUpDown_TextChanged(object sender, EventArgs e)
        {
            Price = Math.Abs(Convert.ToInt64(orderPriceUpDown.Value));
            UpdateOrderUI();
        }

        private void orderPriceUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
                e.Handled = true;
        }

        private void orderCountUpDown_ValueChanged(object sender, EventArgs e)
        {
            Count = Convert.ToInt64(orderCountUpDown.Value);
            UpdateOrderUI();
        }

        private void orderCountUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
                e.Handled = true;
        }

        private void orderListView_DoubleClick(object sender, EventArgs e)
        {
            var price = _mainStockInfo.GetOrderPrice((eOrderType)orderListView.FocusedItem.Index);
            orderPriceUpDown.Value = Convert.ToInt64(price);
        }

        private long _price = 0;
        private long Price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = Math.Abs(value);
                _total = _count * _price;
            }
        }
        private long _count = 1;
        private long Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = Math.Abs(value);
                _total = _count * _price;
            }
        }
        private long _total = 0;

        /// <summary>
        /// 주문 UI 초기화
        /// </summary>
        private void DefaultSetOrder()
        {
            Count = 1;
            OrderInfo orderInfo;
            _mainStockInfo.OrderInfos.TryGetValue(eOrderType.매도최우선호가, out orderInfo);
            if (orderInfo != null)
                Price = Math.Abs(orderInfo.price);
            UpdateOrderUI();
        }

        /// <summary>
        /// 주문 UI 업데이트
        /// </summary>
        private void UpdateOrderUI()
        {
            orderCountUpDown.Text = string.Format("{0:#,##0}", Count);
            orderPriceUpDown.Text = string.Format("{0:#,##0}", Price);
            totalPriceLabel.Text = $"총 {string.Format("{0:#,##0}", _total)}원";
        }
    }
}
