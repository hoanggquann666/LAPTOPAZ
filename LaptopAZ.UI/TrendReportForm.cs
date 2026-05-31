using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LaptopAZ.BLL;
using LaptopAZ.Repository;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;

namespace LaptopAZ.UI
{
    /// <summary>
    /// Trend Report Form - displays revenue and sales charts using LiveCharts.
    /// </summary>
    public class TrendReportForm : Form
    {
        // ── Services ─────────────────────────────────────────────────────
        private readonly DashboardService _dashboardService;
        private readonly DapperReportService _dapperReportService;

        // ── Current data ─────────────────────────────────────────────────
        private Dictionary<string, decimal> _chartData = new Dictionary<string, decimal>();

        // ── UI Controls ──────────────────────────────────────────────────
        private RadioButton _rbDay, _rbMonth, _rbYear;
        private NumericUpDown _numMonth, _numYear;
        private Label _lblMonth, _lblTotal;
        private Button _btnRefresh;
        private TabControl _tabControl;
        private TabPage _tabRevenue, _tabTopProducts, _tabOrderStatus;

        // ── LiveCharts Controls ──────────────────────────────────────────
        private LiveCharts.WinForms.CartesianChart _chartRevenue;
        private LiveCharts.WinForms.CartesianChart _chartTopProducts;
        private LiveCharts.WinForms.PieChart _chartOrderStatus;

        // ─────────────────────────────────────────────────────────────────
        public TrendReportForm(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
            _dashboardService = new DashboardService(unitOfWork);
            _dapperReportService = new DapperReportService();
            
            InitializeComponent();
            LoadRevenueData();
            LoadTopProductsData();
            LoadOrderStatusData();
        }

        private void InitializeComponent()
        {
            this.Text = "Phân Tích Xu Hướng & Báo Cáo Thống Kê";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9F);

