namespace LaptopAZ.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.panelActiveIndicator = new System.Windows.Forms.Panel();
            this.lblSidebarBrand = new System.Windows.Forms.Label();
            this.btnTabDashboard = new System.Windows.Forms.Button();
            this.btnTabProducts = new System.Windows.Forms.Button();
            this.btnTabCategories = new System.Windows.Forms.Button();
            this.btnTabImport = new System.Windows.Forms.Button();
            this.btnTabSales = new System.Windows.Forms.Button();
            this.btnTabReturns = new System.Windows.Forms.Button();
            this.btnTabPartners = new System.Windows.Forms.Button();
            this.btnTabStaff = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblClock = new System.Windows.Forms.Label();
            this.lblEmployeeInfo = new System.Windows.Forms.Label();
            this.lblActiveTabTitle = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.panelMainContainer = new System.Windows.Forms.Panel();
            this.panelSidebar.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSidebar
            // 
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.panelSidebar.Controls.Add(this.panelActiveIndicator);
            this.panelSidebar.Controls.Add(this.btnTabStaff);
            this.panelSidebar.Controls.Add(this.btnTabPartners);
            this.panelSidebar.Controls.Add(this.btnTabReturns);
            this.panelSidebar.Controls.Add(this.btnTabSales);
            this.panelSidebar.Controls.Add(this.btnTabImport);
            this.panelSidebar.Controls.Add(this.btnTabCategories);
            this.panelSidebar.Controls.Add(this.btnTabProducts);
            this.panelSidebar.Controls.Add(this.btnTabDashboard);
            this.panelSidebar.Controls.Add(this.lblSidebarBrand);
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Location = new System.Drawing.Point(0, 0);
            this.panelSidebar.Name = "panelSidebar";
            this.panelSidebar.Size = new System.Drawing.Size(220, 700);
            this.panelSidebar.TabIndex = 0;
            // 
            // panelActiveIndicator
            // 
            this.panelActiveIndicator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            this.panelActiveIndicator.Location = new System.Drawing.Point(0, 100);
            this.panelActiveIndicator.Name = "panelActiveIndicator";
            this.panelActiveIndicator.Size = new System.Drawing.Size(6, 45);
            this.panelActiveIndicator.TabIndex = 2;
            // 
            // lblSidebarBrand
            // 
            this.lblSidebarBrand.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSidebarBrand.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSidebarBrand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            this.lblSidebarBrand.Location = new System.Drawing.Point(0, 0);
            this.lblSidebarBrand.Name = "lblSidebarBrand";
            this.lblSidebarBrand.Size = new System.Drawing.Size(220, 80);
            this.lblSidebarBrand.TabIndex = 0;
            this.lblSidebarBrand.Text = "LAPTOP AZ";
            this.lblSidebarBrand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTabDashboard
            // 
            this.btnTabDashboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabDashboard.FlatAppearance.BorderSize = 0;
            this.btnTabDashboard.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabDashboard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabDashboard.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabDashboard.ForeColor = System.Drawing.Color.White;
            this.btnTabDashboard.Location = new System.Drawing.Point(6, 100);
            this.btnTabDashboard.Name = "btnTabDashboard";
            this.btnTabDashboard.Size = new System.Drawing.Size(214, 45);
            this.btnTabDashboard.TabIndex = 1;
            this.btnTabDashboard.Text = "  📊   Dashboard";
            this.btnTabDashboard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabDashboard.UseVisualStyleBackColor = true;
            this.btnTabDashboard.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabProducts
            // 
            this.btnTabProducts.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabProducts.FlatAppearance.BorderSize = 0;
            this.btnTabProducts.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabProducts.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabProducts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabProducts.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabProducts.ForeColor = System.Drawing.Color.White;
            this.btnTabProducts.Location = new System.Drawing.Point(6, 150);
            this.btnTabProducts.Name = "btnTabProducts";
            this.btnTabProducts.Size = new System.Drawing.Size(214, 45);
            this.btnTabProducts.TabIndex = 3;
            this.btnTabProducts.Text = "  💻   Sản Phẩm";
            this.btnTabProducts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabProducts.UseVisualStyleBackColor = true;
            this.btnTabProducts.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabCategories
            // 
            this.btnTabCategories.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabCategories.FlatAppearance.BorderSize = 0;
            this.btnTabCategories.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabCategories.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabCategories.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabCategories.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabCategories.ForeColor = System.Drawing.Color.White;
            this.btnTabCategories.Location = new System.Drawing.Point(6, 200);
            this.btnTabCategories.Name = "btnTabCategories";
            this.btnTabCategories.Size = new System.Drawing.Size(214, 45);
            this.btnTabCategories.TabIndex = 4;
            this.btnTabCategories.Text = "  📁   Hãng & Danh Mục";
            this.btnTabCategories.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabCategories.UseVisualStyleBackColor = true;
            this.btnTabCategories.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabImport
            // 
            this.btnTabImport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabImport.FlatAppearance.BorderSize = 0;
            this.btnTabImport.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabImport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabImport.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabImport.ForeColor = System.Drawing.Color.White;
            this.btnTabImport.Location = new System.Drawing.Point(6, 250);
            this.btnTabImport.Name = "btnTabImport";
            this.btnTabImport.Size = new System.Drawing.Size(214, 45);
            this.btnTabImport.TabIndex = 5;
            this.btnTabImport.Text = "  📦   Nhập Kho";
            this.btnTabImport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabImport.UseVisualStyleBackColor = true;
            this.btnTabImport.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabSales
            // 
            this.btnTabSales.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabSales.FlatAppearance.BorderSize = 0;
            this.btnTabSales.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabSales.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabSales.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabSales.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabSales.ForeColor = System.Drawing.Color.White;
            this.btnTabSales.Location = new System.Drawing.Point(6, 300);
            this.btnTabSales.Name = "btnTabSales";
            this.btnTabSales.Size = new System.Drawing.Size(214, 45);
            this.btnTabSales.TabIndex = 6;
            this.btnTabSales.Text = "  🛒   Bán Hàng";
            this.btnTabSales.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabSales.UseVisualStyleBackColor = true;
            this.btnTabSales.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabReturns
            // 
            this.btnTabReturns.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabReturns.FlatAppearance.BorderSize = 0;
            this.btnTabReturns.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabReturns.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabReturns.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabReturns.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabReturns.ForeColor = System.Drawing.Color.White;
            this.btnTabReturns.Location = new System.Drawing.Point(6, 350);
            this.btnTabReturns.Name = "btnTabReturns";
            this.btnTabReturns.Size = new System.Drawing.Size(214, 45);
            this.btnTabReturns.TabIndex = 7;
            this.btnTabReturns.Text = "  🔄   Đổi Trả Hàng";
            this.btnTabReturns.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabReturns.UseVisualStyleBackColor = true;
            this.btnTabReturns.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabPartners
            // 
            this.btnTabPartners.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabPartners.FlatAppearance.BorderSize = 0;
            this.btnTabPartners.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabPartners.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabPartners.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabPartners.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabPartners.ForeColor = System.Drawing.Color.White;
            this.btnTabPartners.Location = new System.Drawing.Point(6, 400);
            this.btnTabPartners.Name = "btnTabPartners";
            this.btnTabPartners.Size = new System.Drawing.Size(214, 45);
            this.btnTabPartners.TabIndex = 8;
            this.btnTabPartners.Text = "  👥   Khách & Đối Tác";
            this.btnTabPartners.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabPartners.UseVisualStyleBackColor = true;
            this.btnTabPartners.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // btnTabStaff
            // 
            this.btnTabStaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabStaff.FlatAppearance.BorderSize = 0;
            this.btnTabStaff.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.btnTabStaff.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTabStaff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabStaff.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTabStaff.ForeColor = System.Drawing.Color.White;
            this.btnTabStaff.Location = new System.Drawing.Point(6, 450);
            this.btnTabStaff.Name = "btnTabStaff";
            this.btnTabStaff.Size = new System.Drawing.Size(214, 45);
            this.btnTabStaff.TabIndex = 9;
            this.btnTabStaff.Text = "  ⚙️   Quản Lý Nhân Viên";
            this.btnTabStaff.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabStaff.UseVisualStyleBackColor = true;
            this.btnTabStaff.Click += new System.EventHandler(this.TabButton_Click);
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelHeader.Controls.Add(this.btnLogout);
            this.panelHeader.Controls.Add(this.lblClock);
            this.panelHeader.Controls.Add(this.lblEmployeeInfo);
            this.panelHeader.Controls.Add(this.lblActiveTabTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(220, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(980, 80);
            this.panelHeader.TabIndex = 1;
            // 
            // lblClock
            // 
            this.lblClock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClock.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            this.lblClock.Location = new System.Drawing.Point(740, 43);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(228, 23);
            this.lblClock.TabIndex = 2;
            this.lblClock.Text = "20/05/2026 15:00:00";
            this.lblClock.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEmployeeInfo
            // 
            this.lblEmployeeInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEmployeeInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmployeeInfo.ForeColor = System.Drawing.Color.White;
            this.lblEmployeeInfo.Location = new System.Drawing.Point(620, 15);
            this.lblEmployeeInfo.Name = "lblEmployeeInfo";
            this.lblEmployeeInfo.Size = new System.Drawing.Size(250, 23);
            this.lblEmployeeInfo.TabIndex = 1;
            this.lblEmployeeInfo.Text = "Nhân viên: Lê Minh Tuấn (Admin)";
            this.lblEmployeeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblActiveTabTitle
            // 
            this.lblActiveTabTitle.AutoSize = true;
            this.lblActiveTabTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveTabTitle.ForeColor = System.Drawing.Color.White;
            this.lblActiveTabTitle.Location = new System.Drawing.Point(20, 24);
            this.lblActiveTabTitle.Name = "lblActiveTabTitle";
            this.lblActiveTabTitle.Size = new System.Drawing.Size(166, 32);
            this.lblActiveTabTitle.TabIndex = 0;
            this.lblActiveTabTitle.Text = "DASHBOARD";
            // 
            // btnLogout
            // 
            this.btnLogout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogout.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Location = new System.Drawing.Point(880, 12);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(88, 28);
            this.btnLogout.TabIndex = 3;
            this.btnLogout.Text = "ĐĂNG XUẤT";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // panelMainContainer
            // 
            this.panelMainContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelMainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainContainer.Location = new System.Drawing.Point(220, 80);
            this.panelMainContainer.Name = "panelMainContainer";
            this.panelMainContainer.Size = new System.Drawing.Size(980, 620);
            this.panelMainContainer.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelMainContainer);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelSidebar);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LaptopAZ - Hệ Thống Quản Lý Cửa Hàng Laptop";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelSidebar.ResumeLayout(false);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSidebar;
        private System.Windows.Forms.Label lblSidebarBrand;
        private System.Windows.Forms.Button btnTabDashboard;
        private System.Windows.Forms.Button btnTabProducts;
        private System.Windows.Forms.Button btnTabCategories;
        private System.Windows.Forms.Button btnTabImport;
        private System.Windows.Forms.Button btnTabSales;
        private System.Windows.Forms.Button btnTabReturns;
        private System.Windows.Forms.Button btnTabPartners;
        private System.Windows.Forms.Button btnTabStaff;
        private System.Windows.Forms.Panel panelActiveIndicator;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Label lblEmployeeInfo;
        private System.Windows.Forms.Label lblActiveTabTitle;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel panelMainContainer;
    }
}
