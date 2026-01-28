using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinClient
{
    public partial class MainForm
    {
        private void InitCustomUI()
        {
            this.Text = "QUẢN LÝ SINH VIÊN";
            this.Size = new Size(1300, 780);
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Chế độ FixedDialog
            this.MaximizeBox = false; // Vô hiệu hóa phóng to
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 250); // Nền xám xanh nhẹ hiện đại
            this.Font = new Font("Segoe UI", 10);

            SetupHeader();
            InitSidebar();
            SetupMainLayout();
            SetupChatUI();
        }

        private void SetupHeader()
        {
            Panel pnlHeader = new Panel { 
                Dock = DockStyle.Top, 
                Height = 75, 
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(pnlHeader);

            // Shadow effect for header
            pnlHeader.Paint += (s, e) => {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 230, 230), 2), 0, 74, pnlHeader.Width, 74);
            };

            Button btnMenu = new Button {
                Text = "☰",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Location = new Point(10, 12),
                Cursor = Cursors.Hand,
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Click += (s, e) => ToggleSidebar();
            pnlHeader.Controls.Add(btnMenu);

            Label lblTitle = new Label {
                Text = "QUẢN LÝ SINH VIÊN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(70, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            lblWelcome = new Label {
                Text = $"Chào bạn, {MyFullName}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Right,
                Width = 400,
                Padding = new Padding(0, 28, 20, 0),
                TextAlign = ContentAlignment.TopRight
            };
            pnlHeader.Controls.Add(lblWelcome);
            lblWelcome.BringToFront();
        }

        private void SetupMainLayout()
        {
            // Center area - Card style
            pnlInputForm = new Panel { 
                Location = new Point(20, 95), 
                Size = new Size(920, 300), // Height 300 for 2 rows + buttons
                BackColor = Color.White
            };
            pnlInputForm.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlInputForm.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlInputForm);

            lblSectionTitle = new Label { Text = "THÔNG TIN CHI TIẾT SINH VIÊN", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) };
            pnlInputForm.Controls.Add(lblSectionTitle);

            // Row 1
            txtID = new TextBox { PlaceholderText = "Ví dụ: SV001" };
            txtName = new TextBox { PlaceholderText = "Nhập họ và tên đầy đủ" };
            txtClass = new TextBox { PlaceholderText = "Ví dụ: CNTT1" };
            
            lblID = AddInputPairWithLabel(pnlInputForm, "Mã Định Danh (MSSV):", txtID, 20, 45, 70, 260);
            lblName = AddInputPairWithLabel(pnlInputForm, "Họ và Tên:", txtName, 310, 45, 70, 300);
            lblClass = AddInputPairWithLabel(pnlInputForm, "Lớp Học:", txtClass, 640, 45, 70, 250);

            // Row 2
            txtPhone = new TextBox { PlaceholderText = "09xxxxxxxx" };
            txtEmail = new TextBox { PlaceholderText = "example@email.com" };
            txtSubject = new TextBox { PlaceholderText = "Môn học..." };

            lblPhone = AddInputPairWithLabel(pnlInputForm, "Số Điện Thoại:", txtPhone, 20, 105, 130, 260);
            lblEmail = AddInputPairWithLabel(pnlInputForm, "Địa Chỉ Email:", txtEmail, 310, 105, 130, 300);
            lblSubject = AddInputPairWithLabel(pnlInputForm, "Môn Học:", txtSubject, 640, 105, 130, 250);

            // Action Buttons
            int bx = 20, by = 180, bw = 120, bh = 45, gap = 135;
            
            btnAdd = CreateActionButton("THÊM MỚI", Color.FromArgb(16, 185, 129), bx, by, bw, bh, btnAdd_Click);
            btnUpdate = CreateActionButton("CẬP NHẬT", Color.FromArgb(37, 99, 235), bx += gap, by, bw, bh, btnUpdate_Click);
            btnDelete = CreateActionButton("XÓA BỎ", Color.FromArgb(220, 53, 69), bx += gap, by, bw, bh, btnDelete_Click);
            btnRefresh = CreateActionButton("LÀM MỚI", Color.FromArgb(255, 193, 7), bx += gap, by, bw, bh, btnRefresh_Click);
            btnImport = CreateActionButton("NHẬP EXCEL", Color.SeaGreen, bx += gap, by, bw, bh, btnImport_Click);

            pnlInputForm.Controls.Add(btnAdd); pnlInputForm.Controls.Add(btnUpdate);
            pnlInputForm.Controls.Add(btnDelete); pnlInputForm.Controls.Add(btnRefresh);
            pnlInputForm.Controls.Add(btnImport);



            // Data Table Card
            Panel pnlTable = new Panel {
                Location = new Point(20, 410), // Moved down
                Size = new Size(920, 295),
                BackColor = Color.White
            };
            pnlTable.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTable.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlTable);

            dgvStudents = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.White, 
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowTemplate = { Height = 40 }
            };
            dgvStudents.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvStudents.ColumnHeadersHeight = 45;
            dgvStudents.EnableHeadersVisualStyles = false;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvStudents.CellClick += dgvStudents_CellClick;
            pnlTable.Controls.Add(dgvStudents);
        }

        private void SetupChatUI()
        {
            // Di chuyển vị trí xuống Y=95 (ngang hàng với pnlMain) và tăng chiều rộng
            Panel pnlChat = new Panel { 
                Location = new Point(960, 95), 
                Size = new Size(310, 610), // Kéo dài xuống dưới
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle 
            };
            this.Controls.Add(pnlChat);

            // 1. Tiêu đề (Vị trí cố định trên cùng)
            Label lblChat = new Label { 
                Text = "CỘNG ĐỒNG THẢO LUẬN", 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                Location = new Point(0, 0),
                Size = new Size(315, 35),
                TextAlign = ContentAlignment.MiddleCenter, 
                BackColor = Color.FromArgb(235, 240, 250),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            pnlChat.Controls.Add(lblChat);

            // 2. Khung nội dung (Bắt đầu từ Y=35, chiều cao tự động co giãn)
            rtbChat = new RichTextBox { 
                Location = new Point(5, 40),
                Size = new Size(300, 510), // Tăng chiều cao lên
                BorderStyle = BorderStyle.None, 
                BackColor = Color.FromArgb(252, 252, 252), 
                ReadOnly = true, 
                Font = new Font("Segoe UI", 10)
            };
            pnlChat.Controls.Add(rtbChat);

            // 3. Khu vực nhập liệu (Nằm ở đáy)
            Panel pnlInput = new Panel { 
                Location = new Point(0, 555), // Đẩy xuống đáy mới
                Size = new Size(310, 55),
                BackColor = Color.FromArgb(245, 245, 245)
            };
            pnlChat.Controls.Add(pnlInput);

            btnSend = new Button { 
                Text = "GỬI", 
                Location = new Point(255, 10),
                Size = new Size(55, 35),
                BackColor = Color.FromArgb(0, 120, 215), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            txtChatInput = new TextBox { 
                Location = new Point(5, 12),
                Width = 245,
                Font = new Font("Segoe UI", 11), 
                PlaceholderText = "Nhập tin nhắn..." 
            };
            txtChatInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnSend_Click(s, e); } };
            pnlInput.Controls.Add(txtChatInput);

            pnlChat.BringToFront();
        }

        private void AddInputPair(Control parent, string text, TextBox tb, int x, int ly, int ty, int w)
        {
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, ly), AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(100, 116, 139) });
            tb.Location = new Point(x, ty); tb.Width = w; tb.Font = new Font("Segoe UI", 10);
            parent.Controls.Add(tb);
        }

        private Label AddInputPairWithLabel(Control parent, string text, TextBox tb, int x, int ly, int ty, int w)
        {
            Label lbl = new Label { Text = text, Location = new Point(x, ly), AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(100, 116, 139) };
            parent.Controls.Add(lbl);
            tb.Location = new Point(x, ty); tb.Width = w; tb.Font = new Font("Segoe UI", 10);
            parent.Controls.Add(tb);
            return lbl;
        }

        private Button CreateActionButton(string text, Color backColor, int x, int y, int w, int h, EventHandler click)
        {
            Button btn = new Button {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;
            return btn;
        }
    }
}