            // ── Header Panel ─────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = "📊  BÁO CÁO THỐNG KÊ LIVECHARTS",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // ── Filter Toolbar (for Revenue) ──────────────────────────────
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(16, 0, 16, 0)
            };
            pnlFilter.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, pnlFilter.Height - 1, pnlFilter.Width, pnlFilter.Height - 1);
            };

            // Mode radio buttons
            var lblMode = new Label
            {
                Text = "Xem doanh thu:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(16, 20),
                AutoSize = true
            };

            _rbMonth = new RadioButton
            {
                Text = "Tháng",
                Checked = true,
                Location = new Point(125, 18),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            _rbDay = new RadioButton
            {
                Text = "Ngày",
                Location = new Point(200, 18),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            _rbYear = new RadioButton
            {
                Text = "Năm",
                Location = new Point(265, 18),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            // Month spinner
            _lblMonth = new Label
            {
                Text = "Tháng:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(335, 21),
                AutoSize = true,
                Visible = false
            };

            _numMonth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 12,
                Value = DateTime.Today.Month,
                Location = new Point(385, 16),
                Width = 52,
                Height = 26,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            // Year spinner
            var lblYear = new Label
            {
                Text = "Năm:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(447, 21),
                AutoSize = true
            };

            _numYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2099,
                Value = DateTime.Today.Year,
                Location = new Point(483, 16),
                Width = 72,
                Height = 26,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Refresh button
            _btnRefresh = new Button
            {
                Text = "🔄  Tải Lại",
                Location = new Point(567, 14),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 82, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += (s, e) => {
                LoadRevenueData();
                LoadTopProductsData();
                LoadOrderStatusData();
            };

            // Total label
            _lblTotal = new Label
            {
                Text = "Tổng: đang tính...",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 82, 204),
                Location = new Point(680, 20),
                AutoSize = true
            };

            pnlFilter.Controls.AddRange(new Control[] {
                lblMode, _rbMonth, _rbDay, _rbYear,
                _lblMonth, _numMonth, lblYear, _numYear,
                _btnRefresh, _lblTotal
            });

            // Mode change handler → toggle Month spinner visibility
            _rbMonth.CheckedChanged += OnModeChanged;
            _rbDay.CheckedChanged += OnModeChanged;
            _rbYear.CheckedChanged += OnModeChanged;

            // ── Tab Control for Charts ────────────────────────────────────
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Padding = new Point(12, 6)
            };

            _tabRevenue = new TabPage { Text = "📈 Doanh Thu Từng Kỳ", BackColor = Color.White };
            _tabTopProducts = new TabPage { Text = "🔥 Top Sản Phẩm Bán Chạy", BackColor = Color.White };
            _tabOrderStatus = new TabPage { Text = "📦 Trạng Thái Đơn Hàng", BackColor = Color.White };

            _tabControl.TabPages.AddRange(new TabPage[] { _tabRevenue, _tabTopProducts, _tabOrderStatus });

            // 1. Revenue Chart
            _chartRevenue = new LiveCharts.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            _tabRevenue.Controls.Add(_chartRevenue);

            // 2. Top Products Chart
            _chartTopProducts = new LiveCharts.WinForms.CartesianChart
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            _tabTopProducts.Controls.Add(_chartTopProducts);

            // 3. Order Status Chart
            _chartOrderStatus = new LiveCharts.WinForms.PieChart
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            _tabOrderStatus.Controls.Add(_chartOrderStatus);

            // ── Footer Panel ──────────────────────────────────────────────
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            var lblNote = new Label
            {
                Text = "Thống kê tự động cập nhật thời gian thực dựa trên cơ sở dữ liệu LaptopAZ.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(16, 20),
                AutoSize = true
            };

            var btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(100, 32),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Location = new Point(this.Width - 128, 12);
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(lblNote);
            pnlFooter.Controls.Add(btnClose);

            // ── Assemble ─────────────────────────────────────────────────
            this.Controls.Add(_tabControl);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            _lblMonth.Visible = _rbDay.Checked;
            _numMonth.Visible = _rbDay.Checked;
            _numYear.Enabled = !_rbYear.Checked;
            LoadRevenueData();
        }

        // ─── Data Loading ─────────────────────────────────────────────────
        private void LoadRevenueData()
        {
            try
            {
                int year = (int)_numYear.Value;
                int month = (int)_numMonth.Value;

                if (_rbDay.Checked)
                    _chartData = _dashboardService.GetRevenueByDay(year, month);
                else if (_rbYear.Checked)
                    _chartData = _dashboardService.GetRevenueByYear();
                else
                    _chartData = _dashboardService.GetRevenueByMonth(year);

                decimal total = 0;
                var values = new ChartValues<decimal>();
                var labels = new List<string>();

                foreach (var kvp in _chartData)
                {
                    labels.Add(kvp.Key);
                    values.Add(kvp.Value);
                    total += kvp.Value;
                }

                _lblTotal.Text = $"Tổng: {total:N0} đ";

                _chartRevenue.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Doanh thu",
                        Values = values,
                        Fill = System.Windows.Media.Brushes.DodgerBlue,
                        DataLabels = values.Count <= 12
                    }
                };

                _chartRevenue.AxisX.Clear();
                _chartRevenue.AxisX.Add(new Axis
                {
                    Title = _rbDay.Checked ? "Ngày" : (_rbYear.Checked ? "Năm" : "Tháng"),
                    Labels = labels.ToArray(),
                    Separator = new Separator { Step = 1 }
                });

                _chartRevenue.AxisY.Clear();
                _chartRevenue.AxisY.Add(new Axis
                {
                    Title = "Doanh thu (đ)",
                    LabelFormatter = val => val.ToString("N0")
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu doanh thu:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTopProductsData()
        {
            try
            {
                var topProducts = _dapperReportService.GetTopSellingProducts(10);
                topProducts.Reverse(); // Đưa sản phẩm bán chạy nhất lên trên cùng

                var names = new List<string>();
                var quantities = new ChartValues<int>();

                foreach (var p in topProducts)
                {
                    names.Add(p.ProductName);
                    quantities.Add(p.QuantitySold);
                }

                _chartTopProducts.Series = new SeriesCollection
                {
                    new RowSeries
                    {
                        Title = "Số lượng đã bán",
                        Values = quantities,
                        Fill = System.Windows.Media.Brushes.MediumSeaGreen,
                        DataLabels = true
                    }
                };

                _chartTopProducts.AxisY.Clear();
                _chartTopProducts.AxisY.Add(new Axis
                {
                    Title = "Sản phẩm",
                    Labels = names.ToArray()
                });

                _chartTopProducts.AxisX.Clear();
                _chartTopProducts.AxisX.Add(new Axis
                {
                    Title = "Số lượng máy",
                    LabelFormatter = val => val.ToString("N0")
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu sản phẩm bán chạy:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderStatusData()
        {
            try
            {
                var statusCounts = _dapperReportService.GetOrderCountByStatus();
                var series = new SeriesCollection();

                foreach (var sc in statusCounts)
                {
                    string displayName = sc.Status;
                    switch (sc.Status)
                    {
                        case "Pending": displayName = "Chờ xử lý"; break;
                        case "Confirmed": displayName = "Xác nhận"; break;
                        case "Shipped": displayName = "Đang giao"; break;
                        case "Delivered": displayName = "Đã giao"; break;
                        case "Completed": displayName = "Hoàn thành"; break;
                        case "Cancelled": displayName = "Hủy"; break;
                        case "Paid": displayName = "Đã thanh toán"; break;
                    }

                    series.Add(new PieSeries
                    {
                        Title = displayName,
                        Values = new ChartValues<int> { sc.Count },
                        DataLabels = true,
                        LabelPoint = chartPoint => $"{chartPoint.Y} đơn ({chartPoint.Participation:P0})"
                    });
                }

                _chartOrderStatus.Series = series;
                _chartOrderStatus.LegendLocation = LegendLocation.Right;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu trạng thái đơn hàng:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
