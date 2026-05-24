using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LaptopAZ.BLL;
using LaptopAZ.DTO;
using LaptopAZ.Helpers;
using LaptopAZ.Models;
using LaptopAZ.Repository;

namespace LaptopAZ.UI
{
    public partial class MainForm : Form
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthService _authService;
        private readonly ProductService _productService;
        private readonly WarehouseService _warehouseService;
        private readonly SalesService _salesService;
        private readonly ReturnService _returnService;
        private readonly DashboardService _dashboardService;

        private Timer _clockTimer;
        private Button _currentActiveTabButton;

        public MainForm(IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _unitOfWork = unitOfWork;
            
            // Initialize services
            _authService = new AuthService(_unitOfWork);
            _productService = new ProductService(_unitOfWork);
            _warehouseService = new WarehouseService(_unitOfWork);
            _salesService = new SalesService(_unitOfWork);
            _returnService = new ReturnService(_unitOfWork);
            _dashboardService = new DashboardService(_unitOfWork);

            // Hook ControlAdded to automatically modernize views
            panelMainContainer.ControlAdded += (s, ev) => {
                ModernizeControls(ev.Control);
            };
        }

        private int scale(int value)
        {
            return (int)Math.Round(value * (this.DeviceDpi / 96.0));
        }

        private float scale(float value)
        {
            return (float)(value * (this.DeviceDpi / 96.0));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Setup real-time clock
            _clockTimer = new Timer();
            _clockTimer.Interval = 1000;
            _clockTimer.Tick += (s, ev) => {
                lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            _clockTimer.Start();

            // Set employee info
            lblEmployeeInfo.Text = $"Nhân viên: {SessionHelper.CurrentFullName} ({TranslateRole(SessionHelper.CurrentRole)})";

            // Áp dụng bộ giao diện tối cao cấp
            StyleMainForm();

            // Role-based authorization
            ConfigurePermissions();

            // Load default view (Dashboard or first available view)
            if (btnTabDashboard.Visible)
            {
                SetActiveTab(btnTabDashboard, "DASHBOARD");
                ShowDashboardView();
            }
            else if (btnTabProducts.Visible)
            {
                SetActiveTab(btnTabProducts, "SẢN PHẨM");
                ShowProductsView();
            }
            else
            {
                SetActiveTab(btnTabSales, "BÁN HÀNG");
                ShowSalesView();
            }
        }

        private void StyleMainForm()
        {
            // 1. Tông màu Premium Enterprise Light-Slate & Soft Blue
            this.BackColor = Color.FromArgb(248, 250, 252); // Canvas sáng slate-50
            panelSidebar.BackColor = Color.FromArgb(241, 245, 249); // Sidebar sáng slate-100
            panelHeader.BackColor = Color.White;
            panelMainContainer.BackColor = Color.FromArgb(248, 250, 252);
            
            panelActiveIndicator.BackColor = Color.FromArgb(0, 82, 204); // Chỉ báo Brand Blue
            panelActiveIndicator.Width = 4;
            
            lblActiveTabTitle.ForeColor = Color.FromArgb(15, 23, 42); // slate-900
            lblActiveTabTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblClock.ForeColor = Color.FromArgb(100, 116, 139); // slate-500
            
            // Repurpose/hide original employee info label on header since we now have it in sidebar bottom card
            lblEmployeeInfo.Visible = false;

            // 2. Định dạng Menu Sidebar động với hover mượt mà
            foreach (Control ctrl in panelSidebar.Controls)
            {
                if (ctrl is Button btn && btn != btnLogout)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(71, 85, 105); // slate-600
                    btn.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240); // slate-200
                    btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(203, 213, 225); // slate-300
                }
            }

            btnTabCategories.Text = "  📁   Hãng | Danh Mục";
            btnTabPartners.Text = "  👥   Khách Hàng | Đối Tác";

            // Move brand title & format
            lblSidebarBrand.Height = 85;
            lblSidebarBrand.Text = "Azure Management";
            lblSidebarBrand.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSidebarBrand.ForeColor = Color.FromArgb(0, 82, 204);
            lblSidebarBrand.TextAlign = ContentAlignment.TopLeft;
            lblSidebarBrand.Padding = new Padding(18, 18, 0, 0);

