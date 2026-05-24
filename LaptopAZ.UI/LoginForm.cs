using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LaptopAZ.BLL;
using LaptopAZ.Repository;

namespace LaptopAZ.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        // DLL imports for window dragging
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private static extern void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public LoginForm()
        {
            InitializeComponent();
            _unitOfWork = new UnitOfWork();
            _authService = new AuthService(_unitOfWork);
            
            // Thiết lập phong cách cao cấp (Premium Dark Theme)
            StyleForm();
        }

        private void StyleForm()
        {
            // Set Form Size and background to Slate-50 Light Slate Blue
            this.Size = new Size(900, 640);
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Hook mouse down dragging to Form
            this.MouseDown += new MouseEventHandler(Form_MouseDown);

            // Hide/Repurpose panelLeft
            panelLeft.Visible = false;

            // Design panelRight as centered white card
            panelRight.Dock = DockStyle.None;
            panelRight.Size = new Size(400, 520);
            panelRight.Location = new Point((this.ClientSize.Width - panelRight.Width) / 2, (this.ClientSize.Height - panelRight.Height) / 2 - 25);
            panelRight.BackColor = Color.White;

            // Draw custom premium card border
            panelRight.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1)) // Slate-200
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(pen, 0, 0, panelRight.Width - 1, panelRight.Height - 1);
                }
            };

            // Remove/Clear standard control properties from designer
            lblBrandName.Parent = panelRight;
            lblBrandSubtitle.Parent = panelRight;

            // 1. Brand Logo (Laptop Icon)
            Label lblLaptopIcon = new Label();
            lblLaptopIcon.Text = "💻";
            lblLaptopIcon.Font = new Font("Segoe UI", 36F);
            lblLaptopIcon.ForeColor = Color.FromArgb(0, 82, 204);
            lblLaptopIcon.AutoSize = true;
            lblLaptopIcon.Location = new Point((panelRight.Width - 75) / 2, 15);
            panelRight.Controls.Add(lblLaptopIcon);

            // 2. Brand name
            lblBrandName.Text = "LAPTOP AZ";
            lblBrandName.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblBrandName.ForeColor = Color.FromArgb(0, 82, 204);
            lblBrandName.AutoSize = true;
            lblBrandName.Location = new Point((panelRight.Width - 165) / 2, 75);

            // 3. Brand subtitle
            lblBrandSubtitle.Text = "INVENTORY CONTROL SYSTEM";
            lblBrandSubtitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblBrandSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate-400
            lblBrandSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblBrandSubtitle.Location = new Point(10, 115);
            lblBrandSubtitle.Size = new Size(panelRight.Width - 20, 20);

            // 4. Login Title
            lblTitle.Text = "ĐĂNG NHẬP";
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(30, 41, 59); // Slate-800
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(35, 145);

            // 5. Login Subtitle
            Label lblLoginSubtitle = new Label();
            lblLoginSubtitle.Text = "Vui lòng nhập thông tin để truy cập hệ thống quản lý.";
            lblLoginSubtitle.Font = new Font("Segoe UI", 9F);
            lblLoginSubtitle.ForeColor = Color.FromArgb(100, 116, 139); // Slate-500
            lblLoginSubtitle.Location = new Point(35, 175);
            lblLoginSubtitle.Size = new Size(330, 20);
            panelRight.Controls.Add(lblLoginSubtitle);

            // 6. Username Label
            lblUsername.Text = "Tên đăng nhập";
            lblUsername.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(71, 85, 105); // Slate-600
            lblUsername.Location = new Point(35, 200);

            // 7. Username input container
            Panel pnlUsername = new Panel();
            pnlUsername.Size = new Size(330, 36);
            pnlUsername.Location = new Point(35, 220);
            pnlUsername.BackColor = Color.FromArgb(248, 250, 252);
            panelRight.Controls.Add(pnlUsername);

            pnlUsername.Controls.Add(txtUsername);
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.BackColor = Color.FromArgb(248, 250, 252);
            txtUsername.ForeColor = Color.FromArgb(30, 41, 59);
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.Width = 290;
            txtUsername.Location = new Point(30, 9);

            Label lblUserIcon = new Label();
            lblUserIcon.Text = "👤";
            lblUserIcon.Font = new Font("Segoe UI", 9.5F);
            lblUserIcon.ForeColor = Color.FromArgb(148, 163, 184);
            lblUserIcon.Location = new Point(8, 8);
            lblUserIcon.Size = new Size(20, 20);
            pnlUsername.Controls.Add(lblUserIcon);

            txtUsername.Enter += (s, ev) => { pnlUsername.BackColor = Color.White; pnlUsername.Invalidate(); };
            txtUsername.Leave += (s, ev) => { pnlUsername.BackColor = Color.FromArgb(248, 250, 252); pnlUsername.Invalidate(); };
            pnlUsername.Paint += (s, e) => {
                bool hasFocus = txtUsername.Focused;
                using (var pen = new Pen(hasFocus ? Color.FromArgb(0, 82, 204) : Color.FromArgb(226, 232, 240), hasFocus ? 2 : 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlUsername.Width - 1, pnlUsername.Height - 1);
                }
            };

            // 8. Password Label
            lblPassword.Text = "Mật khẩu";
            lblPassword.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(71, 85, 105);
            lblPassword.Location = new Point(35, 260);

            // Forgot Password link
            LinkLabel lnkForgotPassword = new LinkLabel();
            lnkForgotPassword.Text = "Quên mật khẩu?";
            lnkForgotPassword.Font = new Font("Segoe UI", 8.5F);
            lnkForgotPassword.ForeColor = Color.FromArgb(0, 82, 204);
            lnkForgotPassword.LinkColor = Color.FromArgb(0, 82, 204);
            lnkForgotPassword.ActiveLinkColor = Color.FromArgb(9, 97, 239);
            lnkForgotPassword.VisitedLinkColor = Color.FromArgb(0, 82, 204);
            lnkForgotPassword.LinkBehavior = LinkBehavior.HoverUnderline;
            lnkForgotPassword.AutoSize = true;
            lnkForgotPassword.Location = new Point(275, 260);
            panelRight.Controls.Add(lnkForgotPassword);
            lnkForgotPassword.Click += (s, ev) => {
                MessageBox.Show("Vui lòng liên hệ Quản trị viên (admin@laptopaz.vn) để cấp lại mật khẩu.", "Khôi phục mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // 9. Password input container
            Panel pnlPassword = new Panel();
            pnlPassword.Size = new Size(330, 36);
            pnlPassword.Location = new Point(35, 280);
            pnlPassword.BackColor = Color.FromArgb(248, 250, 252);
            panelRight.Controls.Add(pnlPassword);

            pnlPassword.Controls.Add(txtPassword);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.BackColor = Color.FromArgb(248, 250, 252);
            txtPassword.ForeColor = Color.FromArgb(30, 41, 59);
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Width = 265;
            txtPassword.Location = new Point(30, 9);

            Label lblLockIcon = new Label();
            lblLockIcon.Text = "🔒";
            lblLockIcon.Font = new Font("Segoe UI", 9.5F);
            lblLockIcon.ForeColor = Color.FromArgb(148, 163, 184);
            lblLockIcon.Location = new Point(8, 8);
            lblLockIcon.Size = new Size(20, 20);
            pnlPassword.Controls.Add(lblLockIcon);

            Label lblEyeIcon = new Label();
            lblEyeIcon.Text = "👁️";
            lblEyeIcon.Font = new Font("Segoe UI", 9.5F);
            lblEyeIcon.ForeColor = Color.FromArgb(148, 163, 184);
            lblEyeIcon.Location = new Point(302, 8);
            lblEyeIcon.Size = new Size(20, 20);
            lblEyeIcon.Cursor = Cursors.Hand;
            pnlPassword.Controls.Add(lblEyeIcon);

            lblEyeIcon.Click += (s, ev) => {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                lblEyeIcon.Text = txtPassword.UseSystemPasswordChar ? "👁️" : "🙈";
            };

            txtPassword.Enter += (s, ev) => { pnlPassword.BackColor = Color.White; pnlPassword.Invalidate(); };
            txtPassword.Leave += (s, ev) => { pnlPassword.BackColor = Color.FromArgb(248, 250, 252); pnlPassword.Invalidate(); };
            pnlPassword.Paint += (s, e) => {
                bool hasFocus = txtPassword.Focused;
                using (var pen = new Pen(hasFocus ? Color.FromArgb(0, 82, 204) : Color.FromArgb(226, 232, 240), hasFocus ? 2 : 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlPassword.Width - 1, pnlPassword.Height - 1);
                }
            };

            // 10. Checkbox "Hiển thị mật khẩu" repurposed as "Ghi nhớ đăng nhập"
            chkShowPassword.Text = "Ghi nhớ đăng nhập";
            chkShowPassword.ForeColor = Color.FromArgb(100, 116, 139); // Slate-500
            chkShowPassword.Font = new Font("Segoe UI", 8.5F);
            chkShowPassword.Location = new Point(35, 323);
            chkShowPassword.CheckedChanged -= chkShowPassword_CheckedChanged; // We don't want check changes to toggle show password anymore, because we have eye icon!

            // 11. Login button
            btnLogin.BackColor = Color.FromArgb(0, 82, 204);
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(9, 97, 239);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 68, 170);
            btnLogin.Location = new Point(35, 350);
            btnLogin.Size = new Size(330, 42);

            // 12. SSO Azure
            Label lblSSOOr = new Label();
            lblSSOOr.Text = "Hoặc tiếp tục với";
            lblSSOOr.Font = new Font("Segoe UI", 8F);
            lblSSOOr.ForeColor = Color.FromArgb(148, 163, 184); // slate-400
            lblSSOOr.TextAlign = ContentAlignment.MiddleCenter;
            lblSSOOr.Location = new Point(35, 400);
            lblSSOOr.Size = new Size(330, 15);
            panelRight.Controls.Add(lblSSOOr);

            Button btnSSO = new Button();
            btnSSO.Text = "  💻  SSO Azure";
            btnSSO.BackColor = Color.White;
            btnSSO.ForeColor = Color.FromArgb(30, 41, 59); // slate-800
            btnSSO.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            btnSSO.FlatStyle = FlatStyle.Flat;
            btnSSO.FlatAppearance.BorderSize = 1;
            btnSSO.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); // slate-200
            btnSSO.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnSSO.FlatAppearance.MouseDownBackColor = Color.FromArgb(241, 245, 249);
            btnSSO.Location = new Point(35, 420);
            btnSSO.Size = new Size(330, 36);
            btnSSO.Cursor = Cursors.Hand;
            panelRight.Controls.Add(btnSSO);
            btnSSO.Click += (s, ev) => {
                MessageBox.Show("Hệ thống đăng nhập một lần (SSO) Azure đang được bảo trì. Vui lòng đăng nhập bằng tài khoản nội bộ.", "SSO Azure", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            // 13. Error label
            lblError.ForeColor = Color.FromArgb(244, 63, 94); // Rose-500
            lblError.Location = new Point(35, 465);
            lblError.Size = new Size(330, 20);
            lblError.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            lblError.BackColor = Color.Transparent;

            // 14. Adjust close and minimize buttons to top-right of screen
            this.Controls.Add(btnClose);
            this.Controls.Add(btnMinimize);
            btnClose.BringToFront();
            btnMinimize.BringToFront();
            btnClose.Location = new Point(this.Width - 45, 10);
            btnMinimize.Location = new Point(this.Width - 80, 10);
            btnClose.BackColor = Color.Transparent;
            btnMinimize.BackColor = Color.Transparent;
            btnClose.ForeColor = Color.FromArgb(148, 163, 184);
            btnMinimize.ForeColor = Color.FromArgb(148, 163, 184);

            // 15. Form Footer labels
            Label lblFooter1 = new Label();
            lblFooter1.Text = "🛡️ Hệ thống quản lý cửa hàng chuyên nghiệp";
            lblFooter1.Font = new Font("Segoe UI", 9F);
            lblFooter1.ForeColor = Color.FromArgb(100, 116, 139); // slate-500
            lblFooter1.TextAlign = ContentAlignment.MiddleCenter;
            lblFooter1.Location = new Point(10, 580);
            lblFooter1.Size = new Size(this.Width - 20, 20);
            lblFooter1.BackColor = Color.Transparent;
            this.Controls.Add(lblFooter1);

            Label lblFooter2 = new Label();
            lblFooter2.Text = "© 2026 Laptop AZ. Bản quyền thuộc về Azure Management Corp.";
            lblFooter2.Font = new Font("Segoe UI", 8F);
            lblFooter2.ForeColor = Color.FromArgb(148, 163, 184); // slate-400
            lblFooter2.TextAlign = ContentAlignment.MiddleCenter;
            lblFooter2.Location = new Point(10, 605);
            lblFooter2.Size = new Size(this.Width - 20, 20);
            lblFooter2.BackColor = Color.Transparent;
            this.Controls.Add(lblFooter2);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _unitOfWork.Dispose();
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.White;
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.FromArgb(148, 163, 184);
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "ĐANG ĐĂNG NHẬP...";

            try
            {
                bool success = _authService.Login(username, password);
                if (success)
                {
                    // Open Main Form
                    this.Hide();
                    var mainForm = new MainForm(_unitOfWork);
                    mainForm.FormClosed += (s, args) => {
                        _unitOfWork.Dispose();
                        Application.Exit();
                    };
                    mainForm.Show();
                }
                else
                {
                    lblError.Text = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                    btnLogin.Enabled = true;
                    btnLogin.Text = "ĐĂNG NHẬP";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Lỗi hệ thống: " + ex.Message;
                btnLogin.Enabled = true;
                btnLogin.Text = "ĐĂNG NHẬP";
            }
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
