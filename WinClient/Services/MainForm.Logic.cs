using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;

namespace WinClient
{
    public partial class MainForm
    {
        private void InitTable()
        {
            dtStudents = new DataTable();
            dtStudents.Columns.Add("MSSV");
            dtStudents.Columns.Add("Họ Tên");
            dtStudents.Columns.Add("Lớp");
            dtStudents.Columns.Add("SĐT");
            dtStudents.Columns.Add("Email");
            dtStudents.Columns.Add("Môn"); // Môn học
            dgvStudents.DataSource = dtStudents;
        }

        public void MainForm_Load(object sender, EventArgs e)
        {
            if (!SocketClient.Connect())
            {
                MessageBox.Show("Mất kết nối với máy chủ!", "Lỗi");
                this.Close();
                return;
            }

            isListening = true;
            Task.Run(() => ListenToServer());
            SocketClient.Send("LIST");
        }

        private void ListenToServer()
        {
            while (isListening)
            {
                try
                {
                    string msg = SocketClient.Receive();
                    if (!string.IsNullOrEmpty(msg)) this.Invoke(new Action(() => ProcessMessage(msg)));
                }
                catch { break; }
            }
        }

        private void ProcessMessage(string msg)
        {
            string[] parts = msg.Split('|');
            string cmd = parts[0];

            switch (cmd)
            {
                case "REFRESH":
                    LogSystem("Cập nhật: Dữ liệu hệ thống đã thay đổi.");
                    RefreshData();
                    if (pnlSearchPage?.Visible == true) SocketClient.Send("LIST_CLASSES");
                    break;

                case "CHAT":
                    if (parts.Length >= 3) LogChat(parts[1], parts[2]);
                    break;

                case "LIST_RES":
                    UpdateGrid(msg.Substring(9));
                    break;

                case "LIST_USERS_RES":
                    UpdateUserGrid(msg.Substring(15));
                    break;
                
                case "LIST_CLASSES_RES":
                    UpdateClassFilter(msg.Substring(17));
                    break;

                case "ADD_SUCCESS": LogSystem("Thành công: Đã thêm sinh viên mới."); ClearInputs(); break;
                case "UPDATE_SUCCESS": LogSystem("Thành công: Đã cập nhật thông tin."); ClearInputs(); break;
                case "DELETE_SUCCESS": LogSystem("Thành công: Đã xóa sinh viên."); ClearInputs(); break;
                
                case "CREATE_USER_SUCCESS": 
                    MessageBox.Show("Đã cấp tài khoản thành công!"); 
                    RefreshData(); 
                    break;
                case "UPDATE_USER_SUCCESS": 
                    MessageBox.Show("Cập nhật tài khoản thành công!"); 
                    RefreshData(); 
                    break;
                case "DELETE_USER_SUCCESS": 
                    MessageBox.Show("Đã xóa tài khoản."); 
                    RefreshData(); 
                    break;

                case "EXISTS": MessageBox.Show("Lỗi: Mã này đã tồn tại!"); break;
                case "STUDENT_NOT_FOUND": 
                    LogSystem("Thông báo: Không tìm thấy kết quả."); 
                    if (pnlSearchPage?.Visible == true) ((DataTable)dgvSearchResults.DataSource).Clear();
                    break;
            }
        }

        private void UpdateGrid(string data)
        {
            if (isViewingUsers) return;
            
            bool isSearchPage = (pnlSearchPage?.Visible == true);
            var targetDT = isSearchPage ? (DataTable)dgvSearchResults.DataSource : dtStudents;
            targetDT.Clear();
            
            foreach (var row in data.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] p = row.Split('#');
                
                if (isSearchPage)
                {
                    // Search page only has 3 columns: MSSV, Họ Tên, Lớp
                    if (p.Length >= 3) targetDT.Rows.Add(p[0], p[1], p[2]);
                }
                else
                {
                    // Main page has 6 columns
                    if (p.Length >= 6)
                    {
                        string sdt = string.IsNullOrWhiteSpace(p[3]) ? "Chưa cập nhật" : p[3];
                        string email = string.IsNullOrWhiteSpace(p[4]) ? "Chưa cập nhật" : p[4];
                        string subject = string.IsNullOrWhiteSpace(p[5]) ? "Chưa cập nhật" : p[5];
                        targetDT.Rows.Add(p[0], p[1], p[2], sdt, email, subject);
                    }
                    else if (p.Length >= 3)
                    {
                        targetDT.Rows.Add(p[0], p[1], p[2], "Chưa cập nhật", "Chưa cập nhật", "Chưa cập nhật");
                    }
                }
            }
        }