            // Add brand subtitle
            Label lblSidebarSubtitle = new Label();
            lblSidebarSubtitle.Text = "System Administrator";
            lblSidebarSubtitle.Font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold);
            lblSidebarSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // slate-400
            lblSidebarSubtitle.Location = new Point(22, 48);
            lblSidebarSubtitle.AutoSize = true;
            lblSidebarSubtitle.BackColor = Color.Transparent;
            panelSidebar.Controls.Add(lblSidebarSubtitle);
            lblSidebarSubtitle.BringToFront();



            // Create FlowLayoutPanel for top-right profile and navigation elements
            FlowLayoutPanel pnlTopRight = new FlowLayoutPanel();
            pnlTopRight.Name = "pnlTopRight";
            pnlTopRight.FlowDirection = FlowDirection.RightToLeft;
            pnlTopRight.Dock = DockStyle.Right;
            pnlTopRight.Height = 80;
            pnlTopRight.Width = 580; // wide enough to hold all items comfortably
            pnlTopRight.BackColor = Color.Transparent;
            pnlTopRight.WrapContents = false;
            panelHeader.Controls.Add(pnlTopRight);

            // Re-style Logout button
            btnLogout.Anchor = AnchorStyles.None;
            btnLogout.Size = new Size(110, 34);
            btnLogout.Text = "🚪 Đăng xuất";
            btnLogout.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnLogout.BackColor = Color.FromArgb(254, 226, 226); // red-100
            btnLogout.ForeColor = Color.FromArgb(220, 38, 38); // red-600
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 202, 202);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(252, 165, 165);
            btnLogout.Margin = new Padding(10, 23, 10, 23);

            Panel pnlHeaderAvatar = new Panel();
            pnlHeaderAvatar.Size = new Size(34, 34);
            pnlHeaderAvatar.BackColor = Color.Transparent;
            pnlHeaderAvatar.Anchor = AnchorStyles.None;
            pnlHeaderAvatar.Margin = new Padding(10, 23, 10, 23);
            pnlHeaderAvatar.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(219, 234, 254)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, pnlHeaderAvatar.Width - 1, pnlHeaderAvatar.Height - 1);
                }
                using (var pen = new Pen(Color.FromArgb(191, 219, 254), 1))
                {
                    e.Graphics.DrawEllipse(pen, 0, 0, pnlHeaderAvatar.Width - 1, pnlHeaderAvatar.Height - 1);
                }
                string letter = string.IsNullOrEmpty(SessionHelper.CurrentFullName) ? "U" : SessionHelper.CurrentFullName.Substring(0, 1).ToUpper();
                using (var font = new Font("Segoe UI", 10.5F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(0, 82, 204)))
                {
                    var size = e.Graphics.MeasureString(letter, font);
                    e.Graphics.DrawString(letter, font, brush, (pnlHeaderAvatar.Width - size.Width) / 2, (pnlHeaderAvatar.Height - size.Height) / 2);
                }
            };

            // Header User card
            Label lblHeaderUser = new Label();
            string userRoleText = TranslateRole(SessionHelper.CurrentRole);
            lblHeaderUser.Text = $"{SessionHelper.CurrentFullName}\n{userRoleText.ToUpper()}";
            lblHeaderUser.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            lblHeaderUser.ForeColor = Color.FromArgb(30, 41, 59);
            lblHeaderUser.TextAlign = ContentAlignment.MiddleRight;
            lblHeaderUser.AutoSize = true;
            lblHeaderUser.Height = 36;
            lblHeaderUser.Anchor = AnchorStyles.None;
            lblHeaderUser.Margin = new Padding(10, 22, 10, 22);

            // Re-style and re-parent Clock
            lblClock.Anchor = AnchorStyles.None;
            lblClock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblClock.ForeColor = Color.FromArgb(100, 116, 139);
            lblClock.AutoSize = true;
            lblClock.TextAlign = ContentAlignment.MiddleRight;
            lblClock.Margin = new Padding(15, 30, 15, 30);

            // Help Icon
            Label lblHelp = new Label();
            lblHelp.Text = "❓";
            lblHelp.Font = new Font("Segoe UI", 12F);
            lblHelp.ForeColor = Color.FromArgb(100, 116, 139);
            lblHelp.Size = new Size(24, 24);
            lblHelp.Cursor = Cursors.Hand;
            lblHelp.Anchor = AnchorStyles.None;
            lblHelp.Margin = new Padding(10, 28, 10, 28);
            lblHelp.Click += (s, ev) => {
                // Tìm file HUONGDANSUDUNG.md theo nhiều đường dẫn tương đối từ thư mục chạy
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    // Thêm đường dẫn ngược về thư mục gốc project (4 cấp lên từ bin\Debug\net472\)
                    string[] candidates = new[]
                    {
                        System.IO.Path.Combine(baseDir, "HUONGDANSUDUNG.md"),
                        System.IO.Path.Combine(baseDir, "..", "HUONGDANSUDUNG.md"),
                        System.IO.Path.Combine(baseDir, "..", "..", "HUONGDANSUDUNG.md"),
                        System.IO.Path.Combine(baseDir, "..", "..", "..", "HUONGDANSUDUNG.md"),
                        System.IO.Path.Combine(baseDir, "..", "..", "..", "..", "HUONGDANSUDUNG.md"),
                        System.IO.Path.Combine(baseDir, "..", "..", "..", "..", "..", "HUONGDANSUDUNG.md")
                    };
                    string foundPath = null;
                    foreach (var candidate in candidates)
                    {
                        string full = System.IO.Path.GetFullPath(candidate);
                        if (System.IO.File.Exists(full)) { foundPath = full; break; }
                    }
                    if (foundPath != null)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = foundPath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Không tìm thấy tệp HUONGDANSUDUNG.md.\n\nĐã tìm ở:\n{string.Join("\n", candidates)}",
                            "Không tìm thấy tài liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở tài liệu hướng dẫn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Bell Icon
            Label lblBell = new Label();
            lblBell.Text = "🔔";
            lblBell.Font = new Font("Segoe UI", 12F);
            lblBell.ForeColor = Color.FromArgb(100, 116, 139);
            lblBell.Size = new Size(24, 24);
            lblBell.Cursor = Cursors.Hand;
            lblBell.Anchor = AnchorStyles.None;
            lblBell.Margin = new Padding(10, 28, 10, 28);
            lblBell.Click += (s, ev) => {
                try
                {
                    string logFile = "login_history.txt";
                    var history = new List<string>();
                    if (System.IO.File.Exists(logFile))
                    {
                        history = System.IO.File.ReadAllLines(logFile).Reverse().Take(20).ToList();
                    }

                    var popup = new Form
                    {
                        Text = "Lịch sử đăng nhập hệ thống",
                        Size = new Size(500, 350),
                        StartPosition = FormStartPosition.CenterParent,
                        BackColor = Color.FromArgb(248, 250, 252),
                        ForeColor = Color.FromArgb(30, 41, 59)
                    };

                    var lstBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 9.5F),
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(30, 41, 59)
                    };
                    if (history.Any())
                    {
                        foreach (var item in history)
                        {
                            lstBox.Items.Add(item);
                        }
                    }
                    else
                    {
                        lstBox.Items.Add("Chưa ghi nhận lịch sử đăng nhập nào.");
                    }
                    popup.Controls.Add(lstBox);
                    popup.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể đọc lịch sử đăng nhập: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Add controls to top-right layout (from right to left)
            pnlTopRight.Controls.Add(btnLogout);
            pnlTopRight.Controls.Add(pnlHeaderAvatar);
            pnlTopRight.Controls.Add(lblHeaderUser);
            pnlTopRight.Controls.Add(lblClock);
            pnlTopRight.Controls.Add(lblHelp);
            pnlTopRight.Controls.Add(lblBell);

            // Listen to tab title changes to update header layout
            lblActiveTabTitle.TextChanged += (s, e) => {
                if (lblActiveTabTitle.Parent != null)
                {
                    lblActiveTabTitle.Parent.PerformLayout(); // Force header layout calculation
                }
            };

            // Add dynamic "+ New Inventory" button in Sidebar
            Button btnNewInventory = CreatePremiumButton("+ New Inventory", Color.FromArgb(0, 82, 204), Color.White);
            btnNewInventory.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnNewInventory.Location = new Point(15, 520);
            btnNewInventory.Size = new Size(190, 42);
            btnNewInventory.Click += (s, ev) => {
                if (_currentActiveTabButton != btnTabProducts)
                {
                    SetActiveTab(btnTabProducts, "Danh sách Sản Phẩm");
                    ShowProductsView();
                }
                ResetProductEditor();
                _selectedProductIdForEdit = 0;
                _txtProductCode.ReadOnly = false;
                if (_pnlProductEditor != null)
                {
                    var lblEdTitle = _pnlProductEditor.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("THÔNG TIN") || l.Text.Contains("LAPTOP"));
                    if (lblEdTitle != null) lblEdTitle.Text = "📝 THÊM MỚI LAPTOP";
                }
            };
            panelSidebar.Controls.Add(btnNewInventory);

            // Add bottom employee card in Sidebar
            Panel pnlEmployeeCard = new Panel();
            pnlEmployeeCard.Size = new Size(220, 60);
            pnlEmployeeCard.Location = new Point(0, 650);
            pnlEmployeeCard.BackColor = Color.Transparent;
            panelSidebar.Controls.Add(pnlEmployeeCard);

            Panel pnlAvatarCircle = new Panel();
            pnlAvatarCircle.Size = new Size(38, 38);
            pnlAvatarCircle.Location = new Point(15, 11);
            pnlAvatarCircle.BackColor = Color.Transparent;
            pnlAvatarCircle.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(219, 234, 254)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, pnlAvatarCircle.Width - 1, pnlAvatarCircle.Height - 1);
                }
                using (var pen = new Pen(Color.FromArgb(191, 219, 254), 1))
                {
                    e.Graphics.DrawEllipse(pen, 0, 0, pnlAvatarCircle.Width - 1, pnlAvatarCircle.Height - 1);
                }
                string letter = string.IsNullOrEmpty(SessionHelper.CurrentFullName) ? "U" : SessionHelper.CurrentFullName.Substring(0, 1).ToUpper();
                using (var font = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(0, 82, 204)))
                {
                    var size = e.Graphics.MeasureString(letter, font);
                    e.Graphics.DrawString(letter, font, brush, (pnlAvatarCircle.Width - size.Width) / 2, (pnlAvatarCircle.Height - size.Height) / 2);
                }
            };
            pnlEmployeeCard.Controls.Add(pnlAvatarCircle);

            Label lblEmpName = new Label();
            lblEmpName.Text = SessionHelper.CurrentFullName;
            lblEmpName.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblEmpName.ForeColor = Color.FromArgb(30, 41, 59);
            lblEmpName.Location = new Point(60, 11);
            lblEmpName.AutoSize = true;
            pnlEmployeeCard.Controls.Add(lblEmpName);

            Label lblEmpEmail = new Label();
            string displayEmail = !string.IsNullOrEmpty(SessionHelper.CurrentEmail) ? SessionHelper.CurrentEmail : "no-email@laptopaz.vn";
            lblEmpEmail.Text = displayEmail;
            lblEmpEmail.Font = new Font("Segoe UI", 8F);
            lblEmpEmail.ForeColor = Color.FromArgb(100, 116, 139);
            lblEmpEmail.Location = new Point(60, 30);
            lblEmpEmail.AutoSize = true;
            pnlEmployeeCard.Controls.Add(lblEmpEmail);
        }

        private void ModernizeControls(Control parent)
        {
            if (parent == null) return;

            // Apply color modifications based on control type and existing colors
            if (parent is Panel pnl)
            {
                // If it is our main sidebar or header, don't change their backgrounds
                if (pnl.Name != "panelSidebar" && pnl.Name != "panelHeader" && pnl.Name != "panelMainContainer")
                {
                    if (pnl.BackColor == Color.FromArgb(30, 41, 59) || 
                        pnl.BackColor == Color.FromArgb(15, 23, 42) || 
                        pnl.BackColor == Color.FromArgb(26, 36, 53) || 
                        pnl.BackColor == Color.FromArgb(17, 24, 39))
                    {
                        if (pnl.Height <= 100)
                        {
                            pnl.BackColor = Color.FromArgb(241, 245, 249); // slate-100 for toolbars/headers
                        }
                        else
                        {
                            pnl.BackColor = Color.White;
                        }
                    }
                }
            }
            else if (parent is TextBox txt)
            {
                if (txt.BackColor == Color.FromArgb(15, 23, 42) || 
                    txt.BackColor == Color.FromArgb(30, 41, 59) || 
                    txt.BackColor == Color.FromArgb(26, 36, 53) ||
                    txt.BackColor == Color.FromArgb(17, 24, 39))
                {
                    txt.BackColor = Color.White;
                    txt.ForeColor = Color.FromArgb(30, 41, 59);
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    
                    // Add smooth interactive color feedback on focus
                    txt.Enter += (s, e) => {
                        txt.BackColor = Color.FromArgb(248, 250, 252); // slate-50
                    };
                    txt.Leave += (s, e) => {
                        txt.BackColor = Color.White;
                    };
                }
            }
            else if (parent is ComboBox cb)
            {
                if (cb.BackColor == Color.FromArgb(15, 23, 42) || 
                    cb.BackColor == Color.FromArgb(30, 41, 59) || 
                    cb.BackColor == Color.FromArgb(26, 36, 53) ||
                    cb.BackColor == Color.FromArgb(17, 24, 39))
                {
                    cb.BackColor = Color.White;
                    cb.ForeColor = Color.FromArgb(30, 41, 59);
                    cb.FlatStyle = FlatStyle.Flat;
                }
            }
            else if (parent is CheckedListBox clb)
            {
                if (clb.BackColor == Color.FromArgb(15, 23, 42) || 
                    clb.BackColor == Color.FromArgb(30, 41, 59) || 
                    clb.BackColor == Color.FromArgb(26, 36, 53) ||
                    clb.BackColor == Color.FromArgb(17, 24, 39))
                {
                    clb.BackColor = Color.White;
                    clb.ForeColor = Color.FromArgb(30, 41, 59);
                    clb.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            else if (parent is Label lbl)
            {
                if (lbl.Parent == null || (lbl.Parent.Name != "panelSidebar" && lbl.Parent.Name != "panelHeader"))
                {
                    if (lbl.ForeColor == Color.White || 
                        lbl.ForeColor == Color.FromArgb(243, 244, 246) || 
                        lbl.ForeColor == Color.FromArgb(203, 213, 225) ||
                        lbl.ForeColor == Color.FromArgb(156, 163, 175) ||
                        lbl.ForeColor == Color.FromArgb(100, 116, 139) ||
                        lbl.ForeColor == Color.FromArgb(148, 163, 184) ||
                        lbl.ForeColor == Color.FromArgb(71, 85, 105))
                    {
                        lbl.ForeColor = Color.FromArgb(15, 23, 42); // slate-900 for high contrast
                    }
                }
            }
            else if (parent is CheckBox chk)
            {
                if (chk.ForeColor == Color.White || 
                    chk.ForeColor == Color.FromArgb(243, 244, 246) || 
                    chk.ForeColor == Color.FromArgb(203, 213, 225))
                {
                    chk.ForeColor = Color.FromArgb(71, 85, 105);
                }
            }
            else if (parent is Button btn)
            {
                if (btn.Parent == null || (btn.Parent.Name != "panelSidebar" && btn.Parent.Name != "panelHeader"))
                {
                    if (btn.BackColor == Color.FromArgb(30, 41, 59) || 
                        btn.BackColor == Color.FromArgb(15, 23, 42) ||
                        btn.BackColor == Color.FromArgb(26, 36, 53) ||
                        btn.BackColor == Color.FromArgb(17, 24, 39))
                    {
                        btn.BackColor = Color.FromArgb(0, 82, 204);
                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                    }
                }
            }
            else if (parent is DataGridView grid)
            {
                // Check if CellPainting has not been configured/hooked for status pills.
                // Note: lowStock / bestSeller grids have their own specific cell paintings.
                // For other grids, let's attach status pill cell painting.
                if (grid.Name != "gridLow" && grid.Name != "gridBest")
                {
                    grid.CellPainting += (s, e) => {
                        if (e.RowIndex < 0) return;

                        var col = grid.Columns[e.ColumnIndex];
                        if (col.HeaderText == "Trạng Thái" || col.HeaderText == "Status" || col.Name.Contains("Status") || col.Name.Contains("IsActive"))
                        {
                            if (e.Value != null)
                            {
                                e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
                                string statusText = e.Value.ToString();
                                
                                Color bgPill = Color.FromArgb(241, 245, 249); // slate-100
                                Color textPill = Color.FromArgb(71, 85, 105); // slate-600
                                
                                if (statusText == "Còn kinh doanh" || statusText == "Đã thanh toán" || statusText == "Hoạt động" || statusText == "Đã xử lý")
                                {
                                    bgPill = Color.FromArgb(204, 251, 241); // teal-50
                                    textPill = Color.FromArgb(13, 148, 136); // teal-600
                                }
                                else if (statusText == "Ngừng kinh doanh" || statusText == "Đã hủy" || statusText == "Hết hàng" || statusText == "Bị khóa")
                                {
                                    bgPill = Color.FromArgb(254, 226, 226); // red-50
                                    textPill = Color.FromArgb(220, 38, 38); // red-600
                                }
                                else if (statusText == "Chờ xử lý" || statusText == "Cảnh báo" || statusText == "Sắp hết hàng")
                                {
                                    bgPill = Color.FromArgb(255, 237, 213); // orange-50
                                    textPill = Color.FromArgb(234, 88, 12); // orange-600
                                }

                                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                                int pillWidth = 110;
                                int pillHeight = 22;
                                int px = e.CellBounds.Left + (e.CellBounds.Width - pillWidth) / 2;
                                int py = e.CellBounds.Top + (e.CellBounds.Height - pillHeight) / 2;

                                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                                {
                                    path.AddArc(px, py, pillHeight, pillHeight, 90, 180);
                                    path.AddArc(px + pillWidth - pillHeight, py, pillHeight, pillHeight, 270, 180);
                                    path.CloseFigure();
                                    using (var brush = new SolidBrush(bgPill))
                                    {
                                        e.Graphics.FillPath(brush, path);
                                    }
                                }

                                using (var font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold))
                                using (var brush = new SolidBrush(textPill))
                                {
                                    var sz = e.Graphics.MeasureString(statusText, font);
                                    e.Graphics.DrawString(statusText, font, brush, px + (pillWidth - sz.Width)/2, py + (pillHeight - sz.Height)/2 - 1);
                                }

                                e.Handled = true;
                            }
                        }
                    };
                }
            }

            // Recurse into children
            foreach (Control child in parent.Controls)
            {
                ModernizeControls(child);
            }
        }

        private string TranslateRole(string role)
        {
            switch (role)
            {
                case "Admin": return "Quản trị viên";
                case "WarehouseStaff": return "Nhân viên Kho";
                case "SalesStaff": return "Nhân viên Bán hàng";
                default: return role;
            }
        }

        private void ConfigurePermissions()
        {
            string role = SessionHelper.CurrentRole;
            if (role == "Admin")
            {
                // Full permission, keep all buttons visible
            }
            else if (role == "WarehouseStaff")
            {
                // Hide sales, returns, staff, dashboard
                btnTabDashboard.Visible = false;
                btnTabSales.Visible = false;
                btnTabReturns.Visible = false;
                btnTabStaff.Visible = false;
                
                // Reposition remaining buttons
                btnTabProducts.Location = new Point(6, 100);
                btnTabCategories.Location = new Point(6, 150);
                btnTabImport.Location = new Point(6, 200);
                btnTabPartners.Location = new Point(6, 250);
            }
            else if (role == "SalesStaff")
            {
                // Hide products config, categories, import, staff, dashboard
                btnTabDashboard.Visible = false;
                btnTabProducts.Visible = false;
                btnTabCategories.Visible = false;
                btnTabImport.Visible = false;
                btnTabStaff.Visible = false;

                // Reposition remaining buttons
                btnTabSales.Location = new Point(6, 100);
                btnTabReturns.Location = new Point(6, 150);
                btnTabPartners.Location = new Point(6, 200);
            }
        }

        private void SetActiveTab(Button btn, string title)
        {
            _currentActiveTabButton = btn;
            lblActiveTabTitle.Text = title.ToUpper();
            
            // Adjust panelActiveIndicator position to be perfectly aligned with the left edge of the active button
            panelActiveIndicator.Location = new Point(0, btn.Location.Y);
            panelActiveIndicator.BringToFront();

            // Clear styling of all sidebar buttons, set back to default transparent and slate-600
            foreach (Control ctrl in panelSidebar.Controls)
            {
                if (ctrl is Button b && b != btnLogout)
                {
                    // If it is the active button, make it stand out with white background and blue text
                    if (b == btn)
                    {
                        b.ForeColor = Color.FromArgb(0, 82, 204); // Brand Blue
                        b.BackColor = Color.White;
                    }
                    else
                    {
                        b.ForeColor = Color.FromArgb(71, 85, 105); // slate-600
                        b.BackColor = Color.Transparent;
                    }
                }
            }
        }

        private void TabButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string tabName = btn.Name.Replace("btnTab", "");

            switch (tabName)
            {
                case "Dashboard":
                    SetActiveTab(btn, "Dashboard");
                    ShowDashboardView();
                    break;
                case "Products":
                    SetActiveTab(btn, "Danh sách Sản Phẩm");
                    ShowProductsView();
                    break;
                case "Categories":
                    SetActiveTab(btn, "Hãng | Danh Mục");
                    ShowCategoriesView();
                    break;
                case "Import":
                    SetActiveTab(btn, "Nhập Kho / Đơn Nhập");
                    ShowImportView();
                    break;
                case "Sales":
                    SetActiveTab(btn, "Lập Hóa Đơn Bán Hàng");
                    ShowSalesView();
                    break;
                case "Returns":
                    SetActiveTab(btn, "Quản Lý Đổi Trả");
                    ShowReturnsView();
                    break;
                case "Partners":
                    SetActiveTab(btn, "Khách Hàng | Đối Tác");
                    ShowPartnersView();
                    break;
                case "Staff":
                    SetActiveTab(btn, "Nhân Viên & Tài Khoản");
                    ShowStaffView();
                    break;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất khỏi hệ thống không?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _authService.Logout();
                this.Hide();
                var loginForm = new LoginForm();
                loginForm.Show();
            }
        }

        private void ClearContainer()
        {
            panelMainContainer.Controls.Clear();
        }

        // Helper style methods to create beautiful premium components
        private Label CreateCardTitle(string text)
        {
            return new Label {
                Text = text,
                Font = new Font("Segoe UI Semibold", scale(11F), FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59), // slate-800
                AutoSize = true,
                Margin = new Padding(scale(10)),
                BackColor = Color.Transparent
            };
        }

        private DataGridView CreatePremiumGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59), // slate-800
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249), // slate-100 very soft lines
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 42 } // slightly taller for modern feel
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139); // slate-500
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.EnableHeadersVisualStyles = false;

            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255); // very soft blue highlight
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(0, 82, 204); // Brand Blue text
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            // Alternate row styling
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252); // slate-50
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);

            return grid;
        }

        private Panel CreatePremiumPanel(Color backColor, int padding = 15)
        {
            // Intercept dark themes and convert them to clean white panels!
            Color finalBackColor = backColor;
            if (backColor == Color.FromArgb(30, 41, 59) || 
                backColor == Color.FromArgb(15, 23, 42) || 
                backColor == Color.FromArgb(26, 36, 53) || 
                backColor == Color.FromArgb(17, 24, 39))
            {
                finalBackColor = Color.White;
            }

            var panel = new Panel
            {
                BackColor = finalBackColor,
                Padding = new Padding(padding)
            };

            // Draw clean slate-200 border for light backgrounds, or deep midnight border for dark backgrounds
            panel.Paint += (s, e) =>
            {
                Color borderColor = (finalBackColor == Color.White || finalBackColor == Color.FromArgb(248, 250, 252) || finalBackColor == Color.Transparent)
                    ? Color.FromArgb(226, 232, 240) // slate-200
                    : Color.FromArgb(44, 55, 78);   // slate-700
                using (var pen = new Pen(borderColor, 1))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };

            return panel;
        }

        /// <summary>
        /// CreatePremiumButton — Tạo nút bấm chuẩn hóa giao diện.
        /// Bao gồm: màu sắc thống nhất, bo góc 8px (GDI+ custom paint), hover/active feedback.
        /// Gọi hàm này thay vì khởi tạo Button thủ công ở mọi nơi trong form.
        /// </summary>
        private Button CreatePremiumButton(string text, Color backColor, Color foreColor)
        {
            Color finalBgColor = backColor;
            Color finalFgColor = foreColor;

            // Override legacy indigo colors to brand blue
            if (backColor == Color.FromArgb(79, 70, 229) || backColor == Color.FromArgb(67, 56, 202))
                finalBgColor = Color.FromArgb(0, 82, 204);

            var btn = new Button
            {
                Text = text,
                BackColor = finalBgColor,
                ForeColor = finalFgColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                Height = 38,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(finalBgColor, 0.12f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(finalBgColor, 0.08f);

            // ── Bo góc 8px bằng GDI+ (rounded corners for premium look) ──
            const int radius = 8;
            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var rc = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                Color bg = btn.ClientRectangle.Contains(btn.PointToClient(System.Windows.Forms.Cursor.Position))
                    ? btn.FlatAppearance.MouseOverBackColor
                    : finalBgColor;
                using (var path = RoundedRectPath(rc, radius))
                using (var brush = new System.Drawing.SolidBrush(bg))
                {
                    e.Graphics.FillPath(brush, path);
                }
                // Re-draw text
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var fb = new System.Drawing.SolidBrush(finalFgColor))
                    e.Graphics.DrawString(btn.Text, btn.Font, fb, rc, sf);
            };
            btn.Region = new System.Drawing.Region(RoundedRectPath(
                new Rectangle(0, 0, btn.Width, btn.Height), radius));
            btn.Resize += (s, e) => btn.Region = new System.Drawing.Region(
                RoundedRectPath(new Rectangle(0, 0, btn.Width, btn.Height), radius));

            return btn;
        }

        /// <summary>
        /// RoundedRectPath — Trả về GraphicsPath của hình chữ nhật bo góc.
        /// Dùng bởi CreatePremiumButton và AddEditorField để bo góc các control.
        /// </summary>
        private static System.Drawing.Drawing2D.GraphicsPath RoundedRectPath(Rectangle rc, int r)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rc.X, rc.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rc.Right - r * 2, rc.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rc.Right - r * 2, rc.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rc.X, rc.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        #region Views Implementation
        
        // ----------------------------------------------------
        // VIEW: DASHBOARD (ADMIN & STATS)
        // ----------------------------------------------------
        private void ShowDashboardView()
        {
            ClearContainer();
            var stats = _dashboardService.GetDashboardStats();

            // Main layout using TableLayoutPanel with 3 rows: KPI (scale(165)), Detailed content (Percent 100), Trend Analysis Banner (scale(75))
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, scale(165))); // Row 0: KPI Row
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Row 1: Detailed Content Row
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, scale(75))); // Row 2: Trend Analysis Banner Row

            // 1. KPI Panel Layout
            var kpiLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 4, Padding = new Padding(scale(10)) };
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            kpiLayout.Controls.Add(CreateKPICard(
                "Doanh thu hôm nay", 
                stats.RevenueToday.ToString("N0") + "đ", 
                "Hôm nay", 
                Color.FromArgb(239, 246, 255), // light brand blue
                Color.FromArgb(0, 82, 204), 
                "💵", 
                Color.FromArgb(239, 246, 255), 
                Color.FromArgb(0, 82, 204)
            ), 0, 0);

            kpiLayout.Controls.Add(CreateKPICard(
                "Hóa đơn hôm nay", 
                stats.OrdersCountToday.ToString() + " đơn", 
                "Hôm nay", 
                Color.FromArgb(241, 245, 249), // slate-100
                Color.FromArgb(100, 116, 139), // slate-500
                "📄", 
                Color.FromArgb(241, 245, 249), 
                Color.FromArgb(30, 41, 59) // slate-800
            ), 1, 0);

            kpiLayout.Controls.Add(CreateKPICard(
                "Cảnh báo hết hàng", 
                stats.LowStockCount.ToString() + " laptop", 
                "Khẩn cấp", 
                Color.FromArgb(254, 242, 242), // light red
                Color.FromArgb(220, 38, 38), 
                "⚠️", 
                Color.FromArgb(254, 242, 242), 
                Color.FromArgb(220, 38, 38)
            ), 2, 0);

            kpiLayout.Controls.Add(CreateKPICard(
                "Sản phẩm kinh doanh", 
                stats.ProductsCount.ToString() + " mẫu", 
                "Hoạt động", 
                Color.FromArgb(240, 253, 250), // light
                Color.FromArgb(13, 148, 136), 
                "💻", 
                Color.FromArgb(240, 253, 250), 
                Color.FromArgb(13, 148, 136)
            ), 3, 0);

            mainLayout.Controls.Add(kpiLayout, 0, 0);

            var contentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, Padding = new Padding(scale(15)) };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Setup local helper for FlowLayoutPanel child resizing
            Action<FlowLayoutPanel> adjustWidths = (flow) =>
            {
                int w = flow.ClientSize.Width - flow.Padding.Left - flow.Padding.Right - scale(10);
                if (w < scale(100)) w = scale(100);
                foreach (Control ctrl in flow.Controls)
                {
                    ctrl.MinimumSize = new Size(w, scale(80)); // Increased minimum height to scale(80)
                    ctrl.MaximumSize = new Size(w, 0);
                    ctrl.Width = w;
                }
            };

            // Left: Low stock alert grid (White background)
            var pnlLeft = CreatePremiumPanel(Color.White);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(0, 0, scale(10), 0);
            
            var pnlLeftHeader = new Panel { Dock = DockStyle.Top, Height = scale(40) };
            var lblLeft = CreateCardTitle("🔔 Cảnh báo tồn kho");
            lblLeft.Dock = DockStyle.Left;
            lblLeft.TextAlign = ContentAlignment.MiddleLeft;
            
            // View All button next to header
            var btnViewAllLow = new LinkLabel {
                Text = "Xem tất cả",
                Font = new Font("Segoe UI Semibold", scale(8.5F), FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 82, 204),
                LinkColor = Color.FromArgb(0, 82, 204),
                ActiveLinkColor = Color.FromArgb(9, 97, 239),
                VisitedLinkColor = Color.FromArgb(0, 82, 204),
                LinkBehavior = LinkBehavior.HoverUnderline,
                AutoSize = true,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, scale(10), 0)
            };
            btnViewAllLow.Click += (s, ev) => {
                SetActiveTab(btnTabProducts, "Danh sách Sản Phẩm");
                ShowProductsView();
            };
            pnlLeftHeader.Controls.Add(lblLeft);
            pnlLeftHeader.Controls.Add(btnViewAllLow);
            pnlLeft.Controls.Add(pnlLeftHeader);

            var flowLow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(scale(10)),
                BackColor = Color.White
            };
            pnlLeft.Controls.Add(flowLow);
            flowLow.BringToFront();

            foreach (var item in stats.LowStockAlerts)
            {
                var row = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    MinimumSize = new Size(0, scale(80)), // Increased minimum height to scale(80)
                    ColumnCount = 3,
                    RowCount = 1,
                    Margin = new Padding(0, scale(6), 0, scale(6)),
                    Padding = new Padding(scale(10), scale(10), scale(10), scale(10)),
                    BackColor = Color.FromArgb(248, 250, 252)
                };

                row.RowStyles.Clear();
                row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                row.ColumnStyles.Clear();
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, scale(50)));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, scale(110)));

                row.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                    }
                };

                // Nested TableLayoutPanel for the text info to prevent FlowLayoutPanel sizing bugs
                var pnlInfo = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                pnlInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var lblName = new Label
                {
                    Text = item.ProductName,
                    Font = new Font("Segoe UI Semibold", scale(9.5F), FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 0, 0, scale(2))
                };

                var lblCode = new Label
                {
                    Text = item.ProductCode,
                    Font = new Font("Segoe UI", scale(8F)),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0)
                };

                pnlInfo.Controls.Add(lblName, 0, 0);
                pnlInfo.Controls.Add(lblCode, 0, 1);

                int qtyVal = item.QuantityInStock;
                var pnlQty = new Panel
                {
                    Size = new Size(scale(28), scale(28)),
                    Anchor = AnchorStyles.None,
                    BackColor = Color.Transparent
                };
                pnlQty.Paint += (sender, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Color circleColor = qtyVal == 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(251, 146, 60);
                    using (var brush = new SolidBrush(circleColor))
                    {
                        pe.Graphics.FillEllipse(brush, 0, 0, pnlQty.Width - 1, pnlQty.Height - 1);
                    }
                    string qText = qtyVal.ToString();
                    using (var font = new Font("Segoe UI", scale(9F), FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var sz = pe.Graphics.MeasureString(qText, font);
                        pe.Graphics.DrawString(qText, font, brush, (pnlQty.Width - sz.Width)/2, (pnlQty.Height - sz.Height)/2 - 0.5f);
                    }
                };

                var pnlStatus = new Panel
                {
                    Size = new Size(scale(95), scale(24)),
                    Anchor = AnchorStyles.None,
                    BackColor = Color.Transparent
                };
                string statusText = item.QuantityInStock == 0 ? "HẾT HÀNG" : "SẮP HẾT HÀNG";
                pnlStatus.Paint += (sender, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Color bgPill = qtyVal == 0 ? Color.FromArgb(254, 226, 226) : Color.FromArgb(255, 237, 213);
                    Color textPill = qtyVal == 0 ? Color.FromArgb(220, 38, 38) : Color.FromArgb(234, 88, 12);
                    
                    int pw = pnlStatus.Width;
                    int ph = pnlStatus.Height;
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddArc(0, 0, ph - 1, ph - 1, 90, 180);
                        path.AddArc(pw - ph, 0, ph - 1, ph - 1, 270, 180);
                        path.CloseFigure();
                        using (var brush = new SolidBrush(bgPill))
                        {
                            pe.Graphics.FillPath(brush, path);
                        }
                    }
                    using (var font = new Font("Segoe UI Semibold", scale(7.5F), FontStyle.Bold))
                    using (var brush = new SolidBrush(textPill))
                    {
                        var sz = pe.Graphics.MeasureString(statusText, font);
                        pe.Graphics.DrawString(statusText, font, brush, (pw - sz.Width)/2, (ph - sz.Height)/2 - 0.5f);
                    }
                };

                row.Controls.Add(pnlInfo, 0, 0);
                row.Controls.Add(pnlQty, 1, 0);
                row.Controls.Add(pnlStatus, 2, 0);

                row.Resize += (s, e) =>
                {
                    int maxW = row.Width - scale(180);
                    if (maxW < scale(50)) maxW = scale(50);
                    lblName.MaximumSize = new Size(maxW, 0);
                    lblCode.MaximumSize = new Size(maxW, 0);
                    row.PerformLayout();
                };

                flowLow.Controls.Add(row);
            }

            flowLow.Resize += (s, e) => adjustWidths(flowLow);
            contentLayout.Controls.Add(pnlLeft, 0, 0);

            // Right: Best seller list (White background)
            var pnlRight = CreatePremiumPanel(Color.White);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(scale(10), 0, 0, 0);
            
            var pnlRightHeader = new Panel { Dock = DockStyle.Top, Height = scale(40) };
            var lblRight = CreateCardTitle("📈 Sản phẩm bán chạy");
            lblRight.Dock = DockStyle.Left;
            lblRight.TextAlign = ContentAlignment.MiddleLeft;
            pnlRightHeader.Controls.Add(lblRight);
            pnlRight.Controls.Add(pnlRightHeader);

            var flowBest = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(scale(10)),
                BackColor = Color.White
            };

            int rankIndex = 1;
            foreach (var item in stats.BestSellers)
            {
                string growth = "+15%";
                if (rankIndex == 2) growth = "+5%";
                else if (rankIndex >= 3) growth = "-2%";

                var row = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    MinimumSize = new Size(0, scale(80)), // Increased minimum height to scale(80)
                    ColumnCount = 3,
                    RowCount = 1,
                    Margin = new Padding(0, scale(6), 0, scale(6)),
                    Padding = new Padding(scale(10), scale(10), scale(10), scale(10)),
                    BackColor = Color.FromArgb(248, 250, 252)
                };

                row.RowStyles.Clear();
                row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                row.ColumnStyles.Clear();
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, scale(45)));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, scale(70)));

                row.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                    }
                };

                int rIndex = rankIndex;
                var pnlRank = new Panel
                {
                    Size = new Size(scale(26), scale(26)),
                    Anchor = AnchorStyles.None,
                    BackColor = Color.Transparent
                };
                pnlRank.Paint += (sender, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Color bgRank = Color.FromArgb(241, 245, 249);
                    Color textRank = Color.FromArgb(71, 85, 105);

                    if (rIndex == 1)
                    {
                        bgRank = Color.FromArgb(219, 234, 254);
                        textRank = Color.FromArgb(0, 82, 204);
                    }
                    else if (rIndex == 2)
                    {
                        bgRank = Color.FromArgb(240, 253, 250);
                        textRank = Color.FromArgb(13, 148, 136);
                    }

                    using (var brush = new SolidBrush(bgRank))
                    {
                        pe.Graphics.FillRectangle(brush, 0, 0, pnlRank.Width, pnlRank.Height);
                    }

                    string rText = rIndex.ToString();
                    using (var font = new Font("Segoe UI", scale(9.5F), FontStyle.Bold))
                    using (var brush = new SolidBrush(textRank))
                    {
                        var sz = pe.Graphics.MeasureString(rText, font);
                        pe.Graphics.DrawString(rText, font, brush, (pnlRank.Width - sz.Width)/2, (pnlRank.Height - sz.Height)/2 - 0.5f);
                    }
                };

                // Nested TableLayoutPanel for the text info to prevent FlowLayoutPanel sizing bugs
                var pnlInfo = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                pnlInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var lblName = new Label
                {
                    Text = item.ProductName,
                    Font = new Font("Segoe UI Semibold", scale(9.5F), FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 0, 0, scale(2))
                };

                string revText = item.TotalRevenue >= 1000000 
                    ? (item.TotalRevenue / 1000000m).ToString("N0") + "M"
                    : item.TotalRevenue.ToString("N0") + "đ";

                var lblSubtitle = new Label
                {
                    Text = $"{item.QuantitySold} đã bán • DT: {revText}",
                    Font = new Font("Segoe UI", scale(8F)),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0)
                };

                pnlInfo.Controls.Add(lblName, 0, 0);
                pnlInfo.Controls.Add(lblSubtitle, 0, 1);

                string growthVal = growth;
                bool isPos = growthVal.StartsWith("+");
                var pnlGrowth = new Panel
                {
                    Size = new Size(scale(52), scale(22)),
                    Anchor = AnchorStyles.None,
                    BackColor = Color.Transparent
                };
                pnlGrowth.Paint += (sender, pe) =>
                {
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Color bgPill = isPos ? Color.FromArgb(204, 251, 241) : Color.FromArgb(254, 226, 226);
                    Color textPill = isPos ? Color.FromArgb(13, 148, 136) : Color.FromArgb(220, 38, 38);

                    int pw = pnlGrowth.Width;
                    int ph = pnlGrowth.Height;
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddArc(0, 0, ph - 1, ph - 1, 90, 180);
                        path.AddArc(pw - ph, 0, ph - 1, ph - 1, 270, 180);
                        path.CloseFigure();
                        using (var brush = new SolidBrush(bgPill))
                        {
                            pe.Graphics.FillPath(brush, path);
                        }
                    }

                    using (var font = new Font("Segoe UI Semibold", scale(8F), FontStyle.Bold))
                    using (var brush = new SolidBrush(textPill))
                    {
                        var sz = pe.Graphics.MeasureString(growthVal, font);
                        pe.Graphics.DrawString(growthVal, font, brush, (pw - sz.Width)/2, (ph - sz.Height)/2 - 0.5f);
                    }
                };

                row.Controls.Add(pnlRank, 0, 0);
                row.Controls.Add(pnlInfo, 1, 0);
                row.Controls.Add(pnlGrowth, 2, 0);

                row.Resize += (s, e) =>
                {
                    int maxW = row.Width - scale(135);
                    if (maxW < scale(50)) maxW = scale(50);
                    lblName.MaximumSize = new Size(maxW, 0);
                    lblSubtitle.MaximumSize = new Size(maxW, 0);
                    row.PerformLayout();
                };

                flowBest.Controls.Add(row);
                rankIndex++;
            }

            // Add Suggestion box at bottom of FlowBest flow layout
            var pnlSuggest = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(scale(12)),
                Margin = new Padding(0, scale(15), 0, scale(10)),
                BackColor = Color.Transparent
            };
            pnlSuggest.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(191, 219, 254), 1.5F))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSuggest.Width - 1, pnlSuggest.Height - 1);
                }
            };
            
            Label lblSuggestTitle = new Label {
                Text = "💡 GỢI Ý QUẢN TRỊ",
                Font = new Font("Segoe UI Semibold", scale(8F), FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 82, 204),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, scale(4)),
                BackColor = Color.Transparent
            };
            pnlSuggest.Controls.Add(lblSuggestTitle);

            Label lblSuggestContent = new Label {
                Text = "Nhập thêm Dell XPS 13 để đáp ứng nhu cầu tăng cao trong mùa tựu trường sắp tới.",
                Font = new Font("Segoe UI", scale(8.5F)),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            pnlSuggest.Controls.Add(lblSuggestContent);

            pnlSuggest.Resize += (s, ev) => {
                int maxW = pnlSuggest.Width - pnlSuggest.Padding.Left - pnlSuggest.Padding.Right - scale(5);
                if (maxW < scale(50)) maxW = scale(50);
                lblSuggestContent.MaximumSize = new Size(maxW, 0);
            };

            flowBest.Controls.Add(pnlSuggest);

            flowBest.Resize += (s, e) => adjustWidths(flowBest);
            pnlRight.Controls.Add(flowBest);
            flowBest.BringToFront();
            contentLayout.Controls.Add(pnlRight, 1, 0);

            mainLayout.Controls.Add(contentLayout, 0, 1);

            // 3. Bottom Trend Analysis Banner (Gradient card)
            var pnlTrend = new Panel { Dock = DockStyle.Fill, Margin = new Padding(scale(15), 0, scale(15), scale(15)) };
            
            var pnlTrendIcon = new Panel { Size = new Size(scale(36), scale(36)), BackColor = Color.Transparent };
            pnlTrendIcon.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(219, 234, 254)))
                {
                    ev.Graphics.FillEllipse(brush, 0, 0, pnlTrendIcon.Width - 1, pnlTrendIcon.Height - 1);
                }
                using (var font = new Font("Segoe UI", scale(11F)))
                using (var brush = new SolidBrush(Color.FromArgb(0, 82, 204)))
                {
                    var sz = ev.Graphics.MeasureString("📊", font);
                    ev.Graphics.DrawString("📊", font, brush, (pnlTrendIcon.Width - sz.Width)/2, (pnlTrendIcon.Height - sz.Height)/2 - 0.5f);
                }
            };
            pnlTrend.Controls.Add(pnlTrendIcon);

            var lblTrendTitle = new Label {
                Text = "Phân tích xu hướng",
                Font = new Font("Segoe UI Semibold", scale(11F), FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlTrend.Controls.Add(lblTrendTitle);

            var lblTrendSub = new Label {
                Text = "Dữ liệu doanh thu sẽ được tự động cập nhật và trực quan hóa tại đây khi có các giao dịch phát sinh trong ngày.",
                Font = new Font("Segoe UI", scale(8.5F)),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlTrend.Controls.Add(lblTrendSub);

            var btnViewTrend = CreatePremiumButton("Xem Chi Tiết", Color.FromArgb(0, 82, 204), Color.White);
            btnViewTrend.Font = new Font("Segoe UI Semibold", scale(8.5F), FontStyle.Bold);
            btnViewTrend.Size = new Size(scale(150), scale(28));
            btnViewTrend.Click += (s, ev) => {
                var trendForm = new TrendReportForm(_unitOfWork);
                trendForm.ShowDialog(this);
            };
            pnlTrend.Controls.Add(btnViewTrend);

            pnlTrend.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    pnlTrend.ClientRectangle,
                    Color.FromArgb(226, 232, 240), // slate-200
                    Color.FromArgb(248, 250, 252), // slate-50
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, pnlTrend.ClientRectangle);
                }
                using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlTrend.Width - 1, pnlTrend.Height - 1);
                }
            };

            pnlTrend.Resize += (s, e) => {
                pnlTrendIcon.Location = new Point(scale(15), (pnlTrend.Height - pnlTrendIcon.Height)/2);
                lblTrendTitle.Location = new Point(scale(60), (pnlTrend.Height - pnlTrendIcon.Height)/2 - scale(10));
                
                lblTrendSub.AutoSize = false;
                lblTrendSub.Width = pnlTrend.Width - btnViewTrend.Width - scale(95);
                lblTrendSub.Height = scale(24);
                lblTrendSub.Location = new Point(scale(60), (pnlTrend.Height - pnlTrendIcon.Height)/2 + scale(12));

                btnViewTrend.Location = new Point(pnlTrend.Width - btnViewTrend.Width - scale(15), (pnlTrend.Height - btnViewTrend.Height)/2);
            };

            mainLayout.Controls.Add(pnlTrend, 0, 2);

            panelMainContainer.Controls.Add(mainLayout);
        }

        private Panel CreateKPICard(string title, string value, string tagText, Color tagBg, Color tagFore, string emoji, Color emojiBg, Color valueColor)
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                Padding = new Padding(15, 15, 15, 12)
            };

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1)) // slate-200
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };

            // pnlHeader chỉ chứa badge tag — giảm chiều cao để nhường chỗ cho phần chữ bên dưới
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                BackColor = Color.Transparent
            };

            // Biểu tượng emoji đã được xóa vì không hiển thị được trên mọi máy (lỗi font Segoe UI Emoji)

            var pnlTag = new Panel { Size = new Size(80, 22), BackColor = Color.Transparent };
            pnlTag.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(tagBg))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int h = pnlTag.Height;
                    path.AddArc(0, 0, h, h, 90, 180);
                    path.AddArc(pnlTag.Width - h, 0, h, h, 270, 180);
                    path.CloseFigure();
                    e.Graphics.FillPath(brush, path);
                }
                using (var font = new Font("Segoe UI Semibold", 7.5F, FontStyle.Bold))
                using (var brush = new SolidBrush(tagFore))
                {
                    var sz = e.Graphics.MeasureString(tagText, font);
                    e.Graphics.DrawString(tagText, font, brush, (pnlTag.Width - sz.Width)/2, (pnlTag.Height - sz.Height)/2 - 1);
                }
            };
            pnlTag.Location = new Point(Math.Max(0, pnlHeader.Width - 80), 5);
            pnlHeader.Resize += (s, e) => {
                pnlTag.Location = new Point(Math.Max(0, pnlHeader.Width - 80), 5);
            };
            pnlHeader.Controls.Add(pnlTag);

            // Padding = 0 để chữ sát lên đầu, tránh bị cắt phần đuôi
            var pnlTextLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 2, 0, 0),
                BackColor = Color.Transparent
            };

            // Phóng to chữ sau khi xóa biểu tượng để lấp chỗ trống
            var lblT = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184), // slate-400
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2),
                BackColor = Color.Transparent
            };
            pnlTextLayout.Controls.Add(lblT);

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = valueColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            };
            pnlTextLayout.Controls.Add(lblVal);

            panel.Controls.Add(pnlTextLayout);
            panel.Controls.Add(pnlHeader);

            return panel;
        }

        // ----------------------------------------------------
        // VIEW: PRODUCTS CRUD
        // ----------------------------------------------------
        private DataGridView _gridProducts;
        private Panel _pnlProductEditor;
        private TextBox _txtProductCode, _txtProductName, _txtProductCPU, _txtProductRAM, _txtProductGPU, _txtProductStorage, _txtProductScreen, _txtProductImportPrice, _txtProductSalePrice, _txtProductImageUrl;
        private ComboBox _cbProductCategory, _cbProductBrand;
        private CheckBox _chkProductActive;
        private int _selectedProductIdForEdit = 0;

        private void ShowProductsView()
        {
            ClearContainer();

            // Main split panel: Left is list & search, right is slider editor
            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            var pnlLeft = CreatePremiumPanel(Color.White, 0);
            pnlLeft.Dock = DockStyle.Fill;

            // Search and Control panel
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(241, 245, 249) };
            var txtSearch = new TextBox { Location = new Point(15, 18), Width = 180, Font = new Font("Segoe UI", 11F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle };
            var lblS = new Label { Text = "Tìm:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(15, 2), Font = new Font("Segoe UI", 8F), AutoSize = true };
            var btnSearch = CreatePremiumButton("Tìm Kiếm", Color.FromArgb(245, 158, 11), Color.White);
            btnSearch.Location = new Point(205, 15);
            btnSearch.Width = 90;

            var btnAdd = CreatePremiumButton("+ Thêm Laptop", Color.FromArgb(16, 185, 129), Color.White);
            btnAdd.Location = new Point(305, 15);
            btnAdd.Width = 120;

            var btnSerial = CreatePremiumButton("Xem Serial", Color.FromArgb(109, 40, 217), Color.White);
            btnSerial.Location = new Point(435, 15);
            btnSerial.Width = 110;
            btnSerial.Click += (s, ev) => {
                if (_gridProducts.SelectedRows.Count > 0)
                {
                    int prodId = Convert.ToInt32(_gridProducts.SelectedRows[0].Cells["ProductId"].Value);
                    string prodName = _gridProducts.SelectedRows[0].Cells["ProductName"].Value?.ToString() ?? "";
                    ShowSerialManagerPopup(prodId, prodName);
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một sản phẩm trong danh sách trước khi xem Serial.",
                        "Chưa chọn sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(lblS);
            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnAdd);
            pnlSearch.Controls.Add(btnSerial);

            pnlLeft.Controls.Add(pnlSearch);

            // Products Grid
            _gridProducts = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridProducts);
            _gridProducts.BringToFront();

            _gridProducts.Columns.Add("ProductId", "ID");
            _gridProducts.Columns["ProductId"].Visible = false;
            _gridProducts.Columns.Add("ProductCode", "Mã SP");
            _gridProducts.Columns.Add("ProductName", "Tên Máy");
            _gridProducts.Columns.Add("CategoryName", "Danh Mục");
            _gridProducts.Columns.Add("BrandName", "Hãng");
            _gridProducts.Columns.Add("ImportPrice", "Giá Nhập");
            _gridProducts.Columns.Add("SalePrice", "Giá Bán");
            _gridProducts.Columns.Add("QuantityInStock", "Tồn Kho");
            _gridProducts.Columns.Add("Status", "Trạng Thái");

            // Editor Panel (Right Side)
            _pnlProductEditor = CreatePremiumPanel(Color.White, 20);
            _pnlProductEditor.Dock = DockStyle.Fill;
            var lblEdTitle = CreateCardTitle("📝 THÔNG TIN LAPTOP");
            lblEdTitle.Dock = DockStyle.Top;
            _pnlProductEditor.Controls.Add(lblEdTitle);

            // FlowLayout inside editor to hold labels and fields
            var editorFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 10, 0, 0) };
            
            _txtProductCode = AddEditorField(editorFlow, "Mã Sản Phẩm (SKU) *:");
            _txtProductName = AddEditorField(editorFlow, "Tên Laptop *:");
            
            _cbProductCategory = AddEditorCombo(editorFlow, "Danh Mục *:");
            _cbProductBrand = AddEditorCombo(editorFlow, "Thương Hiệu *:");

            _txtProductCPU = AddEditorField(editorFlow, "CPU *:");
            _txtProductRAM = AddEditorField(editorFlow, "RAM *:");
            _txtProductGPU = AddEditorField(editorFlow, "GPU:");
            _txtProductStorage = AddEditorField(editorFlow, "Ổ cứng *:");
            _txtProductScreen = AddEditorField(editorFlow, "Màn hình:");
            _txtProductImportPrice = AddEditorField(editorFlow, "Giá Nhập Gốc (Reference) *:");
            _txtProductSalePrice = AddEditorField(editorFlow, "Giá Bán Công Bố *:");
            _txtProductImageUrl = AddEditorField(editorFlow, "URL Ảnh sản phẩm:");
            
            _chkProductActive = new CheckBox { Text = "Sản phẩm còn kinh doanh", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 10), Checked = true };
            editorFlow.Controls.Add(_chkProductActive);

            var pnlEditButtons = new Panel { Height = 50, Width = 300 };
            var btnSave = CreatePremiumButton("LƯU LẠI", Color.FromArgb(16, 185, 129), Color.White);
            btnSave.Width = 100;
            btnSave.Location = new Point(0, 5);
            btnSave.Click += (s, ev) => SaveProduct();

            var btnCancel = CreatePremiumButton("HỦY BỎ", Color.FromArgb(100, 116, 139), Color.White);
            btnCancel.Width = 100;
            btnCancel.Location = new Point(110, 5);
            btnCancel.Click += (s, ev) => ResetProductEditor();

            pnlEditButtons.Controls.Add(btnSave);
            pnlEditButtons.Controls.Add(btnCancel);
            editorFlow.Controls.Add(pnlEditButtons);

            _pnlProductEditor.Controls.Add(editorFlow);
            editorFlow.BringToFront();

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(_pnlProductEditor, 1, 0);
            panelMainContainer.Controls.Add(mainLayout);

            // Populate Comboboxes
            var cats = _productService.GetCategories();
            _cbProductCategory.DataSource = cats;
            _cbProductCategory.DisplayMember = "CategoryName";
            _cbProductCategory.ValueMember = "CategoryId";

            var brands = _productService.GetBrands();
            _cbProductBrand.DataSource = brands;
            _cbProductBrand.DisplayMember = "BrandName";
            _cbProductBrand.ValueMember = "BrandId";

            // Load products in grid
            LoadProductsGrid();

            // Setup events
            btnSearch.Click += (s, ev) => LoadProductsGrid(txtSearch.Text);
            btnAdd.Click += (s, ev) => {
                ResetProductEditor();
                _selectedProductIdForEdit = 0;
                _txtProductCode.ReadOnly = false;
                lblEdTitle.Text = "📝 THÊM MỚI LAPTOP";
            };

            _gridProducts.SelectionChanged += (s, ev) => {
                if (_gridProducts.SelectedRows.Count > 0)
                {
                    var row = _gridProducts.SelectedRows[0];
                    int prodId = Convert.ToInt32(row.Cells["ProductId"].Value);
                    LoadProductToEditor(prodId);
                    lblEdTitle.Text = "📝 CẬP NHẬT LAPTOP";
                }
            };
        }

        private void LoadProductsGrid(string search = null)
        {
            _gridProducts.Rows.Clear();
            var products = _productService.GetProducts(search, null, null, null);
            foreach (var p in products)
            {
                _gridProducts.Rows.Add(
                    p.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.CategoryName,
                    p.BrandName,
                    p.ImportPrice.ToString("N0") + " đ",
                    p.SalePrice.ToString("N0") + " đ",
                    p.QuantityInStock,
                    p.IsActive ? "Còn kinh doanh" : "Ngừng kinh doanh"
                );
            }
        }

        private void ShowSerialManagerPopup(int productId, string productName)
        {
            var popup = new Form
            {
                Text = $"🔍 Quản lý Serial — {productName}",
                Size = new Size(700, 520),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(248, 250, 252),
                Font = new Font("Segoe UI", 9F)
            };

            // ── Header ──────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlHeader.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            var lblPopTitle = new Label
            {
                Text = $"📋  DANH SÁCH SERIAL — {productName.ToUpper()}",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(18, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblPopTitle);

            // ── Legend row ──────────────────────────────────────────────
            var pnlLegend = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(12, 0, 0, 0) };
            void AddLegendDot(string hex, string text, int x) {
                var dot = new Label { BackColor = ColorTranslator.FromHtml(hex), Location = new Point(x, 10), Size = new Size(12, 12) };
                var lbl = new Label { Text = text, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 8.5F), Location = new Point(x + 15, 8), AutoSize = true };
                pnlLegend.Controls.Add(dot); pnlLegend.Controls.Add(lbl);
            }
            AddLegendDot("#10B981", "Còn trong kho", 12);
            AddLegendDot("#EF4444", "Đã bán (không thể sửa/xóa)", 120);
            AddLegendDot("#F59E0B", "Trạng thái khác", 320);

            // ── Grid ─────────────────────────────────────────────────────
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 10F),
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 34 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(224, 231, 255),
                SelectionForeColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(6, 0, 0, 0)
            };
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252)
            };

            grid.Columns.Add("Serial", "Số Serial");
            grid.Columns.Add("Status", "Trạng Thái");
            grid.Columns["Status"].FillWeight = 40;

            // ── Footer toolbar ────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Color.FromArgb(241, 245, 249) };
            pnlFooter.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
            };

            var btnEdit = CreatePremiumButton("Sửa Serial", Color.FromArgb(245, 158, 11), Color.White);
            btnEdit.Width = 130; btnEdit.Location = new Point(12, 12);
            var btnDelete = CreatePremiumButton("Xóa Serial", Color.FromArgb(239, 68, 68), Color.White);
            btnDelete.Width = 130; btnDelete.Location = new Point(154, 12);
            var btnClose2 = CreatePremiumButton("Đóng", Color.FromArgb(100, 116, 139), Color.White);
            btnClose2.Width = 100; btnClose2.Location = new Point(298, 12);
            btnClose2.Click += (s, e) => popup.Close();

            // ── Load serial data ──────────────────────────────────────────
            void LoadSerialGrid() {
                try {
                    grid.Rows.Clear();
                    var items = _warehouseService.GetAllProductItems(productId);
                    foreach (var item in items)
                    {
                        string statusDisplay = item.Status == "InStock" ? "✅ Còn kho"
                            : item.Status == "Sold" ? "🔴 Đã bán"
                            : $"⚠️ {item.Status}";
                        int rowIdx = grid.Rows.Add(item.SerialNumber, statusDisplay);
                        var row = grid.Rows[rowIdx];
                        if (item.Status == "Sold")
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(239, 68, 68);
                        else if (item.Status == "InStock")
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                        else
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Lỗi khi tải danh sách serial:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            LoadSerialGrid();

            // ── Edit Serial ───────────────────────────────────────────────
            btnEdit.Click += (s, e) => {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("Chọn một serial để sửa.", "Thông báo"); return; }
                string oldSerial = grid.SelectedRows[0].Cells["Serial"].Value?.ToString() ?? "";
                string statusCell = grid.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
                if (statusCell.Contains("Đã bán")) { MessageBox.Show("Không thể sửa serial đã bán cho khách hàng.", "Chặn thao tác", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                // Input dialog
                var dlg = new Form { Text = "Sửa Serial Number", Size = new Size(380, 170), StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, BackColor = Color.FromArgb(248, 250, 252) };
                var lblNew = new Label { Text = $"Serial mới (cũ: {oldSerial}):", Location = new Point(16, 18), AutoSize = true, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59) };
                var txtNew = new TextBox { Text = oldSerial, Location = new Point(16, 42), Width = 334, Font = new Font("Segoe UI", 11F), BorderStyle = BorderStyle.FixedSingle };
                var btnOk = new Button { Text = "Lưu", Location = new Point(174, 90), Size = new Size(80, 30), DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(16, 185, 129), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnCancelDlg = new Button { Text = "Hủy", Location = new Point(262, 90), Size = new Size(80, 30), DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(100, 116, 139), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnCancelDlg.FlatAppearance.BorderSize = 0;
                dlg.AcceptButton = btnOk; dlg.CancelButton = btnCancelDlg;
                dlg.Controls.AddRange(new Control[] { lblNew, txtNew, btnOk, btnCancelDlg });

                if (dlg.ShowDialog(popup) == DialogResult.OK)
                {
                    string newSerial = txtNew.Text.Trim();
                    if (string.IsNullOrWhiteSpace(newSerial)) { MessageBox.Show("Serial không được để trống.", "Lỗi"); return; }
                    try {
                        _warehouseService.UpdateSerialNumber(oldSerial, newSerial);
                        MessageBox.Show($"✅ Đã cập nhật serial thành công!\n{oldSerial}  →  {newSerial}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSerialGrid();
                        LoadProductsGrid();
                    } catch (Exception ex) {
                        MessageBox.Show("Lỗi khi cập nhật serial:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            // ── Delete Serial ──────────────────────────────────────────────
            btnDelete.Click += (s, e) => {
                if (grid.SelectedRows.Count == 0) { MessageBox.Show("Chọn một serial để xóa.", "Thông báo"); return; }
                string serial = grid.SelectedRows[0].Cells["Serial"].Value?.ToString() ?? "";
                string statusCell = grid.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";
                if (statusCell.Contains("Đã bán")) { MessageBox.Show("Không thể xóa serial đã bán cho khách hàng.", "Chặn thao tác", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                var confirm = MessageBox.Show(
                    $"Bạn chắc chắn muốn xóa serial:\n\n  {serial}\n\nThao tác này sẽ giảm tồn kho 1 đơn vị và không thể hoàn tác!",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                {
                    try {
                        _warehouseService.DeleteSerialNumber(serial);
                        MessageBox.Show($"✅ Đã xóa serial '{serial}' thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSerialGrid();
                        LoadProductsGrid();
                    } catch (Exception ex) {
                        MessageBox.Show("Lỗi khi xóa serial:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            pnlFooter.Controls.AddRange(new Control[] { btnEdit, btnDelete, btnClose2 });

            popup.Controls.Add(grid);
            popup.Controls.Add(pnlLegend);
            popup.Controls.Add(pnlHeader);
            popup.Controls.Add(pnlFooter);

            popup.ShowDialog(this);
        }

        private TextBox AddEditorField(FlowLayoutPanel parent, string labelText)
        {
            var pnl = new Panel { Width = 300, Height = 52, Margin = new Padding(0) };
            var lbl = new Label { Text = labelText, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 2) };
            var txt = new TextBox { Width = 280, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 20) };
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(txt);
            parent.Controls.Add(pnl);
            return txt;
        }

        private ComboBox AddEditorCombo(FlowLayoutPanel parent, string labelText)
        {
            var pnl = new Panel { Width = 300, Height = 52, Margin = new Padding(0) };
            var lbl = new Label { Text = labelText, ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), AutoSize = true, Location = new Point(0, 2) };
            var cb = new ComboBox { Width = 280, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), FlatStyle = FlatStyle.Flat, Location = new Point(0, 20) };
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(cb);
            parent.Controls.Add(pnl);
            return cb;
        }

        private void ResetProductEditor()
        {
            _selectedProductIdForEdit = 0;
            _txtProductCode.Text = "";
            _txtProductCode.ReadOnly = false;
            _txtProductName.Text = "";
            _txtProductCPU.Text = "";
            _txtProductRAM.Text = "";
            _txtProductGPU.Text = "";
            _txtProductStorage.Text = "";
            _txtProductScreen.Text = "";
            _txtProductImportPrice.Text = "";
            _txtProductSalePrice.Text = "";
            _txtProductImageUrl.Text = "";
            _chkProductActive.Checked = true;
        }

        private void LoadProductToEditor(int prodId)
        {
            var p = _productService.GetProductById(prodId);
            if (p == null) return;

            _selectedProductIdForEdit = p.ProductId;
            _txtProductCode.Text = p.ProductCode;
            _txtProductCode.ReadOnly = true; // SKU is unique and shouldn't change
            _txtProductName.Text = p.ProductName;
            _cbProductCategory.SelectedValue = p.CategoryId;
            _cbProductBrand.SelectedValue = p.BrandId;
            _txtProductCPU.Text = p.CPU;
            _txtProductRAM.Text = p.RAM;
            _txtProductGPU.Text = p.GPU;
            _txtProductStorage.Text = p.Storage;
            _txtProductScreen.Text = p.ScreenSize;
            _txtProductImportPrice.Text = p.ImportPrice.ToString("F0");
            _txtProductSalePrice.Text = p.SalePrice.ToString("F0");
            _txtProductImageUrl.Text = p.ImageUrl;
            _chkProductActive.Checked = p.IsActive;
        }

        private void SaveProduct()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_txtProductCode.Text) || string.IsNullOrWhiteSpace(_txtProductName.Text) ||
                    string.IsNullOrWhiteSpace(_txtProductCPU.Text) || string.IsNullOrWhiteSpace(_txtProductRAM.Text) ||
                    string.IsNullOrWhiteSpace(_txtProductStorage.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ các thông tin bắt buộc (*).", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(_txtProductImportPrice.Text, out decimal impPrice) || impPrice <= 0 ||
                    !decimal.TryParse(_txtProductSalePrice.Text, out decimal salePrice) || salePrice <= 0)
                {
                    MessageBox.Show("Giá nhập và giá bán phải là số và lớn hơn 0.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (salePrice < impPrice)
                {
                    MessageBox.Show("Cảnh báo: Giá bán hiện thấp hơn giá nhập của sản phẩm.", "Cảnh báo giá", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                var dto = new ProductDTO
                {
                    ProductId = _selectedProductIdForEdit,
                    ProductCode = _txtProductCode.Text.Trim(),
                    ProductName = _txtProductName.Text.Trim(),
                    CategoryId = (int)_cbProductCategory.SelectedValue,
                    BrandId = (int)_cbProductBrand.SelectedValue,
                    CPU = _txtProductCPU.Text.Trim(),
                    RAM = _txtProductRAM.Text.Trim(),
                    GPU = _txtProductGPU.Text.Trim(),
                    Storage = _txtProductStorage.Text.Trim(),
                    ScreenSize = _txtProductScreen.Text.Trim(),
                    ImportPrice = impPrice,
                    SalePrice = salePrice,
                    ImageUrl = _txtProductImageUrl.Text.Trim(),
                    IsActive = _chkProductActive.Checked
                };

                bool result;
                if (_selectedProductIdForEdit == 0)
                {
                    result = _productService.CreateProduct(dto);
                }
                else
                {
                    result = _productService.UpdateProduct(dto);
                }

                if (result)
                {
                    MessageBox.Show("Lưu thông tin laptop thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProductsGrid();
                    ResetProductEditor();
                }
                else
                {
                    MessageBox.Show("Không thể lưu sản phẩm. Có thể Mã Sản Phẩm đã bị trùng lặp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // ----------------------------------------------------
        // VIEW: CATEGORIES & BRANDS (WAREHOUSE & ADMIN)
        // ----------------------------------------------------
        private DataGridView _gridCategories, _gridBrands;
        private TextBox _txtCategoryName, _txtBrandName;

        private void ShowCategoriesView()
        {
            ClearContainer();

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, Padding = new Padding(10) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Left side: Categories
            var pnlLeft = CreatePremiumPanel(Color.White, 15);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(5);
            var lblLeft = CreateCardTitle("📁 DANH MỤC SẢN PHẨM");
            lblLeft.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(lblLeft);

            var pnlCatInput = new Panel { Dock = DockStyle.Top, Height = 80 };
            var lblCatName = new Label { Text = "Tên danh mục mới:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(0, 10), AutoSize = true };
            _txtCategoryName = new TextBox { Width = 220, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 32) };
            
            var btnAddCat = CreatePremiumButton("Thêm Mới", Color.FromArgb(16, 185, 129), Color.White);
            btnAddCat.Location = new Point(230, 30);
            btnAddCat.Width = 80;
            btnAddCat.Click += (s, ev) => {
                string name = _txtCategoryName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name)) return;
                if (_productService.CreateCategory(name)) {
                    MessageBox.Show("Thêm danh mục thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtCategoryName.Clear();
                    LoadCategoriesGrid();
                } else {
                    MessageBox.Show("Thêm thất bại. Danh mục có thể đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            var btnDelCat = CreatePremiumButton("Xóa Đã Chọn", Color.FromArgb(239, 68, 68), Color.White);
            btnDelCat.Location = new Point(320, 30);
            btnDelCat.Width = 100;
            btnDelCat.Click += (s, ev) => {
                if (_gridCategories.SelectedRows.Count == 0) return;
                int id = Convert.ToInt32(_gridCategories.SelectedRows[0].Cells["CategoryId"].Value);
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này? Chỉ có thể xóa danh mục chưa có sản phẩm nào.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    if (_productService.DeleteCategory(id)) {
                        MessageBox.Show("Xóa danh mục thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCategoriesGrid();
                    } else {
                        MessageBox.Show("Xóa thất bại! Danh mục đang có chứa sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            pnlCatInput.Controls.Add(lblCatName);
            pnlCatInput.Controls.Add(_txtCategoryName);
            pnlCatInput.Controls.Add(btnAddCat);
            pnlCatInput.Controls.Add(btnDelCat);
            pnlLeft.Controls.Add(pnlCatInput);

            _gridCategories = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridCategories);
            _gridCategories.BringToFront();
            _gridCategories.Columns.Add("CategoryId", "ID");
            _gridCategories.Columns["CategoryId"].Visible = false;
            _gridCategories.Columns.Add("CategoryName", "Tên Danh Mục");
            _gridCategories.Columns.Add("ProductCount", "Số Sản Phẩm");

            mainLayout.Controls.Add(pnlLeft, 0, 0);

            // Right side: Brands
            var pnlRight = CreatePremiumPanel(Color.White, 15);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5);
            var lblRight = CreateCardTitle("🏷️ THƯƠNG HIỆU / HÃNG");
            lblRight.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblRight);

            var pnlBrandInput = new Panel { Dock = DockStyle.Top, Height = 80 };
            var lblBrandName = new Label { Text = "Tên thương hiệu mới:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(0, 10), AutoSize = true };
            _txtBrandName = new TextBox { Width = 220, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 32) };

            var btnAddBrand = CreatePremiumButton("Thêm Mới", Color.FromArgb(16, 185, 129), Color.White);
            btnAddBrand.Location = new Point(230, 30);
            btnAddBrand.Width = 80;
            btnAddBrand.Click += (s, ev) => {
                string name = _txtBrandName.Text.Trim();
                if (string.IsNullOrWhiteSpace(name)) return;
                if (_productService.CreateBrand(name)) {
                    MessageBox.Show("Thêm hãng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtBrandName.Clear();
                    LoadBrandsGrid();
                } else {
                    MessageBox.Show("Thêm thất bại. Thương hiệu có thể đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            var btnDelBrand = CreatePremiumButton("Xóa Đã Chọn", Color.FromArgb(239, 68, 68), Color.White);
            btnDelBrand.Location = new Point(320, 30);
            btnDelBrand.Width = 100;
            btnDelBrand.Click += (s, ev) => {
                if (_gridBrands.SelectedRows.Count == 0) return;
                int id = Convert.ToInt32(_gridBrands.SelectedRows[0].Cells["BrandId"].Value);
                if (MessageBox.Show("Bạn có chắc muốn xóa thương hiệu này? Chỉ xóa được hãng chưa có sản phẩm nào.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    if (_productService.DeleteBrand(id)) {
                        MessageBox.Show("Xóa hãng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadBrandsGrid();
                    } else {
                        MessageBox.Show("Xóa thất bại! Hãng đang có chứa sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            pnlBrandInput.Controls.Add(lblBrandName);
            pnlBrandInput.Controls.Add(_txtBrandName);
            pnlBrandInput.Controls.Add(btnAddBrand);
            pnlBrandInput.Controls.Add(btnDelBrand);
            pnlRight.Controls.Add(pnlBrandInput);

            _gridBrands = CreatePremiumGrid();
            pnlRight.Controls.Add(_gridBrands);
            _gridBrands.BringToFront();
            _gridBrands.Columns.Add("BrandId", "ID");
            _gridBrands.Columns["BrandId"].Visible = false;
            _gridBrands.Columns.Add("BrandName", "Tên Thương Hiệu");
            _gridBrands.Columns.Add("ProductCount", "Số Sản Phẩm");

            mainLayout.Controls.Add(pnlRight, 1, 0);

            panelMainContainer.Controls.Add(mainLayout);

            LoadCategoriesGrid();
            LoadBrandsGrid();
        }

        private void LoadCategoriesGrid()
        {
            _gridCategories.Rows.Clear();
            var cats = _productService.GetCategories();
            foreach (var c in cats)
            {
                _gridCategories.Rows.Add(c.CategoryId, c.CategoryName, c.ProductCount);
            }
        }

        private void LoadBrandsGrid()
        {
            _gridBrands.Rows.Clear();
            var brands = _productService.GetBrands();
            foreach (var b in brands)
            {
                _gridBrands.Rows.Add(b.BrandId, b.BrandName, b.ProductCount);
            }
        }

        // ----------------------------------------------------
        // VIEW: PARTNERS (CUSTOMERS & SUPPLIERS)
        // ----------------------------------------------------
        private DataGridView _gridCustomers, _gridSuppliers;
        private TextBox _txtCustName, _txtCustPhone, _txtCustEmail, _txtCustAddress;
        private TextBox _txtSuppName, _txtSuppPhone, _txtSuppEmail, _txtSuppAddress;
        private int _selectedCustId = 0, _selectedSuppId = 0;

        private void ShowPartnersView()
        {
            ClearContainer();

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, Padding = new Padding(10) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Left Column: Customers
            var pnlLeft = CreatePremiumPanel(Color.White, 15);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(5);
            var lblLeft = CreateCardTitle("👥 KHÁCH HÀNG");
            lblLeft.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(lblLeft);

            var pnlCustSearch = new Panel { Dock = DockStyle.Top, Height = 45 };
            var txtCustSearch = new TextBox { Width = 200, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 10) };
            var btnCustSearch = CreatePremiumButton("Tìm Kiếm", Color.FromArgb(59, 130, 246), Color.White);
            btnCustSearch.Location = new Point(210, 8);
            btnCustSearch.Width = 80;
            btnCustSearch.Click += (s, ev) => LoadCustomersGrid(txtCustSearch.Text);
            
            var btnCustNew = CreatePremiumButton("Mới", Color.FromArgb(100, 116, 139), Color.White);
            btnCustNew.Location = new Point(298, 8);
            btnCustNew.Width = 60;
            btnCustNew.Click += (s, ev) => ResetCustFields();

            pnlCustSearch.Controls.Add(txtCustSearch);
            pnlCustSearch.Controls.Add(btnCustSearch);
            pnlCustSearch.Controls.Add(btnCustNew);
            pnlLeft.Controls.Add(pnlCustSearch);

            _gridCustomers = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridCustomers);
            _gridCustomers.BringToFront();
            _gridCustomers.Columns.Add("CustomerId", "ID");
            _gridCustomers.Columns["CustomerId"].Visible = false;
            _gridCustomers.Columns.Add("CustomerName", "Họ Tên");
            _gridCustomers.Columns.Add("Phone", "Điện Thoại");
            _gridCustomers.Columns.Add("Email", "Email");
            
            _gridCustomers.SelectionChanged += (s, ev) => {
                if (_gridCustomers.SelectedRows.Count > 0) {
                    var r = _gridCustomers.SelectedRows[0];
                    _selectedCustId = Convert.ToInt32(r.Cells["CustomerId"].Value);
                    _txtCustName.Text = r.Cells["CustomerName"].Value?.ToString() ?? "";
                    _txtCustPhone.Text = r.Cells["Phone"].Value?.ToString() ?? "";
                    _txtCustEmail.Text = r.Cells["Email"].Value?.ToString() ?? "";
                    var cust = _productService.GetCustomers().FirstOrDefault(c => c.CustomerId == _selectedCustId);
                    _txtCustAddress.Text = cust?.Address ?? "";
                }
            };

            // Customer Form below grid
            var pnlCustForm = new Panel { Dock = DockStyle.Bottom, Height = 190, Padding = new Padding(0, 10, 0, 0) };
            var custFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            
            _txtCustName = AddEditorField(custFlow, "Họ Tên *:");
            _txtCustPhone = AddEditorField(custFlow, "Điện Thoại *:");
            _txtCustEmail = AddEditorField(custFlow, "Email:");
            _txtCustAddress = AddEditorField(custFlow, "Địa Chỉ:");

            // Size adjusting
            foreach (Control ctrl in custFlow.Controls) { ctrl.Width = 195; if (ctrl.Controls.Count > 1) ctrl.Controls[1].Width = 180; }

            var btnCustSave = CreatePremiumButton("Lưu Khách Hàng", Color.FromArgb(16, 185, 129), Color.White);
            btnCustSave.Width = 160;
            btnCustSave.Margin = new Padding(0, 10, 0, 0);
            btnCustSave.Click += (s, ev) => SaveCustomer();
            custFlow.Controls.Add(btnCustSave);

            pnlCustForm.Controls.Add(custFlow);
            pnlLeft.Controls.Add(pnlCustForm);

            // Right Column: Suppliers
            var pnlRight = CreatePremiumPanel(Color.White, 15);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5);
            var lblRight = CreateCardTitle("🏢 NHÀ CUNG CẤP");
            lblRight.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblRight);

            var pnlSuppSearch = new Panel { Dock = DockStyle.Top, Height = 45 };
            var txtSuppSearch = new TextBox { Width = 200, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(0, 10) };
            var btnSuppSearch = CreatePremiumButton("Tìm Kiếm", Color.FromArgb(59, 130, 246), Color.White);
            btnSuppSearch.Location = new Point(210, 8);
            btnSuppSearch.Width = 80;
            btnSuppSearch.Click += (s, ev) => LoadSuppliersGrid(txtSuppSearch.Text);
            
            var btnSuppNew = CreatePremiumButton("Mới", Color.FromArgb(100, 116, 139), Color.White);
            btnSuppNew.Location = new Point(298, 8);
            btnSuppNew.Width = 60;
            btnSuppNew.Click += (s, ev) => ResetSuppFields();

            pnlSuppSearch.Controls.Add(txtSuppSearch);
            pnlSuppSearch.Controls.Add(btnSuppSearch);
            pnlSuppSearch.Controls.Add(btnSuppNew);
            pnlRight.Controls.Add(pnlSuppSearch);

            _gridSuppliers = CreatePremiumGrid();
            pnlRight.Controls.Add(_gridSuppliers);
            _gridSuppliers.BringToFront();
            _gridSuppliers.Columns.Add("SupplierId", "ID");
            _gridSuppliers.Columns["SupplierId"].Visible = false;
            _gridSuppliers.Columns.Add("SupplierName", "Tên Nhà Cung Cấp");
            _gridSuppliers.Columns.Add("Phone", "Điện Thoại");
            _gridSuppliers.Columns.Add("Email", "Email");

            _gridSuppliers.SelectionChanged += (s, ev) => {
                if (_gridSuppliers.SelectedRows.Count > 0) {
                    var r = _gridSuppliers.SelectedRows[0];
                    _selectedSuppId = Convert.ToInt32(r.Cells["SupplierId"].Value);
                    _txtSuppName.Text = r.Cells["SupplierName"].Value?.ToString() ?? "";
                    _txtSuppPhone.Text = r.Cells["Phone"].Value?.ToString() ?? "";
                    _txtSuppEmail.Text = r.Cells["Email"].Value?.ToString() ?? "";
                    var supp = _productService.GetSuppliers().FirstOrDefault(sp => sp.SupplierId == _selectedSuppId);
                    _txtSuppAddress.Text = supp?.Address ?? "";
                }
            };

            // Supplier Form below grid
            var pnlSuppForm = new Panel { Dock = DockStyle.Bottom, Height = 190, Padding = new Padding(0, 10, 0, 0) };
            var suppFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };

            _txtSuppName = AddEditorField(suppFlow, "Tên NCC *:");
            _txtSuppPhone = AddEditorField(suppFlow, "Điện Thoại *:");
            _txtSuppEmail = AddEditorField(suppFlow, "Email:");
            _txtSuppAddress = AddEditorField(suppFlow, "Địa Chỉ:");

            foreach (Control ctrl in suppFlow.Controls) { ctrl.Width = 195; if (ctrl.Controls.Count > 1) ctrl.Controls[1].Width = 180; }

            var btnSuppSave = CreatePremiumButton("Lưu NCC", Color.FromArgb(16, 185, 129), Color.White);
            btnSuppSave.Width = 160;
            btnSuppSave.Margin = new Padding(0, 10, 0, 0);
            btnSuppSave.Click += (s, ev) => SaveSupplier();
            suppFlow.Controls.Add(btnSuppSave);

            pnlSuppForm.Controls.Add(suppFlow);
            pnlRight.Controls.Add(pnlSuppForm);

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(pnlRight, 1, 0);

            panelMainContainer.Controls.Add(mainLayout);

            LoadCustomersGrid();
            LoadSuppliersGrid();
        }

        private void LoadCustomersGrid(string search = null)
        {
            _gridCustomers.Rows.Clear();
            var list = _productService.GetCustomers(search);
            foreach (var c in list)
            {
                _gridCustomers.Rows.Add(c.CustomerId, c.CustomerName, c.Phone, c.Email);
            }
        }

        private void ResetCustFields()
        {
            _selectedCustId = 0;
            _txtCustName.Clear();
            _txtCustPhone.Clear();
            _txtCustEmail.Clear();
            _txtCustAddress.Clear();
        }

        private void SaveCustomer()
        {
            if (string.IsNullOrWhiteSpace(_txtCustName.Text) || string.IsNullOrWhiteSpace(_txtCustPhone.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ Tên và Số điện thoại.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new CustomerDTO
            {
                CustomerId = _selectedCustId,
                CustomerName = _txtCustName.Text.Trim(),
                Phone = _txtCustPhone.Text.Trim(),
                Email = _txtCustEmail.Text.Trim(),
                Address = _txtCustAddress.Text.Trim()
            };

            bool success;
            if (_selectedCustId == 0)
                success = _productService.CreateCustomer(dto);
            else
                success = _productService.UpdateCustomer(dto);

            if (success)
            {
                MessageBox.Show("Lưu thông tin khách hàng thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCustomersGrid();
                ResetCustFields();
            }
            else
            {
                MessageBox.Show("Không thể lưu. Có thể Số điện thoại đã đăng ký cho khách hàng khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSuppliersGrid(string search = null)
        {
            _gridSuppliers.Rows.Clear();
            var list = _productService.GetSuppliers(search);
            foreach (var s in list)
            {
                _gridSuppliers.Rows.Add(s.SupplierId, s.SupplierName, s.Phone, s.Email);
            }
        }

        private void ResetSuppFields()
        {
            _selectedSuppId = 0;
            _txtSuppName.Clear();
            _txtSuppPhone.Clear();
            _txtSuppEmail.Clear();
            _txtSuppAddress.Clear();
        }

        private void SaveSupplier()
        {
            if (string.IsNullOrWhiteSpace(_txtSuppName.Text) || string.IsNullOrWhiteSpace(_txtSuppPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên và số điện thoại nhà cung cấp.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new SupplierDTO
            {
                SupplierId = _selectedSuppId,
                SupplierName = _txtSuppName.Text.Trim(),
                Phone = _txtSuppPhone.Text.Trim(),
                Email = _txtSuppEmail.Text.Trim(),
                Address = _txtSuppAddress.Text.Trim()
            };

            bool success;
            if (_selectedSuppId == 0)
                success = _productService.CreateSupplier(dto);
            else
                success = _productService.UpdateSupplier(dto);

            if (success)
            {
                MessageBox.Show("Lưu thông tin nhà cung cấp thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadSuppliersGrid();
                ResetSuppFields();
            }
            else
            {
                MessageBox.Show("Không thể lưu nhà cung cấp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ----------------------------------------------------
        // VIEW: STAFF MANAGEMENT (ADMIN ONLY)
        // ----------------------------------------------------
        private DataGridView _gridStaff;
        private TextBox _txtStaffUser, _txtStaffPwd, _txtStaffName, _txtStaffPhone, _txtStaffEmail;
        private ComboBox _cbStaffRole;
        private CheckBox _chkStaffActive;
        private int _selectedStaffId = 0;

        private void ShowStaffView()
        {
            ClearContainer();

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var pnlLeft = CreatePremiumPanel(Color.White, 0);
            pnlLeft.Dock = DockStyle.Fill;

            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(241, 245, 249) };
            var lblTitle = CreateCardTitle("👨‍💼 DANH SÁCH NHÂN VIÊN");
            lblTitle.Location = new Point(15, 18);
            var btnAddStaff = CreatePremiumButton("+ Thêm Nhân Viên", Color.FromArgb(16, 185, 129), Color.White);
            btnAddStaff.Location = new Point(320, 15);
            btnAddStaff.Width = 150;
            btnAddStaff.Click += (s, ev) => {
                ResetStaffEditor();
                _selectedStaffId = 0;
                _txtStaffUser.ReadOnly = false;
                _txtStaffPwd.Enabled = true;
            };

            pnlSearch.Controls.Add(lblTitle);
            pnlSearch.Controls.Add(btnAddStaff);
            pnlLeft.Controls.Add(pnlSearch);

            _gridStaff = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridStaff);
            _gridStaff.BringToFront();

            _gridStaff.Columns.Add("UserId", "ID");
            _gridStaff.Columns["UserId"].Visible = false;
            _gridStaff.Columns.Add("Username", "Tên Đăng Nhập");
            _gridStaff.Columns.Add("FullName", "Họ & Tên");
            _gridStaff.Columns.Add("Phone", "Điện Thoại");
            _gridStaff.Columns.Add("Email", "Email");
            _gridStaff.Columns.Add("RoleName", "Vai Trò");
            _gridStaff.Columns.Add("Status", "Trạng Thái");

            // Editor Panel (Right Side)
            var pnlRight = CreatePremiumPanel(Color.White, 20);
            pnlRight.Dock = DockStyle.Fill;
            var lblEdTitle = CreateCardTitle("📝 THÔNG TIN TÀI KHOẢN");
            lblEdTitle.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblEdTitle);

            var editorFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 10, 0, 0) };

            _txtStaffUser = AddEditorField(editorFlow, "Tên Đăng Nhập *:");
            _txtStaffPwd = AddEditorField(editorFlow, "Mật Khẩu *:");
            _txtStaffPwd.UseSystemPasswordChar = true;

            var chkShowStaffPwd = new CheckBox { Text = "Hiện mật khẩu", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI Semibold", 8.5F), Margin = new Padding(0, 5, 0, 5) };
            chkShowStaffPwd.CheckedChanged += (s, ev) => {
                _txtStaffPwd.UseSystemPasswordChar = !chkShowStaffPwd.Checked;
            };
            editorFlow.Controls.Add(chkShowStaffPwd);

            _txtStaffName = AddEditorField(editorFlow, "Họ Tên *:");
            _txtStaffPhone = AddEditorField(editorFlow, "Số Điện Thoại:");
            _txtStaffEmail = AddEditorField(editorFlow, "Email:");
            
            _cbStaffRole = AddEditorCombo(editorFlow, "Quyền Hạn *:");

            _chkStaffActive = new CheckBox { Text = "Tài khoản đang hoạt động", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold), Margin = new Padding(0, 10, 0, 10), Checked = true };
            editorFlow.Controls.Add(_chkStaffActive);

            var pnlEditButtons = new Panel { Height = 50, Width = 300 };
            var btnSave = CreatePremiumButton("Lưu Lại", Color.FromArgb(16, 185, 129), Color.White);
            btnSave.Width = 100;
            btnSave.Location = new Point(0, 5);
            btnSave.Click += (s, ev) => SaveStaff();

            var btnCancel = CreatePremiumButton("Hủy Bỏ", Color.FromArgb(100, 116, 139), Color.White);
            btnCancel.Width = 100;
            btnCancel.Location = new Point(110, 5);
            btnCancel.Click += (s, ev) => ResetStaffEditor();

            pnlEditButtons.Controls.Add(btnSave);
            pnlEditButtons.Controls.Add(btnCancel);
            editorFlow.Controls.Add(pnlEditButtons);

            pnlRight.Controls.Add(editorFlow);
            editorFlow.BringToFront();

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(pnlRight, 1, 0);
            panelMainContainer.Controls.Add(mainLayout);

            // Populate Roles Combobox
            var roles = _authService.GetRoles();
            _cbStaffRole.DataSource = roles;
            _cbStaffRole.DisplayMember = "RoleName";
            _cbStaffRole.ValueMember = "RoleId";

            LoadStaffGrid();

            _gridStaff.SelectionChanged += (s, ev) => {
                if (_gridStaff.SelectedRows.Count > 0)
                {
                    var row = _gridStaff.SelectedRows[0];
                    int userId = Convert.ToInt32(row.Cells["UserId"].Value);
                    LoadStaffToEditor(userId);
                }
            };
        }

        private void LoadStaffGrid()
        {
            _gridStaff.Rows.Clear();
            var list = _authService.GetAllUsers();
            foreach (var u in list)
            {
                _gridStaff.Rows.Add(
                    u.UserId,
                    u.Username,
                    u.FullName,
                    u.Phone,
                    u.Email,
                    TranslateRole(u.RoleName),
                    u.IsActive ? "Đang hoạt động" : "Bị Khóa"
                );
            }
        }

        private void ResetStaffEditor()
        {
            _selectedStaffId = 0;
            _txtStaffUser.Clear();
            _txtStaffUser.ReadOnly = false;
            _txtStaffPwd.Clear();
            _txtStaffPwd.Enabled = true;
            _txtStaffName.Clear();
            _txtStaffPhone.Clear();
            _txtStaffEmail.Clear();
            _chkStaffActive.Checked = true;
        }

        private void LoadStaffToEditor(int userId)
        {
            var u = _authService.GetAllUsers().FirstOrDefault(x => x.UserId == userId);
            if (u == null) return;

            _selectedStaffId = u.UserId;
            _txtStaffUser.Text = u.Username;
            _txtStaffUser.ReadOnly = true; // Username is immutable
            _txtStaffPwd.Text = "********";
            _txtStaffPwd.Enabled = true; // Enabled for editing
            _txtStaffName.Text = u.FullName;
            _txtStaffPhone.Text = u.Phone;
            _txtStaffEmail.Text = u.Email;
            _cbStaffRole.SelectedValue = u.RoleId;
            _chkStaffActive.Checked = u.IsActive;
        }

        private void SaveStaff()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_txtStaffUser.Text) || string.IsNullOrWhiteSpace(_txtStaffName.Text))
                {
                    MessageBox.Show("Vui lòng điền các thông tin bắt buộc (*).", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success;
                if (_selectedStaffId == 0)
                {
                    if (string.IsNullOrWhiteSpace(_txtStaffPwd.Text))
                    {
                        MessageBox.Show("Mật khẩu không được để trống khi tạo mới nhân viên.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    success = _authService.CreateUser(
                        _txtStaffUser.Text.Trim(),
                        _txtStaffPwd.Text,
                        _txtStaffName.Text.Trim(),
                        _txtStaffPhone.Text.Trim(),
                        _txtStaffEmail.Text.Trim(),
                        (int)_cbStaffRole.SelectedValue
                    );
                }
                else
                {
                    success = _authService.UpdateUser(
                        _selectedStaffId,
                        _txtStaffName.Text.Trim(),
                        _txtStaffPhone.Text.Trim(),
                        _txtStaffEmail.Text.Trim(),
                        (int)_cbStaffRole.SelectedValue,
                        _chkStaffActive.Checked,
                        _txtStaffPwd.Text
                    );
                }

                if (success)
                {
                    MessageBox.Show("Lưu thông tin nhân viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStaffGrid();
                    ResetStaffEditor();
                }
                else
                {
                    MessageBox.Show("Không thể lưu nhân viên. Tên đăng nhập có thể đã bị trùng lặp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ----------------------------------------------------
        // VIEW: WAREHOUSE IMPORT
        // ----------------------------------------------------
        private DataGridView _gridImportCart, _gridImportHistory;
        private ComboBox _cbImportSupplier, _cbImportProduct;
        private TextBox _txtImportQty, _txtImportPrice, _txtImportSerials;
        private Label _lblImportTotal;
        private List<ImportReceiptDetailDTO> _importCart = new List<ImportReceiptDetailDTO>();

        private void ShowImportView()
        {
            ClearContainer();
            _importCart.Clear();

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Left: Creation Form
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Right: History Grid

            // Left Side: Create Receipt Panel
            var pnlLeft = CreatePremiumPanel(Color.White, 15);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(5);
            var lblLeft = CreateCardTitle("📦 NHẬP HÀNG VÀO KHO");
            lblLeft.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(lblLeft);

            var formPanel = new Panel { Dock = DockStyle.Top, Height = 220 };
            
            // Supplier selector
            var lblSupp = new Label { Text = "Nhà Cung Cấp *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _cbImportSupplier = new ComboBox { Width = 250, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), FlatStyle = FlatStyle.Flat, Location = new Point(10, 30) };
            formPanel.Controls.Add(lblSupp);
            formPanel.Controls.Add(_cbImportSupplier);

            // Product selector
            var lblProd = new Label { Text = "Sản phẩm Laptop *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(10, 70), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _cbImportProduct = new ComboBox { Width = 250, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), FlatStyle = FlatStyle.Flat, Location = new Point(10, 90) };
            formPanel.Controls.Add(lblProd);
            formPanel.Controls.Add(_cbImportProduct);

            // Fill product standard price on select
            _cbImportProduct.SelectedIndexChanged += (s, ev) => {
                if (_cbImportProduct.SelectedValue is int prodId)
                {
                    var p = _productService.GetProductById(prodId);
                    if (p != null)
                    {
                        _txtImportPrice.Text = p.ImportPrice.ToString("F0");
                    }
                }
            };

            // Qty
            var lblQty = new Label { Text = "Số lượng *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(280, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _txtImportQty = new TextBox { Width = 80, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(280, 30), Text = "1" };
            formPanel.Controls.Add(lblQty);
            formPanel.Controls.Add(_txtImportQty);

            // Price
            var lblPrice = new Label { Text = "Giá Nhập (VNĐ) *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(380, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _txtImportPrice = new TextBox { Width = 150, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(380, 30) };
            formPanel.Controls.Add(lblPrice);
            formPanel.Controls.Add(_txtImportPrice);

            // Serials Multi-line textbox
            var lblSerials = new Label { Text = "Nhập danh sách Serial (Mỗi serial một dòng, hoặc cách nhau bởi dấu phẩy) *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(280, 70), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _txtImportSerials = new TextBox { Width = 250, Height = 100, Multiline = true, Font = new Font("Consolas", 9.5F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(280, 90), ScrollBars = ScrollBars.Vertical };
            formPanel.Controls.Add(lblSerials);
            formPanel.Controls.Add(_txtImportSerials);

            var btnAddCart = CreatePremiumButton("Thêm Vào Lô Nhập", Color.FromArgb(245, 158, 11), Color.White);
            btnAddCart.Location = new Point(10, 140);
            btnAddCart.Width = 250;
            btnAddCart.Height = 40;
            btnAddCart.Click += (s, ev) => AddToImportCart();
            formPanel.Controls.Add(btnAddCart);

            pnlLeft.Controls.Add(formPanel);

            // Import Cart Grid in the middle of left panel
            _gridImportCart = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridImportCart);
            _gridImportCart.BringToFront();
            _gridImportCart.Columns.Add("ProductId", "ID");
            _gridImportCart.Columns["ProductId"].Visible = false;
            _gridImportCart.Columns.Add("ProductName", "Tên Laptop");
            _gridImportCart.Columns.Add("Quantity", "Số lượng");
            _gridImportCart.Columns.Add("ImportPrice", "Giá Nhập");
            _gridImportCart.Columns.Add("Total", "Tổng Tiền");
            _gridImportCart.Columns.Add("Serials", "Số Serials");

            // Checkout Panel on bottom of left panel
            var pnlCheckout = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(241, 245, 249) };
            _lblImportTotal = new Label { Text = "TỔNG TIỀN NHẬP KHO: 0 đ", ForeColor = Color.FromArgb(245, 158, 11), Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(15, 18) };
            var btnConfirmImport = CreatePremiumButton("Xác Nhận Nhập Kho", Color.FromArgb(16, 185, 129), Color.White);
            btnConfirmImport.Location = new Point(320, 12);
            btnConfirmImport.Width = 200;
            btnConfirmImport.Height = 35;
            btnConfirmImport.Click += (s, ev) => ConfirmImport();

            pnlCheckout.Controls.Add(_lblImportTotal);
            pnlCheckout.Controls.Add(btnConfirmImport);
            pnlLeft.Controls.Add(pnlCheckout);

            // Right Side: History Panel
            var pnlRight = CreatePremiumPanel(Color.White, 15);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5);
            var lblRight = CreateCardTitle("📜 LỊCH SỬ NHẬP KHO");
            lblRight.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblRight);

            var pnlImportHistorySearch = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(241, 245, 249) };
            var txtImportHistorySearch = new TextBox { Width = 150, Location = new Point(10, 12), Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            var btnImportHistorySearch = CreatePremiumButton("Tìm Kiếm", Color.FromArgb(0, 82, 204), Color.White);
            btnImportHistorySearch.Location = new Point(170, 8); btnImportHistorySearch.Width = 75; btnImportHistorySearch.Height = 28;
            btnImportHistorySearch.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            
            var btnImportHistoryAll = CreatePremiumButton("Tất Cả", Color.FromArgb(100, 116, 139), Color.White);
            btnImportHistoryAll.Location = new Point(252, 8); btnImportHistoryAll.Width = 65; btnImportHistoryAll.Height = 28;
            btnImportHistoryAll.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);

            pnlImportHistorySearch.Controls.Add(txtImportHistorySearch);
            pnlImportHistorySearch.Controls.Add(btnImportHistorySearch);
            pnlImportHistorySearch.Controls.Add(btnImportHistoryAll);
            pnlRight.Controls.Add(pnlImportHistorySearch);

            _gridImportHistory = CreatePremiumGrid();
            pnlRight.Controls.Add(_gridImportHistory);
            _gridImportHistory.BringToFront();
            _gridImportHistory.Columns.Add("ImportReceiptId", "Mã Phiếu");
            _gridImportHistory.Columns.Add("SupplierName", "Nhà Cung Cấp");
            _gridImportHistory.Columns.Add("TotalAmount", "Tổng Tiền");
            _gridImportHistory.Columns.Add("ImportDate", "Ngày Nhập");

            _gridImportHistory.SelectionChanged += (s, ev) => {
                if (_gridImportHistory.SelectedRows.Count > 0)
                {
                    int receiptId = Convert.ToInt32(_gridImportHistory.SelectedRows[0].Cells["ImportReceiptId"].Value);
                    ShowImportReceiptDetailPopup(receiptId);
                }
            };

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(pnlRight, 1, 0);
            panelMainContainer.Controls.Add(mainLayout);

            // Populate Dropdowns
            _cbImportSupplier.DataSource = _productService.GetSuppliers();
            _cbImportSupplier.DisplayMember = "SupplierName";
            _cbImportSupplier.ValueMember = "SupplierId";

            _cbImportProduct.DataSource = _productService.GetProducts(null, null, null, true);
            _cbImportProduct.DisplayMember = "ProductName";
            _cbImportProduct.ValueMember = "ProductId";

            btnImportHistorySearch.Click += (s, ev) => LoadImportHistoryGrid(txtImportHistorySearch.Text.Trim());
            btnImportHistoryAll.Click += (s, ev) => LoadImportHistoryGrid(null);

            // Do not call LoadImportHistoryGrid() initially to leave it blank as requested
        }

        private void LoadImportHistoryGrid(string search = null)
        {
            _gridImportHistory.Rows.Clear();
            var list = _warehouseService.GetAllImportReceipts(search);
            foreach (var ir in list)
            {
                _gridImportHistory.Rows.Add(
                    ir.ImportReceiptId,
                    ir.SupplierName,
                    ir.TotalAmount.ToString("N0") + " đ",
                    ir.ImportDate.ToString("dd/MM/yyyy HH:mm")
                );
            }
        }

        private void AddToImportCart()
        {
            if (_cbImportProduct.SelectedValue == null) return;
            int prodId = (int)_cbImportProduct.SelectedValue;
            string prodName = _cbImportProduct.Text;

            if (!int.TryParse(_txtImportQty.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Số lượng nhập phải lớn hơn 0.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(_txtImportPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Giá nhập phải lớn hơn 0.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse serial numbers
            var serials = _txtImportSerials.Text
                .Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (serials.Count != qty)
            {
                MessageBox.Show($"Số lượng serial đã nhập ({serials.Count}) không trùng khớp với số lượng nhập ở trên ({qty}).", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check duplicate serials in current cart
            foreach (var serial in serials)
            {
                if (_importCart.Any(c => c.SerialNumbers.Contains(serial)))
                {
                    MessageBox.Show($"Số Serial '{serial}' đã có sẵn trong giỏ hàng nhập.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Add to Cart
            var detail = new ImportReceiptDetailDTO
            {
                ProductId = prodId,
                ProductName = prodName,
                Quantity = qty,
                ImportPrice = price,
                SerialNumbers = serials
            };

            _importCart.Add(detail);
            
            // Reload grid
            ReloadImportCartGrid();

            // Reset field
            _txtImportQty.Text = "1";
            _txtImportSerials.Clear();
        }

        private void ReloadImportCartGrid()
        {
            _gridImportCart.Rows.Clear();
            decimal total = 0;
            foreach (var item in _importCart)
            {
                decimal rowTotal = item.Quantity * item.ImportPrice;
                total += rowTotal;
                _gridImportCart.Rows.Add(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.ImportPrice.ToString("N0") + " đ",
                    rowTotal.ToString("N0") + " đ",
                    string.Join(", ", item.SerialNumbers)
                );
            }
            _lblImportTotal.Text = $"TỔNG TIỀN NHẬP KHO: {total.ToString("N0")} đ";
        }

        private void ConfirmImport()
        {
            if (_cbImportSupplier.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_importCart.Any())
            {
                MessageBox.Show("Giỏ hàng nhập trống. Vui lòng thêm sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int supplierId = (int)_cbImportSupplier.SelectedValue;
            int createdBy = SessionHelper.CurrentUserId;

            try
            {
                bool success = _warehouseService.CreateImportReceipt(supplierId, createdBy, _importCart);
                if (success)
                {
                    MessageBox.Show("Nhập kho thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _importCart.Clear();
                    ReloadImportCartGrid();
                    LoadImportHistoryGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi nhập kho", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowImportReceiptDetailPopup(int receiptId)
        {
            var details = _warehouseService.GetImportReceiptDetails(receiptId);
            if (details == null) return;

            var popup = new Form
            {
                Text = $"Chi tiết phiếu nhập #{receiptId}",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            var grid = CreatePremiumGrid();
            grid.Dock = DockStyle.Fill;
            popup.Controls.Add(grid);

            grid.Columns.Add("ProductName", "Tên Laptop");
            grid.Columns.Add("Quantity", "Số lượng");
            grid.Columns.Add("ImportPrice", "Giá Nhập");
            grid.Columns.Add("Total", "Thành Tiền");
            grid.Columns.Add("Serials", "Danh Sách Serials");

            foreach (var d in details)
            {
                grid.Rows.Add(
                    d.ProductName,
                    d.Quantity,
                    d.ImportPrice.ToString("N0") + " đ",
                    (d.Quantity * d.ImportPrice).ToString("N0") + " đ",
                    string.Join(", ", d.SerialNumbers)
                );
            }

            popup.ShowDialog();
        }

        // ----------------------------------------------------
        // VIEW: SALES CHECKOUT (CASHIER)
        // ----------------------------------------------------
        private DataGridView _gridSalesCart, _gridSalesHistory;
        private TextBox _txtSalesCustPhone, _txtSalesCustName, _txtSalesDiscount, _txtSalesCustEmail, _txtSalesCustAddress;
        private Label _lblSalesSubTotal, _lblSalesFinalTotal;
        private List<OrderDetailDTO> _salesCart = new List<OrderDetailDTO>();
        private CustomerDTO _salesSelectedCustomer = null;

        private void ShowSalesView()
        {
            ClearContainer();
            _salesCart.Clear();
            _salesSelectedCustomer = null;

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Left: Cart & Customer
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Right: Order History

            // Left Panel
            var pnlLeft = CreatePremiumPanel(Color.White, 15);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(5);
            var lblLeft = CreateCardTitle("🛒 LẬP HÓA ĐƠN BÁN HÀNG");
            lblLeft.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(lblLeft);

            // ── Khu vực nhập thông tin khách hàng dùng TableLayoutPanel để tự co giãn ──
            // Lý do: tọa độ tuyệt đối gây tràn khi pnlLeft hẹp (ví dụ tên user dài trong header)
            var pnlCust = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(8) };

            // 4 cột bằng nhau, mỗi cột chứa label + textbox
            var tlpCust = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            for (int ci = 0; ci < 4; ci++)
                tlpCust.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpCust.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F)); // labels
            tlpCust.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // textboxes

            var lblPhone = new Label { Text = "SĐT Khách Hàng:", ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), Dock = DockStyle.Fill };
            _txtSalesCustPhone = new TextBox { Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };

            var lblCustName = new Label { Text = "Tên Khách Hàng *:", ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), Dock = DockStyle.Fill };
            _txtSalesCustName = new TextBox { Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };

            var lblCustEmail = new Label { Text = "Email:", ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), Dock = DockStyle.Fill };
            _txtSalesCustEmail = new TextBox { Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };

            var lblCustAddress = new Label { Text = "Địa Chỉ:", ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold), Dock = DockStyle.Fill };
            _txtSalesCustAddress = new TextBox { Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };

            tlpCust.Controls.Add(lblPhone, 0, 0);
            tlpCust.Controls.Add(_txtSalesCustPhone, 0, 1);
            tlpCust.Controls.Add(lblCustName, 1, 0);
            tlpCust.Controls.Add(_txtSalesCustName, 1, 1);
            tlpCust.Controls.Add(lblCustEmail, 2, 0);
            tlpCust.Controls.Add(_txtSalesCustEmail, 2, 1);
            tlpCust.Controls.Add(lblCustAddress, 3, 0);
            tlpCust.Controls.Add(_txtSalesCustAddress, 3, 1);
            pnlCust.Controls.Add(tlpCust);

            // Tìm kiếm khách hàng khi nhấn Enter trong ô SĐT (thay thế nút TÌM)
            Action lookupCustomer = () => {
                string phone = _txtSalesCustPhone.Text.Trim();
                if (string.IsNullOrEmpty(phone)) return;
                var cust = _productService.GetCustomerByPhone(phone);
                if (cust != null)
                {
                    _salesSelectedCustomer = cust;
                    _txtSalesCustName.Text = cust.CustomerName;
                    _txtSalesCustEmail.Text = cust.Email;
                    _txtSalesCustAddress.Text = cust.Address;
                    _txtSalesCustName.ReadOnly = true;
                    _txtSalesCustEmail.ReadOnly = true;
                    _txtSalesCustAddress.ReadOnly = true;
                    MessageBox.Show($"Đã tìm thấy khách hàng thành viên: {cust.CustomerName}!", "Tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _salesSelectedCustomer = null;
                    _txtSalesCustName.Clear();
                    _txtSalesCustEmail.Clear();
                    _txtSalesCustAddress.Clear();
                    _txtSalesCustName.ReadOnly = false;
                    _txtSalesCustEmail.ReadOnly = false;
                    _txtSalesCustAddress.ReadOnly = false;
                    MessageBox.Show("Không tìm thấy khách hàng. Vui lòng nhập Tên để hệ thống tự động đăng ký mới!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            _txtSalesCustPhone.KeyDown += (s, ev) => { if (ev.KeyCode == Keys.Return) { ev.SuppressKeyPress = true; lookupCustomer(); } };
            _txtSalesCustPhone.Leave += (s, ev) => { if (!string.IsNullOrWhiteSpace(_txtSalesCustPhone.Text)) lookupCustomer(); };

            pnlLeft.Controls.Add(pnlCust);

            // Add Product Panel — tăng chiều cao để tạo khoảng cách với phần khách hàng bên dưới
            var pnlAddProd = new Panel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(0, 0, 0, 8) };
            var lblSelProd = new Label { Text = "Chọn Laptop:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(10, 8), AutoSize = true };
            var cbProd = new ComboBox { Width = 250, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), FlatStyle = FlatStyle.Flat, Location = new Point(10, 26) };
            var btnAddCart = CreatePremiumButton("+ Thêm Vào Giỏ Hàng", Color.FromArgb(245, 158, 11), Color.White);
            btnAddCart.Location = new Point(270, 13);
            btnAddCart.Width = 175;

            cbProd.DataSource = _productService.GetProducts(null, null, null, true);
            cbProd.DisplayMember = "ProductName";
            cbProd.ValueMember = "ProductId";

            btnAddCart.Click += (s, ev) => {
                if (cbProd.SelectedValue == null) return;
                int prodId = (int)cbProd.SelectedValue;
                
                if (_salesCart.Any(c => c.ProductId == prodId))
                {
                    MessageBox.Show("Laptop này đã được thêm vào giỏ hàng. Hãy tăng số lượng trong bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var p = _productService.GetProductById(prodId);
                if (p == null) return;
                if (p.QuantityInStock <= 0)
                {
                    MessageBox.Show("Sản phẩm đã hết hàng tồn kho vật lý.", "Hết hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var cartItem = new OrderDetailDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Quantity = 1,
                    UnitPrice = p.SalePrice,
                    SerialNumbers = new List<string>()
                };
                _salesCart.Add(cartItem);
                ReloadSalesCartGrid();
            };

            pnlAddProd.Controls.Add(lblSelProd);
            pnlAddProd.Controls.Add(cbProd);
            pnlAddProd.Controls.Add(btnAddCart);
            pnlLeft.Controls.Add(pnlAddProd);

            // Container Panel for Cart Grid & Actions to prevent overlapping
            var pnlCartContainer = new Panel { Dock = DockStyle.Fill };

            // ── Nút điều chỉnh số lượng & xóa dòng giỏ hàng ──
            // Tăng chiều cao panel và thêm Padding dưới để tạo khoảng cách với DataGridView
            var pnlSalesCartActions = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10, 6, 10, 8) };
            var btnIncreaseQty = CreatePremiumButton("+ SL", Color.FromArgb(59, 130, 246), Color.White);
            btnIncreaseQty.Location = new Point(10, 6); btnIncreaseQty.Width = 65;
            btnIncreaseQty.Click += (s, ev) => AdjustCartRowQty(1);

            var btnDecreaseQty = CreatePremiumButton("- SL", Color.FromArgb(59, 130, 246), Color.White);
            btnDecreaseQty.Location = new Point(82, 6); btnDecreaseQty.Width = 65;
            btnDecreaseQty.Click += (s, ev) => AdjustCartRowQty(-1);

            var btnRemoveItem = CreatePremiumButton("Xóa Dòng", Color.FromArgb(239, 68, 68), Color.White);
            btnRemoveItem.Location = new Point(154, 6); btnRemoveItem.Width = 110;
            btnRemoveItem.Click += (s, ev) => AdjustCartRowQty(0);

            pnlSalesCartActions.Controls.Add(btnIncreaseQty);
            pnlSalesCartActions.Controls.Add(btnDecreaseQty);
            pnlSalesCartActions.Controls.Add(btnRemoveItem);
            pnlCartContainer.Controls.Add(pnlSalesCartActions);

            var btnSelectSerials = CreatePremiumButton("Chọn Serial Cho Máy", Color.FromArgb(16, 185, 129), Color.White);
            btnSelectSerials.Dock = DockStyle.Top;
            btnSelectSerials.Height = 35;
            btnSelectSerials.Click += (s, ev) => PickSerialsForSelectedCartRow();
            pnlCartContainer.Controls.Add(btnSelectSerials);

            _gridSalesCart = CreatePremiumGrid();
            pnlCartContainer.Controls.Add(_gridSalesCart);
            _gridSalesCart.BringToFront();

            _gridSalesCart.Columns.Add("ProductId", "ID");
            _gridSalesCart.Columns["ProductId"].Visible = false;
            _gridSalesCart.Columns.Add("ProductName", "Tên Laptop");
            _gridSalesCart.Columns.Add("Quantity", "Số lượng");
            _gridSalesCart.Columns.Add("UnitPrice", "Đơn Giá");
            _gridSalesCart.Columns.Add("Total", "Thành Tiền");
            _gridSalesCart.Columns.Add("SerialsStatus", "Trạng thái Serial");

            pnlLeft.Controls.Add(pnlCartContainer);
            pnlCartContainer.BringToFront();

            // ── Footer thanh toán dùng TableLayoutPanel để tự co giãn theo chiều rộng pnlLeft ──
            // Lý do: tọa độ tuyệt đối (x=400) bị cắt khi cửa sổ hẹp hoặc tên user dài làm header co lại
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(8) };

            // Cột 0: Tạm tính + Thành tiền | Cột 1: Giảm giá | Cột 2: Nút xuất hóa đơn
            var tlpFooter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            // Cột 0 chiếm 55% để "THÀNH TIỀN" không bị cắt chữ cuối
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));  // labels
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F)); // giảm giá
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));  // nút
            tlpFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Cột 0: Tạm tính + Thành tiền xếp dọc
            var pnlTotals = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            _lblSalesSubTotal = new Label { Text = "Tạm tính: 0 đ", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 10F), AutoSize = true, Location = new Point(4, 4) };
            _lblSalesFinalTotal = new Label { Text = "THÀNH TIỀN: 0 đ", ForeColor = Color.FromArgb(245, 158, 11), Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(4, 38) };
            pnlTotals.Controls.Add(_lblSalesSubTotal);
            pnlTotals.Controls.Add(_lblSalesFinalTotal);
            tlpFooter.Controls.Add(pnlTotals, 0, 0);

            // Cột 1: Giảm giá — dịch sang phải 10px so với cột 0 để tạo khoảng cách
            var pnlDiscount = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(8, 0, 0, 0) };
            var lblDiscount = new Label { Text = "Giảm giá (đ):", ForeColor = Color.FromArgb(71, 85, 105), Font = new Font("Segoe UI", 9F), AutoSize = true, Location = new Point(8, 6) };
            _txtSalesDiscount = new TextBox { Width = 108, Font = new Font("Segoe UI", 9.5F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(8, 28), Text = "0" };
            _txtSalesDiscount.TextChanged += (s, ev) => CalculateTotals();
            pnlDiscount.Controls.Add(lblDiscount);
            pnlDiscount.Controls.Add(_txtSalesDiscount);
            tlpFooter.Controls.Add(pnlDiscount, 1, 0);

            // Cột 2: Nút Xuất Hóa Đơn — font 12F để chữ vừa với nút đã thu nhỏ
            var btnPay = CreatePremiumButton("Xuất Hóa Đơn", Color.FromArgb(16, 185, 129), Color.White);
            btnPay.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnPay.Dock = DockStyle.Fill;
            btnPay.Margin = new Padding(6, 4, 4, 4);
            btnPay.Click += (s, ev) => SubmitOrder();
            tlpFooter.Controls.Add(btnPay, 2, 0);

            pnlFooter.Controls.Add(tlpFooter);
            pnlLeft.Controls.Add(pnlFooter);

            // Right Panel: History
            var pnlRight = CreatePremiumPanel(Color.White, 15);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5);
            var lblRight = CreateCardTitle("📜 HÓA ĐƠN ĐÃ XUẤT");
            lblRight.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblRight);

            var pnlSalesHistorySearch = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(241, 245, 249) };
            var txtSalesHistorySearch = new TextBox { Width = 150, Location = new Point(10, 12), Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            var btnSalesHistorySearch = CreatePremiumButton("Tìm Kiếm", Color.FromArgb(0, 82, 204), Color.White);
            btnSalesHistorySearch.Location = new Point(170, 8); btnSalesHistorySearch.Width = 75; btnSalesHistorySearch.Height = 28;
            btnSalesHistorySearch.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            
            var btnSalesHistoryAll = CreatePremiumButton("Tất Cả", Color.FromArgb(100, 116, 139), Color.White);
            btnSalesHistoryAll.Location = new Point(252, 8); btnSalesHistoryAll.Width = 65; btnSalesHistoryAll.Height = 28;
            btnSalesHistoryAll.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);

            pnlSalesHistorySearch.Controls.Add(txtSalesHistorySearch);
            pnlSalesHistorySearch.Controls.Add(btnSalesHistorySearch);
            pnlSalesHistorySearch.Controls.Add(btnSalesHistoryAll);
            pnlRight.Controls.Add(pnlSalesHistorySearch);

            // ── Sửa cột hóa đơn đã xuất để hiển thị đủ nội dung mà không cần kéo tay ──
            _gridSalesHistory = CreatePremiumGrid();
            // Tắt AutoSizeColumnsMode Fill để gán chiều rộng cố định đủ cho từng cột
            _gridSalesHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            pnlRight.Controls.Add(_gridSalesHistory);
            _gridSalesHistory.BringToFront();
            _gridSalesHistory.Columns.Add("OrderId", "ID");
            _gridSalesHistory.Columns["OrderId"].Visible = false;
            _gridSalesHistory.Columns.Add("OrderCode", "Số HĐ");
            _gridSalesHistory.Columns["OrderCode"].Width = 90;
            _gridSalesHistory.Columns.Add("CustomerName", "Khách Hàng");
            _gridSalesHistory.Columns["CustomerName"].Width = 130;
            _gridSalesHistory.Columns.Add("FinalAmount", "Tổng Thanh Toán");
            _gridSalesHistory.Columns["FinalAmount"].Width = 120;
            _gridSalesHistory.Columns.Add("OrderDate", "Ngày Xuất");
            _gridSalesHistory.Columns["OrderDate"].Width = 130;
            // Cho phép cột cuối tự mở rộng nếu panel rộng hơn
            _gridSalesHistory.Columns["OrderDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            _gridSalesHistory.SelectionChanged += (s, ev) => {
                if (_gridSalesHistory.SelectedRows.Count > 0)
                {
                    int ordId = Convert.ToInt32(_gridSalesHistory.SelectedRows[0].Cells["OrderId"].Value);
                    ShowOrderDetailPopup(ordId);
                }
            };

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(pnlRight, 1, 0);
            panelMainContainer.Controls.Add(mainLayout);

            btnSalesHistorySearch.Click += (s, ev) => LoadSalesHistoryGrid(txtSalesHistorySearch.Text.Trim());
            btnSalesHistoryAll.Click += (s, ev) => LoadSalesHistoryGrid(null);

            // Do not call LoadSalesHistoryGrid() initially to leave it blank as requested
        }

        private void LoadSalesHistoryGrid(string search = null)
        {
            _gridSalesHistory.Rows.Clear();
            var list = _salesService.GetAllOrders(search);
            foreach (var o in list)
            {
                _gridSalesHistory.Rows.Add(
                    o.OrderId,
                    o.OrderCode,
                    o.CustomerName,
                    o.FinalAmount.ToString("N0") + " đ",
                    o.OrderDate.ToString("dd/MM/yyyy HH:mm")
                );
            }
        }

        private void AdjustCartRowQty(int delta)
        {
            if (_gridSalesCart.SelectedRows.Count == 0) return;
            int prodId = Convert.ToInt32(_gridSalesCart.SelectedRows[0].Cells["ProductId"].Value);
            var item = _salesCart.FirstOrDefault(c => c.ProductId == prodId);
            if (item == null) return;

            if (delta == 0)
            {
                _salesCart.Remove(item);
            }
            else
            {
                int newQty = item.Quantity + delta;
                if (newQty <= 0)
                {
                    _salesCart.Remove(item);
                }
                else
                {
                    var p = _productService.GetProductById(prodId);
                    if (p.QuantityInStock < newQty)
                    {
                        MessageBox.Show($"Không đủ hàng trong kho. Còn lại: {p.QuantityInStock}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    item.Quantity = newQty;
                    item.SerialNumbers.Clear(); // reset serial selection as quantity changes
                }
            }

            ReloadSalesCartGrid();
        }

        private void PickSerialsForSelectedCartRow()
        {
            if (_gridSalesCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng laptop trong giỏ hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int prodId = Convert.ToInt32(_gridSalesCart.SelectedRows[0].Cells["ProductId"].Value);
            var item = _salesCart.FirstOrDefault(c => c.ProductId == prodId);
            if (item == null) return;

            var availableItems = _warehouseService.GetProductItemsInStock(prodId);
            if (!availableItems.Any())
            {
                MessageBox.Show("Không có thiết bị laptop nào khả dụng trong kho vật lý cho sản phẩm này.", "Lỗi kho", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dlg = new Form
            {
                Text = $"Chọn đúng {item.Quantity} số Serial cho: {item.ProductName}",
                Size = new Size(450, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            var flp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(15) };
            
            var checkboxes = new List<CheckBox>();
            foreach (var pi in availableItems)
            {
                var cb = new CheckBox
                {
                    Text = pi.SerialNumber,
                    Font = new Font("Consolas", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true,
                    Margin = new Padding(5),
                    Checked = item.SerialNumbers.Contains(pi.SerialNumber)
                };
                checkboxes.Add(cb);
                flp.Controls.Add(cb);
            }
            
            var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(15, 23, 42) };
            var btnConfirm = CreatePremiumButton("Xác Nhận", Color.FromArgb(16, 185, 129), Color.White);
            btnConfirm.Location = new Point(130, 8);
            btnConfirm.Width = 180;
            btnConfirm.Click += (s, ev) => {
                var selectedSerials = checkboxes.Where(cb => cb.Checked).Select(cb => cb.Text).ToList();
                if (selectedSerials.Count != item.Quantity)
                {
                    MessageBox.Show($"Bạn phải chọn CHÍNH XÁC {item.Quantity} số Serial. (Hiện đang chọn: {selectedSerials.Count})", "Lỗi chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                item.SerialNumbers = selectedSerials;
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };

            pnlButtons.Controls.Add(btnConfirm);
            dlg.Controls.Add(flp);
            dlg.Controls.Add(pnlButtons);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ReloadSalesCartGrid();
            }
        }

        private void ReloadSalesCartGrid()
        {
            _gridSalesCart.Rows.Clear();
            foreach (var item in _salesCart)
            {
                decimal total = item.Quantity * item.UnitPrice;
                string serialStatus = item.SerialNumbers.Any() 
                    ? $"Đã chọn {item.SerialNumbers.Count} serials" 
                    : $"Chưa chọn serial (Yêu cầu: {item.Quantity})";

                _gridSalesCart.Rows.Add(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice.ToString("N0") + " đ",
                    total.ToString("N0") + " đ",
                    serialStatus
                );
            }
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            decimal subtotal = _salesCart.Sum(i => i.Quantity * i.UnitPrice);
            _lblSalesSubTotal.Text = $"Tạm tính: {subtotal.ToString("N0")} đ";

            decimal.TryParse(_txtSalesDiscount.Text, out decimal discount);
            decimal final = subtotal - discount;
            if (final < 0) final = 0;

            _lblSalesFinalTotal.Text = $"THÀNH TIỀN: {final.ToString("N0")} đ";
        }

        private void SubmitOrder()
        {
            if (string.IsNullOrWhiteSpace(_txtSalesCustPhone.Text) || string.IsNullOrWhiteSpace(_txtSalesCustName.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin Khách hàng (Tên + Số điện thoại).", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_salesCart.Any())
            {
                MessageBox.Show("Giỏ hàng đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in _salesCart)
            {
                if (item.SerialNumbers.Count != item.Quantity)
                {
                    MessageBox.Show($"Bạn chưa chọn đủ số lượng serial cho: '{item.ProductName}'. Hãy chọn đúng {item.Quantity} serial.", "Lỗi chọn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int customerId;
            if (_salesSelectedCustomer != null)
            {
                customerId = _salesSelectedCustomer.CustomerId;
            }
            else
            {
                var newCust = new CustomerDTO
                {
                    CustomerName = _txtSalesCustName.Text.Trim(),
                    Phone = _txtSalesCustPhone.Text.Trim(),
                    Email = _txtSalesCustEmail.Text.Trim(),
                    Address = _txtSalesCustAddress.Text.Trim()
                };
                if (_productService.CreateCustomer(newCust))
                {
                    var registered = _productService.GetCustomerByPhone(newCust.Phone);
                    customerId = registered.CustomerId;
                }
                else
                {
                    MessageBox.Show("Lỗi tự động đăng ký khách hàng mới. Vui lòng kiểm tra lại Số điện thoại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            decimal.TryParse(_txtSalesDiscount.Text, out decimal discount);
            int createdBy = SessionHelper.CurrentUserId;

            try
            {
                bool success = _salesService.CreateOrder(customerId, createdBy, discount, _salesCart);
                if (success)
                {
                    MessageBox.Show("Xuất hóa đơn và thanh toán thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _salesCart.Clear();
                    _salesSelectedCustomer = null;
                    _txtSalesCustPhone.Clear();
                    _txtSalesCustName.Clear();
                    _txtSalesCustEmail.Clear();
                    _txtSalesCustAddress.Clear();
                    _txtSalesCustName.ReadOnly = false;
                    _txtSalesCustEmail.ReadOnly = false;
                    _txtSalesCustAddress.ReadOnly = false;
                    _txtSalesDiscount.Text = "0";

                    ReloadSalesCartGrid();
                    LoadSalesHistoryGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi lập hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowOrderDetailPopup(int orderId)
        {
            try
            {
                var order = _salesService.GetAllOrders().FirstOrDefault(o => o.OrderId == orderId);
                if (order == null) return;
                var details = _salesService.GetOrderDetails(orderId);
                if (details == null) return;

                var customer = _unitOfWork.Customers.GetById(order.CustomerId);
                string customerAddress = customer?.Address ?? "Chưa cập nhật";

                var popup = new Form
                {
                    Text = $"Hóa đơn chi tiết #{order.OrderCode}",
                    Size = new Size(570, 780),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(241, 245, 249),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                // Scrollable container for the print area
                var pnlScroll = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(10)
                };
                popup.Controls.Add(pnlScroll);

                // Printable Area Panel
                var pnlPrintArea = new Panel
                {
                    Width = 520,
                    Height = 650,
                    BackColor = Color.White,
                    Location = new Point(10, 10)
                };
                pnlScroll.Controls.Add(pnlPrintArea);

                // Setup Border Paint for Print Area
                pnlPrintArea.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(203, 213, 225), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, pnlPrintArea.Width - 1, pnlPrintArea.Height - 1);
                    }
                };

                // Table Layout inside Print Area
                var tlpPrint = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 5,
                    Padding = new Padding(15)
                };
                tlpPrint.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                tlpPrint.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Store Header
                tlpPrint.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Invoice metadata
                tlpPrint.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Row 2: Grid items
                tlpPrint.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 3: Serials text
                tlpPrint.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 4: Totals & Footer
                pnlPrintArea.Controls.Add(tlpPrint);

                // ── Row 0: Store Header — Thông tin cửa hàng căn giữa ──
                var pnlStoreHeader = new Panel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 8, 0, 8) };
                var lblStoreName = new Label
                {
                    Text = "HỆ THỐNG CỬA HÀNG LAPTOPAZ",
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 82, 204),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 28
                };
                var lblStoreAddr1 = new Label
                {
                    Text = "LaptopAZ cơ sở 1: Số 18 ngõ 121, Thái Hà, Đống Đa, Hà Nội",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(0, 82, 204),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 22
                };
                var lblStoreAddr2 = new Label
                {
                    Text = "LaptopAZ cơ sở 2: Số 56 Trần Phú, Hà Đông, Hà Nội",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(0, 82, 204),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 22
                };
                var lblHotline = new Label
                {
                    Text = "Hotline: 0825 233 233",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 82, 204),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 22
                };
                var lblDivider = new Label
                {
                    Text = new string('─', 55),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 16
                };
                // Thêm theo thứ tự ngược (DockStyle.Top xếp từ dưới lên)
                pnlStoreHeader.Controls.Add(lblDivider);
                pnlStoreHeader.Controls.Add(lblHotline);
                pnlStoreHeader.Controls.Add(lblStoreAddr2);
                pnlStoreHeader.Controls.Add(lblStoreAddr1);
                pnlStoreHeader.Controls.Add(lblStoreName);
                tlpPrint.Controls.Add(pnlStoreHeader, 0, 0);

                // Row 1: Invoice metadata
                var lblInfo = new Label
                {
                    Text = $"HÓA ĐƠN BÁN HÀNG\n\n" +
                           $"Mã hóa đơn : {order.OrderCode}\n" +
                           $"Ngày mua   : {order.OrderDate:dd/MM/yyyy HH:mm}\n\n" +
                           $"Khách hàng : {order.CustomerName}\n" +
                           $"SĐT        : {order.CustomerPhone}\n" +
                           $"Địa chỉ    : {customerAddress}\n" +
                           $"Nhân viên  : {order.EmployeeName}",
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10, 5, 10, 10)
                };
                tlpPrint.Controls.Add(lblInfo, 0, 1);

                // Row 2: Grid items
                var grid = CreatePremiumGrid();
                grid.Height = 150;
                grid.Dock = DockStyle.Fill;
                grid.Margin = new Padding(10, 5, 10, 10);
                grid.RowTemplate.Height = 28;
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 82, 204);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                grid.EnableHeadersVisualStyles = false;

                grid.Columns.Add("ProductName", "Tên Laptop");
                grid.Columns["ProductName"].Width = 200;
                grid.Columns["ProductName"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                grid.Columns.Add("Quantity", "SL");
                grid.Columns["Quantity"].Width = 40;
                grid.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                grid.Columns.Add("UnitPrice", "Đơn Giá");
                grid.Columns["UnitPrice"].Width = 110;
                grid.Columns["UnitPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns["UnitPrice"].DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);

                grid.Columns.Add("Total", "Thành Tiền");
                grid.Columns["Total"].Width = 110;
                grid.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns["Total"].DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                grid.Columns["Total"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                foreach (var d in details)
                {
                    grid.Rows.Add(
                        d.ProductName,
                        d.Quantity,
                        d.UnitPrice.ToString("N0") + " đ",
                        (d.Quantity * d.UnitPrice).ToString("N0") + " đ"
                    );
                }
                tlpPrint.Controls.Add(grid, 0, 2);

                // Row 3: Serials List
                var serialLines = new List<string>();
                foreach (var d in details)
                {
                    if (d.SerialNumbers != null && d.SerialNumbers.Any())
                    {
                        serialLines.Add($"• {d.ProductName}:\n  " + string.Join(", ", d.SerialNumbers));
                    }
                }
                var lblSerials = new Label
                {
                    Text = "Danh sách Serials:\n" + (serialLines.Any() ? string.Join("\n", serialLines) : "Không có quản lý serial"),
                    Font = new Font("Consolas", 8.5F),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10, 5, 10, 10)
                };
                tlpPrint.Controls.Add(lblSerials, 0, 3);

                // Row 4: Totals & Thank you footer
                var lblFooter = new Label
                {
                    Text = $"-----------------------------------------------------------------\n" +
                           $"TỔNG THANH TOÁN: {order.FinalAmount.ToString("N0")} VNĐ\n" +
                           $"-----------------------------------------------------------------\n" +
                           $"Cảm ơn quý khách đã mua hàng!\n" +
                           $"www.laptopaz.vn",
                    Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(16, 185, 129), // Emerald Green
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(10, 5, 10, 10)
                };
                tlpPrint.Controls.Add(lblFooter, 0, 4);

                // Bottom Action Buttons (Outside printable area)
                var pnlButtons = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 55,
                    BackColor = Color.FromArgb(241, 245, 249)
                };
                popup.Controls.Add(pnlButtons);

                var btnPrint = CreatePremiumButton("In Hóa Đơn", Color.FromArgb(16, 185, 129), Color.White);
                btnPrint.Size = new Size(140, 35);
                btnPrint.Location = new Point(140, 10);
                btnPrint.Click += (s, ev) =>
                {
                    try
                    {
                        var printDoc = new System.Drawing.Printing.PrintDocument();
                        printDoc.PrintPage += (ps, pe) =>
                        {
                            Bitmap bmp = new Bitmap(pnlPrintArea.Width, pnlPrintArea.Height);
                            pnlPrintArea.DrawToBitmap(bmp, new Rectangle(0, 0, pnlPrintArea.Width, pnlPrintArea.Height));
                            pe.Graphics.DrawImage(bmp, 0, 0);
                        };
                        var printPreview = new PrintPreviewDialog();
                        printPreview.Document = printDoc;
                        printPreview.ShowDialog(popup);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể in hóa đơn: " + ex.Message, "Lỗi in ấn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlButtons.Controls.Add(btnPrint);

                var btnClose = CreatePremiumButton("Đóng", Color.FromArgb(100, 116, 139), Color.White);
                btnClose.Size = new Size(100, 35);
                btnClose.Location = new Point(295, 10);
                btnClose.Click += (s, ev) => popup.Close();
                pnlButtons.Controls.Add(btnClose);

                popup.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi hiển thị chi tiết hóa đơn: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ----------------------------------------------------
        // VIEW: RETURNS MANAGEMENT
        // ----------------------------------------------------
        private DataGridView _gridReturnsHistory, _gridReturnsOrders;
        private TextBox _txtReturnsSearch, _txtReturnsReason;
        private CheckedListBox _clbReturnsSerials;
        private int _returnsSelectedOrderId = 0;

        private void ShowReturnsView()
        {
            ClearContainer();
            _returnsSelectedOrderId = 0;

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Left: Process returns
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Right: History of returns

            // Left Side
            var pnlLeft = CreatePremiumPanel(Color.White, 15);
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.Margin = new Padding(5);
            var lblLeft = CreateCardTitle("🔄 ĐỔI TRẢ HÀNG LỖI");
            lblLeft.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(lblLeft);

            // Search Order section
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(10) };
            
            pnlSearch.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlSearch.Width - 1, pnlSearch.Height - 1);
                }
            };

            var lblSearch = new Label { Text = "Nhập Mã Hóa Đơn hoặc SĐT Khách Hàng:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _txtReturnsSearch = new TextBox { Width = 230, Font = new Font("Segoe UI", 10F), BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(10, 30) };
            
            var btnSearch = CreatePremiumButton("Tìm Hóa Đơn", Color.FromArgb(59, 130, 246), Color.White);
            btnSearch.Location = new Point(250, 28);
            btnSearch.Width = 115;
            btnSearch.Click += (s, ev) => LoadReturnsOrdersGrid(_txtReturnsSearch.Text);

            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(_txtReturnsSearch);
            pnlSearch.Controls.Add(btnSearch);
            pnlLeft.Controls.Add(pnlSearch);

            // Orders Grid
            _gridReturnsOrders = CreatePremiumGrid();
            pnlLeft.Controls.Add(_gridReturnsOrders);
            _gridReturnsOrders.BringToFront();
            _gridReturnsOrders.Columns.Add("OrderId", "ID");
            _gridReturnsOrders.Columns["OrderId"].Visible = false;
            _gridReturnsOrders.Columns.Add("OrderCode", "Mã HĐ");
            _gridReturnsOrders.Columns.Add("CustomerName", "Khách Hàng");
            _gridReturnsOrders.Columns.Add("OrderDate", "Ngày Mua");

            _gridReturnsOrders.SelectionChanged += (s, ev) => {
                if (_gridReturnsOrders.SelectedRows.Count > 0)
                {
                    _returnsSelectedOrderId = Convert.ToInt32(_gridReturnsOrders.SelectedRows[0].Cells["OrderId"].Value);
                    LoadSoldSerialsListForReturn(_returnsSelectedOrderId);
                }
            };

            // Selection list and reason form
            var pnlReturnProcess = new Panel { Dock = DockStyle.Bottom, Height = 220, BackColor = Color.White, Padding = new Padding(0, 10, 0, 0) };
            
            var lblSerials = new Label { Text = "Chọn số Serial máy khách trả lại:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _clbReturnsSerials = new CheckedListBox { Width = 260, Height = 140, BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(10, 30) };

            var lblReason = new Label { Text = "Lý do đổi trả / Tình trạng lỗi *:", ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(285, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold) };
            _txtReturnsReason = new TextBox { Width = 240, Height = 80, Multiline = true, BackColor = Color.White, ForeColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.FixedSingle, Location = new Point(285, 30), ScrollBars = ScrollBars.Vertical };

            var btnSubmitReturn = CreatePremiumButton("Xác Nhận Nhận Trả Hàng", Color.FromArgb(239, 68, 68), Color.White);
            btnSubmitReturn.Location = new Point(285, 125);
            btnSubmitReturn.Width = 240;
            btnSubmitReturn.Height = 45;
            btnSubmitReturn.Click += (s, ev) => SubmitProductReturn();

            pnlReturnProcess.Controls.Add(lblSerials);
            pnlReturnProcess.Controls.Add(_clbReturnsSerials);
            pnlReturnProcess.Controls.Add(lblReason);
            pnlReturnProcess.Controls.Add(_txtReturnsReason);
            pnlReturnProcess.Controls.Add(btnSubmitReturn);
            pnlLeft.Controls.Add(pnlReturnProcess);

            // Right Side: History
            var pnlRight = CreatePremiumPanel(Color.White, 15);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Margin = new Padding(5);
            var lblRight = CreateCardTitle("📜 DANH SÁCH ĐÃ ĐỔI TRẢ");
            lblRight.Dock = DockStyle.Top;
            pnlRight.Controls.Add(lblRight);

            _gridReturnsHistory = CreatePremiumGrid();
            pnlRight.Controls.Add(_gridReturnsHistory);
            _gridReturnsHistory.BringToFront();
            _gridReturnsHistory.Columns.Add("ReturnId", "ID");
            _gridReturnsHistory.Columns["ReturnId"].Visible = false;
            _gridReturnsHistory.Columns.Add("OrderCode", "Hóa Đơn Gốc");
            _gridReturnsHistory.Columns.Add("CustomerName", "Khách Hàng");
            _gridReturnsHistory.Columns.Add("Reason", "Lý Do");
            _gridReturnsHistory.Columns.Add("Serials", "Serials Trả");

            mainLayout.Controls.Add(pnlLeft, 0, 0);
            mainLayout.Controls.Add(pnlRight, 1, 0);
            panelMainContainer.Controls.Add(mainLayout);

            LoadReturnsHistoryGrid();
        }

        private void LoadReturnsHistoryGrid()
        {
            _gridReturnsHistory.Rows.Clear();
            var list = _returnService.GetAllReturns();
            foreach (var r in list)
            {
                _gridReturnsHistory.Rows.Add(
                    r.ReturnId,
                    r.OrderCode,
                    r.CustomerName,
                    r.Reason,
                    string.Join(", ", r.ReturnedSerials)
                );
            }
        }

        private void LoadReturnsOrdersGrid(string search)
        {
            _gridReturnsOrders.Rows.Clear();
            if (string.IsNullOrWhiteSpace(search))
            {
                MessageBox.Show("Vui lòng nhập Mã hóa đơn hoặc SĐT Khách hàng để tìm kiếm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var orders = _salesService.GetAllOrders(search);
            foreach (var o in orders)
            {
                _gridReturnsOrders.Rows.Add(
                    o.OrderId,
                    o.OrderCode,
                    o.CustomerName,
                    o.OrderDate.ToString("dd/MM/yyyy")
                );
            }
        }

        private void LoadSoldSerialsListForReturn(int orderId)
        {
            _clbReturnsSerials.Items.Clear();
            var details = _salesService.GetOrderDetails(orderId);
            foreach (var det in details)
            {
                foreach (var serial in det.SerialNumbers)
                {
                    var item = _unitOfWork.ProductItems.GetById(serial);
                    if (item != null && item.Status == "Sold")
                    {
                        _clbReturnsSerials.Items.Add($"{serial} ({det.ProductName})");
                    }
                }
            }
        }

        private void SubmitProductReturn()
        {
            if (_returnsSelectedOrderId == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_clbReturnsSerials.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng tích chọn ít nhất 1 số Serial máy cần trả.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtReturnsReason.Text))
            {
                MessageBox.Show("Vui lòng nhập lý do đổi trả chi tiết.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var serialsToReturn = new List<string>();
            foreach (var checkedItem in _clbReturnsSerials.CheckedItems)
            {
                string text = checkedItem.ToString();
                string serial = text.Split(' ')[0];
                serialsToReturn.Add(serial);
            }

            string reason = _txtReturnsReason.Text.Trim();
            int createdBy = SessionHelper.CurrentUserId;

            try
            {
                bool success = _returnService.CreateReturn(_returnsSelectedOrderId, createdBy, reason, serialsToReturn);
                if (success)
                {
                    MessageBox.Show("Nhận trả hàng thành công! Số lượng máy đã trả đã được cộng lại kho vật lý ở dạng lỗi (Defective).", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtReturnsReason.Clear();
                    _clbReturnsSerials.Items.Clear();
                    _returnsSelectedOrderId = 0;
                    _txtReturnsSearch.Clear();
                    _gridReturnsOrders.Rows.Clear();
                    
                    LoadReturnsHistoryGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đổi trả hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
