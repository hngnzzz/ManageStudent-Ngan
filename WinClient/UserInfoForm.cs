using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinClient
{
    public class UserInfoForm : Form
    {
        public UserInfoForm(string username, string fullname, string role)
        {
            this.Text = "Hồ Sơ Cá Nhân";
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None; // Modern look without standard border
            this.BackColor = Color.White;

            // Rounded corners for the form itself
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Main Layout Panel setup
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle }; // Add slim border
            this.Controls.Add(pnlMain);

            // --- Header Background ---
            Panel pnlHeader = new Panel { 
                Dock = DockStyle.Top, 
                Height = 140
            };
            pnlHeader.Paint += (s, e) => {
                using (LinearGradientBrush brush = new LinearGradientBrush(pnlHeader.ClientRectangle, 
                       Color.FromArgb(0, 120, 215), Color.FromArgb(100, 200, 255), 45F)) // Blue Gradient
                {
                    e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
                }
            };
            // Drag support
            pnlHeader.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); } };
            pnlMain.Controls.Add(pnlHeader);

            // Title
            Label lblTitle = new Label {
                Text = "THÔNG TIN CÁ NHÂN",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            pnlHeader.Controls.Add(lblTitle);

            // Close Button (X)
            Label lblClose = new Label {
                Text = "✕",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14),
                Location = new Point(380, 10),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblClose.Click += (s, e) => this.Close();
            lblClose.MouseEnter += (s, e) => lblClose.ForeColor = Color.Red;
            lblClose.MouseLeave += (s, e) => lblClose.ForeColor = Color.White;
            pnlHeader.Controls.Add(lblClose);

            // --- Avatar (Center Overlap) ---
            int avSize = 110;
            Label lblAvatar = new Label {
                Size = new Size(avSize, avSize),
                Location = new Point((this.Width - avSize) / 2, 85), 
                BackColor = Color.White,
                ForeColor = Color.FromArgb(0, 120, 215),
                Text = !string.IsNullOrEmpty(fullname) ? fullname.Substring(0, 1).ToUpper() : "U",
                Font = new Font("Segoe UI", 45, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Circle Region for Avatar
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, avSize, avSize);
            lblAvatar.Region = new Region(path);
            
            pnlMain.Controls.Add(lblAvatar);
            lblAvatar.BringToFront();

            // --- Info Section ---
            int yStart = 220; // content starts below avatar
            
            AddDetail(pnlMain, "HỌ VÀ TÊN", fullname, yStart);
            AddDetail(pnlMain, "ĐỊA CHỈ EMAIL", username, yStart + 75);
            AddDetail(pnlMain, "VAI TRÒ HỆ THỐNG", role, yStart + 150);

            // --- Bottom Button ---
            Button btnNice = new Button {
                Text = "ĐÓNG HỒ SƠ",
                Size = new Size(200, 45),
                Location = new Point((this.Width - 200) / 2, 450),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNice.FlatAppearance.BorderSize = 0;
            btnNice.Click += (s, e) => this.Close();
            // Hover
            btnNice.MouseEnter += (s, e) => { btnNice.BackColor = Color.FromArgb(0, 120, 215); btnNice.ForeColor = Color.White; };
            btnNice.MouseLeave += (s, e) => { btnNice.BackColor = Color.FromArgb(241, 245, 249); btnNice.ForeColor = Color.FromArgb(30, 41, 59); };
            
            pnlMain.Controls.Add(btnNice);
        }

        private void AddDetail(Panel p, string label, string content, int y)
        {
            Label l = new Label {
                Text = label,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184), // Slate 400
                Location = new Point(0, y),
                Size = new Size(p.Width, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            p.Controls.Add(l);

            Label c = new Label {
                Text = content,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59), // Slate 800
                Location = new Point(0, y + 20),
                Size = new Size(p.Width, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            p.Controls.Add(c);
        }

        // WinAPI for Dragging and Rounded Corners
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
