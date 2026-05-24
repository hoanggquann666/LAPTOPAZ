using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LaptopAZ.BLL;
using LaptopAZ.Repository;

namespace LaptopAZ.UI
{
    /// <summary>
    /// Trend Report Form - displays revenue charts filtered by Day / Month / Year.
    /// Requires IUnitOfWork to initialize DashboardService internally.
    /// </summary>
    public class TrendReportForm : Form
    {
        // ── Services ─────────────────────────────────────────────────────
        private readonly DashboardService _dashboardService;

        // ── Current data ─────────────────────────────────────────────────
        private Dictionary<string, decimal> _chartData = new Dictionary<string, decimal>();

        // ── UI Controls ──────────────────────────────────────────────────
        private RadioButton _rbDay, _rbMonth, _rbYear;
        private NumericUpDown _numMonth, _numYear;
        private Label _lblMonth, _lblTotal;
        private Panel _pnlChart;
        private Button _btnRefresh;

        // ─────────────────────────────────────────────────────────────────
        public TrendReportForm(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));
            _dashboardService = new DashboardService(unitOfWork);
            InitializeComponent();
            LoadChart(); // Load "Theo Tháng" as default
        }

        private void InitializeComponent()
        {
            this.Text = "Phân Tích Xu Hướng Doanh Thu";
            this.Size = new Size(940, 620);
            this.MinimumSize = new Size(720, 500);
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
                Text = "📈  BÁO CÁO XU HƯỚNG DOANH THU",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // ── Filter Toolbar ────────────────────────────────────────────
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
                Text = "Xem theo:",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(16, 20),
                AutoSize = true
            };

            _rbMonth = new RadioButton
            {
                Text = "Tháng",
                Checked = true,
                Location = new Point(90, 18),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            _rbDay = new RadioButton
            {
                Text = "Ngày",
                Location = new Point(165, 18),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            _rbYear = new RadioButton
            {
                Text = "Năm",
                Location = new Point(228, 18),
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
                Location = new Point(302, 21),
                AutoSize = true
            };

            _numMonth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 12,
                Value = DateTime.Today.Month,
                Location = new Point(352, 16),
                Width = 52,
                Height = 26,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Year spinner
            var lblYear = new Label
            {
                Text = "Năm:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(414, 21),
                AutoSize = true
            };

            _numYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2099,
                Value = DateTime.Today.Year,
                Location = new Point(450, 16),
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
                Location = new Point(534, 14),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 82, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += (s, e) => LoadChart();

            // Total label
            _lblTotal = new Label
            {
                Text = "Tổng: đang tính...",
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 82, 204),
                Location = new Point(650, 20),
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

            // ── Chart Panel ───────────────────────────────────────────────
            _pnlChart = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(24)
            };
            _pnlChart.Paint += PnlChart_Paint;

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
                Text = "Chỉ tính các đơn hàng có trạng thái \"Đã thanh toán\" (Paid).",
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
            this.Controls.Add(_pnlChart);
            this.Controls.Add(pnlFilter);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        // ─── Mode Changed Handler ──────────────────────────────────────────
        private void OnModeChanged(object sender, EventArgs e)
        {
            // Month spinner visible only in "by-day" mode
            _lblMonth.Visible = _rbDay.Checked;
            _numMonth.Visible = _rbDay.Checked;
            // Year hidden in "by-year" mode
            _numYear.Enabled = !_rbYear.Checked;
            LoadChart();
        }

        // ─── Data Loading ─────────────────────────────────────────────────
        private void LoadChart()
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

                // Update total
                decimal total = 0;
                foreach (var v in _chartData.Values) total += v;
                _lblTotal.Text = $"Tổng: {total:N0} đ";

                _pnlChart.Invalidate(); // Trigger repaint
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu biểu đồ:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Chart Paint ─────────────────────────────────────────────────
        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = ((Panel)sender).ClientRectangle;

            int paddingLeft = 95;
            int paddingRight = 30;
            int paddingTop = 52;
            int paddingBottom = 50;

            int chartWidth = rect.Width - paddingLeft - paddingRight;
            int chartHeight = rect.Height - paddingTop - paddingBottom;

            if (chartWidth < 10 || chartHeight < 10) return;

            // ── Chart background ───────────────────────────────────────
            using (var bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, rect);

            // ── Determine max value ────────────────────────────────────
            decimal maxVal = 1;
            foreach (var v in _chartData.Values)
                if (v > maxVal) maxVal = v;

            // Round max up for clean grid
            double log = Math.Log10((double)maxVal);
            double mag = Math.Pow(10, Math.Floor(log));
            decimal roundedMax = (decimal)(Math.Ceiling((double)maxVal / mag) * mag);
            if (roundedMax <= 0) roundedMax = 10_000_000;

            // ── Draw Y-axis grid lines & labels ────────────────────────
            int gridLines = 5;
            using (var gridPen = new Pen(Color.FromArgb(241, 245, 249), 1))
            using (var gridPenSolid = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var axisFont = new Font("Segoe UI", 7.5F))
            using (var axisBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            using (var fmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i <= gridLines; i++)
                {
                    decimal gridVal = roundedMax / gridLines * i;
                    float y = paddingTop + chartHeight - (float)gridVal / (float)roundedMax * chartHeight;

                    // Dashed grid line
                    using (var dash = new Pen(i == 0 ? Color.FromArgb(203, 213, 225) : Color.FromArgb(241, 245, 249), 1))
                    {
                        if (i > 0) dash.DashStyle = DashStyle.Dash;
                        g.DrawLine(dash, paddingLeft, y, paddingLeft + chartWidth, y);
                    }

                    // Y-axis label
                    string label = FormatCurrency(gridVal);
                    g.DrawString(label, axisFont, axisBrush,
                        new RectangleF(0, y - 10, paddingLeft - 6, 20), fmt);
                }
            }

            // ── Draw bars ─────────────────────────────────────────────
            int count = _chartData.Count;
            if (count == 0)
            {
                DrawNoData(g, rect);
                return;
            }

            // Limit bar width for very many data points (daily view has 31 bars)
            float spacing = chartWidth / (float)count;
            float barWidth = Math.Max(6, spacing * 0.55f);

            using (var labelFont = new Font("Segoe UI", count > 20 ? 6.5F : 8F, FontStyle.Bold))
            using (var valFont = new Font("Segoe UI", count > 20 ? 6F : 7.5F))
            using (var textBrush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            using (var highlightBrush = new SolidBrush(Color.FromArgb(0, 82, 204)))
            {
                int index = 0;
                decimal maxInData = 0;
                foreach (var v in _chartData.Values) if (v > maxInData) maxInData = v;

                foreach (var kvp in _chartData)
                {
                    string label = kvp.Key;
                    decimal revenue = kvp.Value;

                    float centerX = paddingLeft + index * spacing + spacing / 2;
                    float barX = centerX - barWidth / 2;
                    float barH = revenue > 0
                        ? (float)revenue / (float)roundedMax * chartHeight
                        : 0;
                    float barY = paddingTop + chartHeight - barH;

                    bool isHighest = revenue > 0 && revenue == maxInData;

                    if (revenue > 0)
                    {
                        var barRect = new RectangleF(barX, barY, barWidth, barH);

                        // Colors: highlight max bar in a brighter gradient
                        Color colorTop = isHighest
                            ? Color.FromArgb(99, 102, 241)   // Indigo (highlight)
                            : Color.FromArgb(0, 82, 204);    // Blue
                        Color colorBottom = isHighest
                            ? Color.FromArgb(139, 92, 246)   // Purple
                            : Color.FromArgb(56, 131, 255);  // Light blue

                        using (var path = new GraphicsPath())
                        {
                            float radius = Math.Min(5, barH / 2);
                            if (barH > radius * 2)
                            {
                                path.AddArc(barRect.X, barRect.Y, radius * 2, radius * 2, 180, 90);
                                path.AddArc(barRect.Right - radius * 2, barRect.Y, radius * 2, radius * 2, 270, 90);
                                path.AddLine(barRect.Right, barRect.Bottom, barRect.X, barRect.Bottom);
                                path.CloseFigure();
                            }
                            else
                            {
                                path.AddRectangle(barRect);
                            }

                            using (var blendBrush = new LinearGradientBrush(
                                barRect,
                                colorTop, colorBottom,
                                LinearGradientMode.Vertical))
                            {
                                g.FillPath(blendBrush, path);
                            }
                        }

                        // Value label on top (only for wider bars)
                        if (barWidth > 18 && barH > 14)
                        {
                            string valText = FormatCurrency(revenue);
                            var szVal = g.MeasureString(valText, valFont);
                            using (var vb = new SolidBrush(isHighest ? Color.FromArgb(99, 102, 241) : Color.FromArgb(0, 82, 204)))
                                g.DrawString(valText, valFont, vb, centerX - szVal.Width / 2, barY - szVal.Height - 2);
                        }
                    }
                    else
                    {
                        // Zero bar: draw a thin gray line
                        using (var zeroPen = new Pen(Color.FromArgb(226, 232, 240), 1))
                            g.DrawLine(zeroPen, barX, paddingTop + chartHeight, barX + barWidth, paddingTop + chartHeight);
                    }

                    // X-axis label
                    if (count <= 31)
                    {
                        var szLbl = g.MeasureString(label, labelFont);
                        float lx = centerX - szLbl.Width / 2;
                        float ly = paddingTop + chartHeight + 8;

                        // Rotate labels for day view to avoid overlap
                        if (count > 15)
                        {
                            var state = g.Save();
                            g.TranslateTransform(centerX, ly + szLbl.Width / 2);
                            g.RotateTransform(-55);
                            g.DrawString(label, labelFont, textBrush, -szLbl.Width / 2, -szLbl.Height / 2);
                            g.Restore(state);
                        }
                        else
                        {
                            g.DrawString(label, labelFont, textBrush, lx, ly);
                        }
                    }

                    index++;
                }
            }

            // ── Axes ─────────────────────────────────────────────────
            using (var axisPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
            {
                g.DrawLine(axisPen, paddingLeft, paddingTop, paddingLeft, paddingTop + chartHeight);
                g.DrawLine(axisPen, paddingLeft, paddingTop + chartHeight,
                    paddingLeft + chartWidth, paddingTop + chartHeight);
            }

            // ── Chart title ───────────────────────────────────────────
            string titleText = BuildChartTitle();
            using (var titleFont = new Font("Segoe UI Semibold", 10F, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                var sz = g.MeasureString(titleText, titleFont);
                g.DrawString(titleText, titleFont, titleBrush,
                    paddingLeft + (chartWidth - sz.Width) / 2, 12);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────
        private static string FormatCurrency(decimal value)
        {
            if (value >= 1_000_000_000)
                return $"{value / 1_000_000_000:0.##}Tỷ";
            if (value >= 1_000_000)
                return $"{value / 1_000_000:0.##}Tr";
            if (value >= 1_000)
                return $"{value / 1_000:0.##}K";
            return $"{value:N0}";
        }

        private string BuildChartTitle()
        {
            if (_rbDay.Checked)
                return $"Doanh thu theo ngày — Tháng {_numMonth.Value}/{_numYear.Value}";
            if (_rbYear.Checked)
                return "Doanh thu theo năm (toàn bộ lịch sử)";
            return $"Doanh thu theo tháng — Năm {_numYear.Value}";
        }

        private static void DrawNoData(Graphics g, Rectangle rect)
        {
            using (var font = new Font("Segoe UI", 13F, FontStyle.Italic))
            using (var brush = new SolidBrush(Color.FromArgb(203, 213, 225)))
            {
                string msg = "Không có dữ liệu doanh thu cho kỳ này.";
                var sz = g.MeasureString(msg, font);
                g.DrawString(msg, font, brush,
                    (rect.Width - sz.Width) / 2,
                    (rect.Height - sz.Height) / 2);
            }
        }
    }
}