        private void UpdateUserGrid(string data)
        {
            if (!isViewingUsers) return;
            dtStudents.Clear();
            foreach (var row in data.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] p = row.Split('#');
                if (p.Length >= 3) 
                {
                    string email = p[0];
                    string name = p[1];
                    string role = p[2];
                    string contactEmail = p.Length > 3 && !string.IsNullOrWhiteSpace(p[3]) ? p[3] : "Chưa cập nhật";
                    string assignedClass = p.Length > 4 && !string.IsNullOrWhiteSpace(p[4]) ? p[4] : "Chưa cập nhật";
                    string subject = p.Length > 5 && !string.IsNullOrWhiteSpace(p[5]) ? p[5] : "Chưa cập nhật";
                    dtStudents.Rows.Add(email, name, role, contactEmail, assignedClass, subject);
                }
            }
            dgvStudents.Refresh();
        }

        private void UpdateClassFilter(string data)
        {
            if (cbClasses == null) return;
            string current = cbClasses.SelectedItem?.ToString() ?? "TẤT CẢ";
            cbClasses.Items.Clear();
            cbClasses.Items.Add("TẤT CẢ");
            foreach (var cls in data.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(cls)) cbClasses.Items.Add(cls.Trim());
            }
            
            int idx = cbClasses.Items.IndexOf(current);
            cbClasses.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void RefreshData()
        {
            SocketClient.Send(isViewingUsers ? "LIST_USERS" : "LIST");
        }

        private void ClearInputs()
        {
            txtID.Text = ""; txtName.Text = ""; txtClass.Text = "";
            txtPhone.Text = ""; txtEmail.Text = ""; txtSubject.Text = "";
            txtID.Enabled = true;
        }

        // --- Event Handlers ---

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (isViewingUsers) return;
            string id = txtID.Text.Trim();
            if (!id.StartsWith("SV")) id = "SV" + id;
            SocketClient.Send($"ADD|{id}|{txtName.Text.Trim()}|{txtClass.Text.Trim()}|{txtPhone.Text.Trim()}|{txtEmail.Text.Trim()}|{txtSubject.Text.Trim()}");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (isViewingUsers)
            {
                string target = txtID.Text.Trim();
                if (string.IsNullOrEmpty(target)) { MessageBox.Show("Vui lòng chọn tài khoản cần cập nhật."); return; }

                Form f = new Form { 
                    Size = new Size(600, 700), 
                    Text = "Cập nhật Tài Khoản", 
                    StartPosition = FormStartPosition.CenterParent, 
                    FormBorderStyle = FormBorderStyle.FixedDialog, 
                    MaximizeBox = false, 
                    MinimizeBox = false, 
                    BackColor = Color.White, 
                    Font = new Font("Segoe UI", 10),
                    Padding = new Padding(20)
                };
                
                int w = 520, x = (f.ClientSize.Width - w) / 2, y = 30;

                // Title
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "CẬP NHẬT THÔNG TIN TÀI KHOẢN", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) });
                y += 55;

                // Họ Tên
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Họ và Tên:", Font = new Font("Segoe UI", 10) });
                TextBox tName = new TextBox { Left = x, Top = y + 28, Width = w, Text = txtName.Text, Font = new Font("Segoe UI", 12) }; f.Controls.Add(tName);
                y += 80;
                
                // Mật khẩu mới
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Mật khẩu mới:", Font = new Font("Segoe UI", 10) });
                TextBox tPass = new TextBox { Left = x, Top = y + 28, Width = w, PlaceholderText = "Để trống nếu không đổi", Font = new Font("Segoe UI", 12) }; f.Controls.Add(tPass);
                y += 80;
                
                // Vai trò
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Vai trò:", Font = new Font("Segoe UI", 10) });
                ComboBox cbRole = new ComboBox { Left = x, Top = y + 28, Width = w, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12) };
                cbRole.Items.AddRange(new[] { "USER", "ADMIN" });
                cbRole.SelectedItem = txtClass.Text.Trim().ToUpper() == "ADMIN" ? "ADMIN" : "USER";
                f.Controls.Add(cbRole);
                y += 80;

                // Email Liên Hệ
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Email Liên Hệ:", Font = new Font("Segoe UI", 10) });
                TextBox tContactEmail = new TextBox { Left = x, Top = y + 28, Width = w, Text = txtPhone.Text, Font = new Font("Segoe UI", 12) }; f.Controls.Add(tContactEmail);
                y += 80;

                // Lớp Dạy
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Lớp Dạy:", Font = new Font("Segoe UI", 10) });
                TextBox tClass = new TextBox { Left = x, Top = y + 28, Width = w, Text = txtEmail.Text, Font = new Font("Segoe UI", 12) }; f.Controls.Add(tClass);
                y += 80;

                // Môn Dạy
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Môn Dạy:", Font = new Font("Segoe UI", 10) });
                TextBox tSubject = new TextBox { Left = x, Top = y + 28, Width = w, Text = txtSubject.Text, Font = new Font("Segoe UI", 12) }; f.Controls.Add(tSubject);
                y += 90;
                
                Button bOk = new Button { 
                    Left = x, Top = y, Width = w, Height = 50, 
                    Text = "LƯU THAY ĐỔI", 
                    BackColor = Color.FromArgb(0, 120, 215), 
                    ForeColor = Color.White, 
                    FlatStyle = FlatStyle.Flat, 
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                bOk.FlatAppearance.BorderSize = 0;
                bOk.Click += (s, ev) => { 
                    SocketClient.Send($"UPDATE_USER|{target}|{tPass.Text}|{cbRole.SelectedItem}|{tName.Text}|{tContactEmail.Text}|{tClass.Text}|{tSubject.Text}"); 
                    f.Close(); 
                };
                f.Controls.Add(bOk);
                f.ShowDialog();
                return;
            }
            SocketClient.Send($"UPDATE|{txtID.Text.Trim()}|{txtName.Text.Trim()}|{txtClass.Text.Trim()}|{txtPhone.Text.Trim()}|{txtEmail.Text.Trim()}|{txtSubject.Text.Trim()}");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string id = txtID.Text.Trim();
            if (string.IsNullOrEmpty(id)) return;
            
            string cmd = isViewingUsers ? "DELETE_USER" : "DELETE";
            if (MessageBox.Show($"Bạn chắc chắn muốn xóa {id}?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                SocketClient.Send($"{cmd}|{id}");
        }

        private void btnRefresh_Click(object sender, EventArgs e) => RefreshData();

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (isViewingUsers) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file Excel để nhập dữ liệu sinh viên";
                ofd.Filter = "Excel Files|*.xlsx;*.xls";
                ofd.FilterIndex = 1;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Thiết lập License cho EPPlus (NonCommercial = miễn phí)
                    OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                    using (var package = new OfficeOpenXml.ExcelPackage(new FileInfo(ofd.FileName)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            MessageBox.Show("Không tìm thấy sheet nào trong file Excel!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        if (rowCount < 2)
                        {
                            MessageBox.Show("File Excel không có dữ liệu hoặc chỉ có tiêu đề!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Hiển thị thông báo xác nhận
                        int dataRows = rowCount - 1; // Trừ dòng tiêu đề
                        DialogResult confirm = MessageBox.Show(
                            $"File chứa {dataRows} sinh viên.\n\n" +
                            "Cấu trúc cột yêu cầu:\n" +
                            "Cột A: MSSV\n" +
                            "Cột B: Họ Tên\n" +
                            "Cột C: Lớp\n" +
                            "Cột D: Số Điện Thoại\n" +
                            "Cột E: Email\n" +
                            "Cột F: Môn Học\n\n" +
                            "Bạn có muốn tiếp tục nhập?",
                            "Xác nhận nhập dữ liệu",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (confirm != DialogResult.Yes) return;

                        int successCount = 0;
                        int errorCount = 0;
                        System.Text.StringBuilder errors = new System.Text.StringBuilder();

                        // Duyệt từ dòng 2 (bỏ qua tiêu đề)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            string mssv = worksheet.Cells[row, 1].Text?.Trim() ?? "";
                            string hoTen = worksheet.Cells[row, 2].Text?.Trim() ?? "";
                            string lop = worksheet.Cells[row, 3].Text?.Trim() ?? "";
                            string sdt = worksheet.Cells[row, 4].Text?.Trim() ?? "";
                            string email = worksheet.Cells[row, 5].Text?.Trim() ?? "";
                            string mon = worksheet.Cells[row, 6].Text?.Trim() ?? "";

                            // Bỏ qua dòng trống
                            if (string.IsNullOrEmpty(mssv) && string.IsNullOrEmpty(hoTen)) continue;

                            // Kiểm tra dữ liệu bắt buộc
                            if (string.IsNullOrEmpty(mssv) || string.IsNullOrEmpty(hoTen))
                            {
                                errorCount++;
                                errors.AppendLine($"Dòng {row}: Thiếu MSSV hoặc Họ Tên");
                                continue;
                            }

                            // Thêm tiền tố SV nếu chưa có
                            if (!mssv.StartsWith("SV", StringComparison.OrdinalIgnoreCase))
                                mssv = "SV" + mssv;

                            // Gửi lệnh ADD qua socket
                            SocketClient.Send($"ADD|{mssv}|{hoTen}|{lop}|{sdt}|{email}|{mon}");
                            successCount++;

                            // Delay nhỏ để tránh overload server
                            System.Threading.Thread.Sleep(50);
                        }

                        // Hiển thị kết quả
                        string resultMsg = $"Hoàn tất nhập dữ liệu!\n\n";
                        resultMsg += $"✓ Thành công: {successCount} sinh viên\n";
                        if (errorCount > 0)
                        {
                            resultMsg += $"✗ Lỗi: {errorCount} dòng\n\n";
                            resultMsg += "Chi tiết lỗi:\n" + errors.ToString();
                        }

                        MessageBox.Show(resultMsg, "Kết quả Import", MessageBoxButtons.OK,
                            errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                        LogSystem($"Import Excel: {successCount} thành công, {errorCount} lỗi");

                        // Refresh danh sách
                        RefreshData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file Excel:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvStudents.Rows[e.RowIndex];
            txtID.Text = row.Cells[0].Value?.ToString();
            
            if (isViewingUsers)
            {
                // User mode: Email(0), HoTen(1), Quyen(2), EmailLienHe(3), LopDay(4), MonDay(5)
                txtName.Text = row.Cells[1].Value?.ToString();
                txtClass.Text = row.Cells[2].Value?.ToString();  // Quyền
                if (row.Cells.Count > 3) txtPhone.Text = row.Cells[3].Value?.ToString();  // Email liên hệ
                if (row.Cells.Count > 4) txtEmail.Text = row.Cells[4].Value?.ToString();  // Lớp dạy
                if (row.Cells.Count > 5) txtSubject.Text = row.Cells[5].Value?.ToString();  // Môn dạy
                txtID.Enabled = false;
            }
            else
            {
                // Student mode: MSSV(0), HoTen(1), Lop(2), SDT(3), Email(4), Mon(5)
                txtName.Text = row.Cells[1].Value?.ToString();
                txtClass.Text = row.Cells[2].Value?.ToString();
                if (row.Cells.Count > 3) txtPhone.Text = row.Cells[3].Value?.ToString();
                if (row.Cells.Count > 4) txtEmail.Text = row.Cells[4].Value?.ToString();
                if (row.Cells.Count > 5) txtSubject.Text = row.Cells[5].Value?.ToString();
                txtID.Enabled = false;
            }
        }
        
        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtChatInput.Text)) return;
            SocketClient.Send($"CHAT|{MyFullName}|{txtChatInput.Text}");
            txtChatInput.Text = "";
        }

        private void LogSystem(string content)
        {
            if (rtbChat.IsDisposed) return;
            string time = DateTime.Now.ToString("HH:mm:ss");
            rtbChat.Invoke(new Action(() => {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = Color.Red;
                rtbChat.AppendText($"[{time}] Hệ thống: {content}\n");
                rtbChat.ScrollToCaret();
            }));
        }

        private void LogChat(string user, string content)
        {
            if (rtbChat.IsDisposed) return;
            bool isMe = (user == MyFullName || user == MyUsername);
            rtbChat.Invoke(new Action(() => {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = isMe ? Color.Blue : Color.DarkGreen;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Bold);
                rtbChat.AppendText($"{(isMe ? "Tôi" : user)}: ");
                rtbChat.SelectionColor = Color.Black;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Regular);
                rtbChat.AppendText($"{content}\n");
                rtbChat.ScrollToCaret();
            }));
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            isViewingUsers = !isViewingUsers;
            dgvStudents.DataSource = null; 
            dtStudents.Rows.Clear();
            dtStudents.Columns.Clear();

            if (isViewingUsers)
            {
                if (btn != null) { btn.Text = "XEM SINH VIÊN"; btn.BackColor = Color.DarkOrange; }
                dtStudents.Columns.Add("Email"); 
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Quyền");
                dtStudents.Columns.Add("Email Liên Hệ");
                dtStudents.Columns.Add("Lớp Dạy");
                dtStudents.Columns.Add("Môn Dạy");
                SocketClient.Send("LIST_USERS");
                
                // Update section title and labels for User mode
                lblSectionTitle.Text = "THÔNG TIN TÀI KHOẢN GIÁO VIÊN";
                lblID.Text = "Email Đăng Nhập:";
                lblName.Text = "Họ và Tên:";
                lblClass.Text = "Quyền:";
                txtID.PlaceholderText = "vidu@school.edu.vn";
                txtClass.PlaceholderText = "USER / ADMIN";
                
                // Update row 2 labels for User mode
                lblPhone.Text = "Email Liên Hệ:";
                lblEmail.Text = "Lớp Dạy:";
                lblSubject.Text = "Môn Dạy:";
                txtPhone.PlaceholderText = "lienhe@gmail.com";
                txtEmail.PlaceholderText = "CNTT1, CNTT2";
                txtSubject.PlaceholderText = "Lập trình C#";
                
                // Hide/Update buttons for User mode
                btnAdd.Visible = false;
                btnImport.Visible = false;
                
                // Reposition visible buttons for better alignment
                btnUpdate.Text = "SỬA TÀI KHOẢN";
                btnUpdate.Width = 150;
                btnUpdate.Location = new Point(20, btnUpdate.Location.Y);
                
                btnDelete.Text = "XÓA TÀI KHOẢN";
                btnDelete.Width = 150;
                btnDelete.Location = new Point(185, btnDelete.Location.Y);
                
                btnRefresh.Location = new Point(350, btnRefresh.Location.Y);
            }
            else
            {
                if (btn != null) { btn.Text = "XEM TÀI KHOẢN"; btn.BackColor = Color.Teal; }
                dtStudents.Columns.Add("MSSV"); 
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Lớp");
                dtStudents.Columns.Add("SĐT");
                dtStudents.Columns.Add("Email");
                dtStudents.Columns.Add("Môn");
                SocketClient.Send("LIST");
                
                // Restore section title and labels for Student mode
                lblSectionTitle.Text = "THÔNG TIN CHI TIẾT SINH VIÊN";
                lblID.Text = "Mã Định Danh (MSSV):";
                lblName.Text = "Họ và Tên:";
                lblClass.Text = "Lớp Học:";
                txtID.PlaceholderText = "Ví dụ: SV001";
                txtClass.PlaceholderText = "Ví dụ: CNTT1";
                
                // Restore row 2 labels for Student mode
                lblPhone.Text = "Số Điện Thoại:";
                lblEmail.Text = "Địa Chỉ Email:";
                lblSubject.Text = "Môn Học:";
                txtPhone.PlaceholderText = "09xxxxxxxx";
                txtEmail.PlaceholderText = "example@email.com";
                txtSubject.PlaceholderText = "Môn học...";
                
                // Restore buttons for Student mode
                btnAdd.Visible = true;
                btnImport.Visible = true;
                
                // Restore original positions
                int bx = 20, by = btnAdd.Location.Y, gap = 135;
                btnAdd.Location = new Point(bx, by);
                btnUpdate.Text = "CẬP NHẬT";
                btnUpdate.Width = 120;
                btnUpdate.Location = new Point(bx + gap, by);
                btnDelete.Text = "XÓA BỎ";
                btnDelete.Width = 120;
                btnDelete.Location = new Point(bx + gap * 2, by);
                btnRefresh.Location = new Point(bx + gap * 3, by);
                btnImport.Location = new Point(bx + gap * 4, by);
            }
            dgvStudents.DataSource = dtStudents;
            txtID.Enabled = true;
            ClearInputs();
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            Form f = new Form { 
                Size = new Size(550, 450), 
                Text = "Cấp Tài Khoản Giáo Viên", 
                StartPosition = FormStartPosition.CenterParent, 
                FormBorderStyle = FormBorderStyle.FixedDialog, 
                MaximizeBox = false, 
                MinimizeBox = false, 
                BackColor = Color.White, 
                Font = new Font("Segoe UI", 10) 
            };
            
            int padding = 30;
            int colW = 230; // Column width
            int gap = 25;   // Gap between columns

            // Header
            Panel pnlHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(0, 120, 215) };
            f.Controls.Add(pnlHead);
            Label lblHeader = new Label { 
                Text = "THÔNG TIN GIÁO VIÊN MỚI", 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                ForeColor = Color.White, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleCenter 
            };
            pnlHead.Controls.Add(lblHeader);

            int y = 80;

            // Row 1: Email đăng nhập + Mật khẩu (2 columns)
            f.Controls.Add(new Label { Left = padding, Top = y, AutoSize = true, Text = "Email đăng nhập:", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tEmail = new TextBox { Left = padding, Top = y + 22, Width = colW, PlaceholderText = "vidu@school.edu.vn" }; 
            f.Controls.Add(tEmail);

            f.Controls.Add(new Label { Left = padding + colW + gap, Top = y, AutoSize = true, Text = "Mật khẩu:", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tPass = new TextBox { Left = padding + colW + gap, Top = y + 22, Width = colW, PlaceholderText = "••••••••" }; 
            f.Controls.Add(tPass);

            y += 65;

            // Row 2: Họ tên + Email liên hệ (2 columns)
            f.Controls.Add(new Label { Left = padding, Top = y, AutoSize = true, Text = "Họ và Tên:", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tName = new TextBox { Left = padding, Top = y + 22, Width = colW, PlaceholderText = "Nguyễn Văn A" }; 
            f.Controls.Add(tName);

            f.Controls.Add(new Label { Left = padding + colW + gap, Top = y, AutoSize = true, Text = "Email liên hệ (tuỳ chọn):", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tContact = new TextBox { Left = padding + colW + gap, Top = y + 22, Width = colW, PlaceholderText = "lienhe@gmail.com" }; 
            f.Controls.Add(tContact);

            y += 65;

            // Row 3: Lớp dạy + Môn dạy (2 columns)
            f.Controls.Add(new Label { Left = padding, Top = y, AutoSize = true, Text = "Lớp dạy:", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tClass = new TextBox { Left = padding, Top = y + 22, Width = colW, PlaceholderText = "CNTT1, CNTT2" }; 
            f.Controls.Add(tClass);

            f.Controls.Add(new Label { Left = padding + colW + gap, Top = y, AutoSize = true, Text = "Môn dạy:", ForeColor = Color.FromArgb(100, 116, 139) });
            TextBox tSubject = new TextBox { Left = padding + colW + gap, Top = y + 22, Width = colW, PlaceholderText = "Lập trình C#" }; 
            f.Controls.Add(tSubject);

            y += 80;

            // Buttons row
            Button bCancel = new Button { 
                Left = padding, Top = y, Width = colW, Height = 42, 
                Text = "HỦY BỎ", 
                BackColor = Color.FromArgb(241, 245, 249), 
                ForeColor = Color.FromArgb(71, 85, 105), 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            bCancel.FlatAppearance.BorderSize = 0;
            bCancel.Click += (s, ev) => f.Close();
            f.Controls.Add(bCancel);

            Button bOk = new Button { 
                Left = padding + colW + gap, Top = y, Width = colW, Height = 42, 
                Text = "TẠO TÀI KHOẢN", 
                BackColor = Color.FromArgb(0, 120, 215), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Cursor = Cursors.Hand 
            };
            bOk.FlatAppearance.BorderSize = 0;
            bOk.Click += (s, ev) => {
                if(string.IsNullOrEmpty(tEmail.Text) || string.IsNullOrEmpty(tPass.Text)) { 
                    MessageBox.Show("Vui lòng nhập Email và Mật khẩu.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                    return; 
                }
                SocketClient.Send($"CREATE_USER|{tEmail.Text.Trim()}|{tPass.Text.Trim()}|USER|{tName.Text.Trim()}|{tContact.Text.Trim()}|{tClass.Text.Trim()}|{tSubject.Text.Trim()}");
                f.Close();
            };
            f.Controls.Add(bOk);

            f.ShowDialog();
        }
    }
}
