using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;
using BILCAM.Models;

namespace BILCAM.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtId, txtPw;
        private Label lblError;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "BILCAM — 로그인";
            this.Size = new Size(420, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var pnl = new Panel
            {
                Width = 360,
                Height = 420,
                BackColor = Theme.BgPrimary,
                Location = new Point(30, 30),
                Padding = new Padding(30)
            };
            // Border simulation via paint
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle,
                Theme.Border, ButtonBorderStyle.Solid);

            // Logo
            var lblLogo = new Label
            {
                Text = "BILCAM",
                Font = Theme.FontTitle,
                ForeColor = Theme.Primary,
                AutoSize = false,
                Width = 300,
                Height = 40,
                Location = new Point(30, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblSub = new Label
            {
                Text = "빌려 쓰는 캠퍼스 — 광운대학교 새빛관",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                Width = 300,
                Height = 22,
                Location = new Point(30, 72),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Divider
            var div = new Panel { BackColor = Theme.Border, Width = 300, Height = 1, Location = new Point(30, 104) };

            // ID field
            var lblId = new Label { Text = "아이디", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(30, 120), AutoSize = true };
            txtId = Theme.MakeInput();
            txtId.Width = 300;
            txtId.Location = new Point(30, 138);
            txtId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPw.Focus(); };

            // PW field
            var lblPw = new Label { Text = "비밀번호", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(30, 178), AutoSize = true };
            txtPw = Theme.MakeInput(isPassword: true);
            txtPw.Width = 300;
            txtPw.Location = new Point(30, 196);
            txtPw.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

            // Error label
            lblError = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.Danger,
                Location = new Point(30, 234),
                AutoSize = false,
                Width = 300,
                Height = 18
            };

            // Login button
            var btnLogin = Theme.MakeButton("로그인", Theme.Primary, Color.White, 300, 38);
            btnLogin.Location = new Point(30, 256);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => DoLogin();

            // Register button
            var btnReg = Theme.MakeButton("회원가입", Theme.BgSecondary, Theme.TextSecondary, 300, 34);
            btnReg.Location = new Point(30, 302);
            btnReg.Click += (s, e) =>
            {
                var reg = new RegisterForm();
                reg.ShowDialog(this);
            };

            // Hint
            var hint = new Label
            {
                Text = "테스트: student/1234 (학생)  |  admin/1234 (관리자)",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                Location = new Point(30, 348),
                AutoSize = false,
                Width = 300,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnl.Controls.AddRange(new Control[] { lblLogo, lblSub, div, lblId, txtId, lblPw, txtPw, lblError, btnLogin, btnReg, hint });
            this.Controls.Add(pnl);
        }

        private void DoLogin()
        {
            string id = txtId.Text.Trim();
            string pw = txtPw.Text.Trim();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                lblError.Text = "아이디와 비밀번호를 입력하세요.";
                return;
            }

            string hash = DatabaseHelper.HashPassword(pw);
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM Users WHERE UserId=@id AND PasswordHash=@pw",
                new System.Data.SQLite.SQLiteParameter("@id", id),
                new System.Data.SQLite.SQLiteParameter("@pw", hash));

            if (dt.Rows.Count == 0)
            {
                lblError.Text = "아이디 또는 비밀번호가 올바르지 않습니다.";
                return;
            }

            var row = dt.Rows[0];
            var user = new User
            {
                Id = Convert.ToInt32(row["Id"]),
                UserId = row["UserId"].ToString(),
                Name = row["Name"].ToString(),
                Role = row["Role"].ToString()
            };

            this.Hide();
            Form next = user.IsAdmin ? (Form)new AdminMainForm(user) : new StudentMainForm(user);
            next.FormClosed += (s, e) => this.Show();
            next.Show();
        }
    }
}
