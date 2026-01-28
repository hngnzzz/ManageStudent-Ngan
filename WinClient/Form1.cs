using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WinClient
{
    public partial class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private Label lblMsg;

        public LoginForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "ĐĂNG NHẬP HỆ THỐNG";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 9);

            int x = 40, w = 300;

            Label lblTitle = new Label {
                Text = "QUẢN LÝ SINH VIÊN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(x, 30),
                Size = new Size(w, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            Label l1 = new Label { Text = "Tài khoản:", Location = new Point(x, 90), AutoSize = true };
            this.Controls.Add(l1);

            txtUser = new TextBox { 
                Location = new Point(x, 115), 
                Width = w, 
                Text = "admin@admin.edu.vn",
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(txtUser);

            Label l2 = new Label { Text = "Mật khẩu:", Location = new Point(x, 155), AutoSize = true };
            this.Controls.Add(l2);

            txtPass = new TextBox { 
                Location = new Point(x, 180), 
                Width = w, 
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(txtPass);

            btnLogin = new Button {
                Text = "ĐĂNG NHẬP",
                Location = new Point(x, 230),
                Size = new Size(w, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            lblMsg = new Label {
                Location = new Point(x, 280),
                Size = new Size(w, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Red
            };
            this.Controls.Add(lblMsg);

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                SocketClient.Close(); 

                if (!SocketClient.Connect())
                {
                    lblMsg.Text = "Không thể kết nối Server!";
                    return;
                }

                string u = txtUser.Text.Trim();
                string p = txtPass.Text.Trim();

                if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
                {
                    lblMsg.Text = "Nhập đủ thông tin.";
                    return;
                }

                SocketClient.Send($"LOGIN|{u}|{p}");
                string response = SocketClient.Receive();

                if (response != null && response.StartsWith("LOGIN_SUCCESS"))
                {
                    string[] parts = response.Split('|');
                    string role = parts.Length > 1 ? parts[1] : "USER";
                    string fullName = (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2])) ? parts[2] : u; 

                    MainForm main = new MainForm(role, u, fullName);
                    this.Hide();
                    main.ShowDialog();
                    
                    if (main.IsLogout) 
                    {
                        SocketClient.Close(); 
                        this.Show(); 
                        txtPass.Text = ""; 
                        txtUser.Focus();
                    }
                    else 
                    {
                        SocketClient.Close();
                        Application.Exit(); 
                    }
                }
                else
                {
                    lblMsg.Text = "Sai tài khoản hoặc mật khẩu!";
                    SocketClient.Close();
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Lỗi: " + ex.Message;
            }
        }
    }
}
