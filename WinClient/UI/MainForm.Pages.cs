using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace WinClient
{
    public partial class MainForm
    {
        private void ShowDashboard()
        {
            if (pnlSearchPage != null) pnlSearchPage.Visible = false;
            if (pnlProfilePage != null) pnlProfilePage.Visible = false;
            if (isViewingUsers) BtnViewUsers_Click(null, null);
        }

        private void ShowUserInfo()
        {
            if (isSidebarOpen) ToggleSidebar();
            if (pnlSearchPage != null) pnlSearchPage.Visible = false;

            if (pnlProfilePage == null)
            {
                pnlProfilePage = new Panel { 
                    Size = this.ClientSize, 
                    BackColor = Color.FromArgb(240, 245, 250), 
                    Location = new Point(0, 0), 
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right 
                };
                this.Controls.Add(pnlProfilePage);

                // Header
                Panel pnlHead = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.FromArgb(0, 120, 215) };
                pnlProfilePage.Controls.Add(pnlHead);
                Label lblTitle = new Label { 
                    Text = "HỒ SƠ CÁ NHÂN", 
                    Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                    ForeColor = Color.White, 
                    Location = new Point(20, 18), 
                    AutoSize = true 
                };
                pnlHead.Controls.Add(lblTitle);

                // Left Column - Avatar Card
                Panel cardAvatar = new Panel {
                    Location = new Point(30, 90),
                    Size = new Size(300, 380),
                    BackColor = Color.White
                };
                cardAvatar.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, cardAvatar.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlProfilePage.Controls.Add(cardAvatar);

                // Avatar
                picProfileAvatar = new PictureBox {
                    Size = new Size(140, 140),
                    Location = new Point((cardAvatar.Width - 140) / 2, 30),
                    BackColor = Color.FromArgb(0, 120, 215),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Cursor = Cursors.Hand
                };
                
                // Load saved avatar if exists
                string avatarPath = GetAvatarPath();
                if (System.IO.File.Exists(avatarPath))
                {
                    picProfileAvatar.Image = Image.FromFile(avatarPath);
                }
                else
                {
                    // Default: Draw initial letter
                    Bitmap bmp = new Bitmap(140, 140);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.FromArgb(0, 120, 215));
                        string initial = !string.IsNullOrEmpty(MyFullName) ? MyFullName.Substring(0, 1).ToUpper() : "U";
                        using (Font f = new Font("Segoe UI", 55, FontStyle.Bold))
                        using (Brush br = new SolidBrush(Color.White))
                        {
                            SizeF sz = g.MeasureString(initial, f);
                            g.DrawString(initial, f, br, (140 - sz.Width) / 2, (140 - sz.Height) / 2);
                        }
                    }
                    picProfileAvatar.Image = bmp;
                }
                
                // Make circular
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, 140, 140);
                picProfileAvatar.Region = new Region(path);
                picProfileAvatar.Click += (s, e) => UploadAvatar();
                cardAvatar.Controls.Add(picProfileAvatar);

                // Name under avatar
                Label lblAvatarName = new Label {
                    Text = MyFullName,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    Location = new Point(0, 185),
                    Size = new Size(cardAvatar.Width, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                cardAvatar.Controls.Add(lblAvatarName);

                // Role badge
                Label lblRoleBadge = new Label {
                    Text = UserRole == "ADMIN" ? "Quản trị viên" : "Giáo viên",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = UserRole == "ADMIN" ? Color.FromArgb(234, 88, 12) : Color.FromArgb(37, 99, 235),
                    BackColor = UserRole == "ADMIN" ? Color.FromArgb(255, 247, 237) : Color.FromArgb(239, 246, 255),
                    Padding = new Padding(15, 5, 15, 5),
                    Location = new Point((cardAvatar.Width - 160) / 2, 225),
                    Size = new Size(160, 32),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                cardAvatar.Controls.Add(lblRoleBadge);

                // Upload hint
                Label lblUploadHint = new Label {
                    Text = "Nhấn vào ảnh để thay đổi",
                    Font = new Font("Segoe UI", 9, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Location = new Point(0, 280),
                    Size = new Size(cardAvatar.Width, 25),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Cursors.Hand
                };
                lblUploadHint.Click += (s, e) => UploadAvatar();
                cardAvatar.Controls.Add(lblUploadHint);

                // Logout button
                Button btnLogout = new Button {
                    Text = "ĐĂNG XUẤT",
                    Location = new Point(40, 330),
                    Size = new Size(cardAvatar.Width - 80, 42),
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnLogout.FlatAppearance.BorderSize = 0;
                btnLogout.Click += (s, e) => { IsLogout = true; this.Close(); };
                cardAvatar.Controls.Add(btnLogout);

                // Right Column - Info Card (expanded)
                Panel cardInfo = new Panel {
                    Location = new Point(370, 90),
                    Size = new Size(580, 400),
                    BackColor = Color.White
                };
                cardInfo.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, cardInfo.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlProfilePage.Controls.Add(cardInfo);

                Label lblInfoTitle = new Label {
                    Text = "THÔNG TIN TÀI KHOẢN",
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    Location = new Point(30, 25),
                    AutoSize = true
                };
                cardInfo.Controls.Add(lblInfoTitle);

                // Info Grid - 2 columns with more spacing
                int yInfo = 75;
                int col1 = 30, col2 = 310;
                int rowGap = 80;
                
                AddProfileFieldCompact(cardInfo, "EMAIL ĐĂNG NHẬP", MyUsername, col1, yInfo);
                AddProfileFieldCompact(cardInfo, "NGÀY THAM GIA", DateTime.Now.ToString("dd/MM/yyyy"), col2, yInfo);
                yInfo += rowGap;
                
                AddProfileFieldCompact(cardInfo, "HỌ VÀ TÊN", MyFullName, col1, yInfo);
                AddProfileFieldCompact(cardInfo, "VAI TRÒ HỆ THỐNG", UserRole == "ADMIN" ? "Quản trị viên" : "Giáo viên", col2, yInfo);
                yInfo += rowGap;
                
                AddProfileFieldCompact(cardInfo, "SỐ ĐIỆN THOẠI", "Chưa cập nhật", col1, yInfo);
                AddProfileFieldCompact(cardInfo, "ĐƠN VỊ CÔNG TÁC", "Trường Đại Học", col2, yInfo);
                yInfo += rowGap;
                
                AddProfileFieldCompact(cardInfo, "ĐỊA CHỈ", "Chưa cập nhật", col1, yInfo);
                AddProfileFieldCompact(cardInfo, "TRẠNG THÁI", "Đang hoạt động", col2, yInfo);

                // Back button
                Button btnBack = new Button {
                    Text = "← QUAY LẠI TRANG CHỦ",
                    Location = new Point(30, pnlProfilePage.Height - 70),
                    Size = new Size(220, 45),
                    BackColor = Color.FromArgb(71, 85, 105),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                };
                btnBack.FlatAppearance.BorderSize = 0;
                btnBack.Click += (s, e) => { pnlProfilePage.Visible = false; };
                pnlProfilePage.Controls.Add(btnBack);
            }
            else
            {
                // Refresh avatar if changed
                string avatarPath = GetAvatarPath();
                if (System.IO.File.Exists(avatarPath))
                {
                    picProfileAvatar.Image?.Dispose();
                    picProfileAvatar.Image = Image.FromFile(avatarPath);
                }
            }

            pnlProfilePage.Visible = true;
            pnlProfilePage.BringToFront();
        }

        private void AddProfileField(Panel parent, string label, string value, int y)
        {
            Label lbl = new Label {
                Text = label,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(50, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);

            Label val = new Label {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(50, y + 20),
                AutoSize = true
            };
            parent.Controls.Add(val);
        }

        private void AddProfileFieldCompact(Panel parent, string label, string value, int x, int y)
        {
            Label lbl = new Label {
                Text = label,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);

            Label val = new Label {
                Text = value,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(x, y + 22),
                AutoSize = true
            };
            parent.Controls.Add(val);
        }

        private void AddStatItem(Panel parent, string icon, string label, string value, int x)
        {
            Label lblIcon = new Label {
                Text = icon,
                Font = new Font("Segoe UI", 16),
                Location = new Point(x + 10, 10),
                Size = new Size(35, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            parent.Controls.Add(lblIcon);

            Label lblLabel = new Label {
                Text = label,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(x + 45, 10),
                AutoSize = true
            };
            parent.Controls.Add(lblLabel);

            Label lblValue = new Label {
                Text = value,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(x + 45, 28),
                AutoSize = true
            };
            parent.Controls.Add(lblValue);
        }

        private string GetAvatarPath()
        {
            string folder = System.IO.Path.Combine(Application.StartupPath, "avatars");
            if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
            // Use username (email) as filename, replace invalid chars
            string safeName = MyUsername.Replace("@", "_").Replace(".", "_");
            return System.IO.Path.Combine(folder, $"{safeName}.png");
        }

        private void UploadAvatar()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh đại diện";
                ofd.Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string avatarPath = GetAvatarPath();
                        
                        // Đọc file vào memory stream để không khóa file gốc
                        byte[] imageBytes = System.IO.File.ReadAllBytes(ofd.FileName);
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        using (Image img = Image.FromStream(ms))
                        {
                            // Resize to 140x140
                            using (Bitmap resized = new Bitmap(140, 140))
                            {
                                using (Graphics g = Graphics.FromImage(resized))
                                {
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    g.DrawImage(img, 0, 0, 140, 140);
                                }
                                
                                // Dispose ảnh cũ trong PictureBox trước khi lưu file mới
                                if (picProfileAvatar.Image != null)
                                {
                                    var oldImage = picProfileAvatar.Image;
                                    picProfileAvatar.Image = null;
                                    oldImage.Dispose();
                                }
                                
                                // Xóa file cũ nếu tồn tại
                                if (System.IO.File.Exists(avatarPath))
                                {
                                    System.IO.File.Delete(avatarPath);
                                }
                                
                                // Lưu file mới
                                resized.Save(avatarPath, System.Drawing.Imaging.ImageFormat.Png);
                                
                                // Đọc lại file vừa lưu để hiển thị (tránh khóa file)
                                byte[] savedBytes = System.IO.File.ReadAllBytes(avatarPath);
                                using (MemoryStream msDisplay = new MemoryStream(savedBytes))
                                {
                                    picProfileAvatar.Image = Image.FromStream(msDisplay);
                                }
                            }
                        }
                        
                        MessageBox.Show("Đã cập nhật ảnh đại diện!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi tải ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ShowSearchPage()
        {
            if (isSidebarOpen) ToggleSidebar();
            isViewingUsers = false; 

            if (pnlSearchPage == null)
            {
                pnlSearchPage = new Panel { Size = this.ClientSize, BackColor = Color.FromArgb(240, 245, 250), Location = new Point(0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
                this.Controls.Add(pnlSearchPage);
                
                // 1. Header Card
                Panel head = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White };
                pnlSearchPage.Controls.Add(head);
                Label title = new Label { Text = "TRA CỨU", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 18), AutoSize = true };
                head.Controls.Add(title);

                // 2. Filter Card
                Panel filterArea = new Panel { Location = new Point(20, 90), Size = new Size(pnlSearchPage.Width - 40, 120), BackColor = Color.White };
                filterArea.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                filterArea.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, filterArea.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlSearchPage.Controls.Add(filterArea);

                Label l1 = new Label { Text = "Lọc theo lớp học", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
                cbClasses = new ComboBox { Location = new Point(20, 45), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
                cbClasses.Items.Add("TẤT CẢ");
                cbClasses.SelectedIndex = 0;
                filterArea.Controls.Add(l1);
                filterArea.Controls.Add(cbClasses);
                
                cbClasses.SelectedIndexChanged += (s, ev) => {
                    string selected = cbClasses.SelectedItem.ToString();
                    if (selected == "TẤT CẢ") SocketClient.Send("LIST");
                    else SocketClient.Send($"SEARCH|CLASS|{selected}");
                };

                Label l2 = new Label { Text = "Từ khóa tìm kiếm (Tên/MSSV)", Location = new Point(220, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
                TextBox tSearch = new TextBox { Location = new Point(220, 45), Width = 300, Font = new Font("Segoe UI", 11), PlaceholderText = "Ví dụ: Nguyễn Văn A..." };
                filterArea.Controls.Add(l2);
                filterArea.Controls.Add(tSearch);
                
                Button bFind = new Button { 
                    Text = "TÌM KIẾM NGAY", 
                    Location = new Point(540, 43), 
                    Size = new Size(150, 40), 
                    BackColor = Color.FromArgb(0, 120, 215), 
                    ForeColor = Color.White, 
                    FlatStyle = FlatStyle.Flat, 
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                bFind.FlatAppearance.BorderSize = 0;
                bFind.Click += (s, ev) => SocketClient.Send($"SEARCH|ALL|{tSearch.Text.Trim()}");
                filterArea.Controls.Add(bFind);

                // 3. Results Card
                Panel gridCard = new Panel {
                    Location = new Point(20, 230),
                    Size = new Size(pnlSearchPage.Width - 40, 410),
                    BackColor = Color.White,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };
                gridCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, gridCard.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlSearchPage.Controls.Add(gridCard);

                dgvSearchResults = new DataGridView { 
                    Dock = DockStyle.Fill, 
                    BackgroundColor = Color.White, 
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    RowTemplate = { Height = 40 }
                };
                dgvSearchResults.ColumnHeadersHeight = 45;
                dgvSearchResults.EnableHeadersVisualStyles = false;
                dgvSearchResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                dgvSearchResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                dgvSearchResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                gridCard.Controls.Add(dgvSearchResults);

                DataTable dtRes = new DataTable();
                dtRes.Columns.Add("MSSV"); dtRes.Columns.Add("Họ Tên"); dtRes.Columns.Add("Lớp");
                dgvSearchResults.DataSource = dtRes;

                // 4. Back Button
                Button bBack = new Button { 
                    Text = "← QUAY LẠI TRẠNG CHỦ", 
                    Location = new Point(20, 660), 
                    Size = new Size(220, 45), 
                    FlatStyle = FlatStyle.Flat, 
                    BackColor = Color.FromArgb(71, 85, 105), 
                    ForeColor = Color.White, 
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                    Cursor = Cursors.Hand
                };
                bBack.FlatAppearance.BorderSize = 0;
                bBack.Click += (s, ev) => ShowDashboard();
                pnlSearchPage.Controls.Add(bBack);
            }
            pnlSearchPage.Visible = true;
            pnlSearchPage.BringToFront();
            SocketClient.Send("LIST_CLASSES");
            SocketClient.Send("LIST"); 
        }
    }
}
