using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinClient
{
    public partial class MainForm
    {
        private void InitSidebar()
        {
            pnlSidebar = new Panel { 
                Size = new Size(250, 780), 
                Location = new Point(-250, 0), 
                BackColor = Color.FromArgb(15, 23, 42), // Obsidian Blue
                Visible = true
            };
            this.Controls.Add(pnlSidebar);
            pnlSidebar.BringToFront();

            Panel pnlSideHeader = new Panel { Size = new Size(250, 75), Dock = DockStyle.Top, BackColor = Color.FromArgb(30, 41, 59) };
            pnlSidebar.Controls.Add(pnlSideHeader);

            Label lblMenu = new Label { 
                Text = "ĐIỀU HƯỚNG", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI Semibold", 10), 
                Location = new Point(20, 25), 
                AutoSize = true 
            };
            pnlSideHeader.Controls.Add(lblMenu);

            int top = 90;
            AddSidebarItem("Bảng điều khiển", top, (s, e) => ShowDashboard()); top += 55;
            AddSidebarItem("Tra cứu", top, (s, e) => ShowSearchPage()); top += 55;
            AddSidebarItem("Hồ sơ cá nhân", top, (s, e) => ShowUserInfo()); top += 55;

            if (UserRole == "ADMIN")
            {
                Label lblAdmin = new Label { Text = "QUẢN TRỊ VIÊN", ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI Bold", 8), Location = new Point(20, top + 10), AutoSize = true };
                pnlSidebar.Controls.Add(lblAdmin);
                top += 35;
                AddSidebarItem("Cấp tài khoản", top, (s, e) => BtnCreateUser_Click(null, null)); top += 55;
                AddSidebarItem("Quản lý tài khoản", top, (s, e) => { if (!isViewingUsers) BtnViewUsers_Click(null, null); }); top += 55;
            }
            
            AddSidebarItem("Đăng xuất", 650, (s, e) => { IsLogout = true; this.Close(); });

            sidebarTimer = new System.Windows.Forms.Timer { Interval = 10 };
            sidebarTimer.Tick += SidebarTimer_Tick;
        }

        private void AddSidebarItem(string text, int top, EventHandler onClick)
        {
            Button btn = new Button {
                Text = text,
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Segoe UI", 10),
                Size = new Size(250, 50),
                Location = new Point(0, top),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btn.Click += (s, e) => { ToggleSidebar(); onClick(s, e); };
            pnlSidebar.Controls.Add(btn);
        }

        private void ToggleSidebar()
        {
            isSidebarOpen = !isSidebarOpen;
            if (isSidebarOpen) pnlSidebar.BringToFront();
            sidebarTimer.Start();
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            int currentX = pnlSidebar.Location.X;
            if (isSidebarOpen)
            {
                if (currentX < 0) pnlSidebar.Location = new Point(currentX + 25, 0); // Tăng tốc độ mượt hơn
                else sidebarTimer.Stop();
            }
            else
            {
                if (currentX > -250) pnlSidebar.Location = new Point(currentX - 25, 0);
                else sidebarTimer.Stop();
            }
        }
    }
}
