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
            this.Text = AppLanguage.Get("login_title");
            this.Size = new Size(420, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var pnl = new Panel
            {
                Width = 360,
                Height = 520,
                BackColor = Theme.BgPrimary,
                Padding = new Padding(30)
            };
            pnl.Location = new Point((this.ClientSize.Width - pnl.Width) / 2, 30);
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle,
                Theme.Border, ButtonBorderStyle.Solid);

            // 언어 선택 버튼
            var btnKo = new Button
            {
                Text = "KO",
                Font = Theme.FontSmall,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppLanguage.Current == "ko" ? Theme.Primary : Theme.BgSecondary,
                ForeColor = AppLanguage.Current == "ko" ? Color.White : Theme.TextSecondary,
                Size = new Size(40, 26),
                Location = new Point(30, 10),
                Cursor = Cursors.Hand
            };
            btnKo.FlatAppearance.BorderSize = 1;
            btnKo.FlatAppearance.BorderColor = Theme.Border;

            var btnEn = new Button
            {
                Text = "EN",
                Font = Theme.FontSmall,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppLanguage.Current == "en" ? Theme.Primary : Theme.BgSecondary,
                ForeColor = AppLanguage.Current == "en" ? Color.White : Theme.TextSecondary,
                Size = new Size(40, 26),
                Location = new Point(74, 10),
                Cursor = Cursors.Hand
            };
            btnEn.FlatAppearance.BorderSize = 1;
            btnEn.FlatAppearance.BorderColor = Theme.Border;

            btnKo.Click += (s, e) =>
            {
                AppLanguage.Current = "ko";
                this.Controls.Clear();
                InitializeComponent();
            };
            btnEn.Click += (s, e) =>
            {
                AppLanguage.Current = "en";
                this.Controls.Clear();
                InitializeComponent();
            };

            // 광운대 로고
            var picLogo = new PictureBox
            {
                Width = 280,
                Height = 72,
                Location = new Point(40, 44),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                string imgPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Application.ExecutablePath),
                    "..", "..", "..", "kwangwoon_logo.png");
                picLogo.Image = Image.FromFile(imgPath);
            }
            catch { }

            // BILCAM 텍스트
            var lblLogo = new Label
            {
                Text = "BILCAM",
                Font = Theme.FontTitle,
                ForeColor = Theme.Primary,
                AutoSize = false,
                Width = 300,
                Height = 40,
                Location = new Point(30, 124),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 서브타이틀
            var lblSub = new Label
            {
                Text = AppLanguage.Get("login_sub"),
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                Width = 300,
                Height = 22,
                Location = new Point(30, 166),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Divider
            var div = new Panel { BackColor = Theme.Border, Width = 300, Height = 1, Location = new Point(30, 196) };

            // ID field
            var lblId = new Label { Text = AppLanguage.Get("login_id"), Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(30, 210), AutoSize = true };
            txtId = Theme.MakeInput();
            txtId.Width = 300;
            txtId.Location = new Point(30, 228);
            txtId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPw.Focus(); };

            // PW field
            var lblPw = new Label { Text = AppLanguage.Get("login_pw"), Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(30, 268), AutoSize = true };
            txtPw = Theme.MakeInput(isPassword: true);
            txtPw.Width = 300;
            txtPw.Location = new Point(30, 286);
            txtPw.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

            // Error label
            lblError = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.Danger,
                Location = new Point(30, 324),
                AutoSize = false,
                Width = 300,
                Height = 18
            };

            // Login button
            var btnLogin = Theme.MakeButton(AppLanguage.Get("login_btn"), Theme.Primary, Color.White, 300, 40);
            btnLogin.Location = new Point(30, 346);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => DoLogin();

            // Register button
            var btnReg = Theme.MakeButton(AppLanguage.Get("login_register"), Theme.BgSecondary, Theme.TextSecondary, 300, 36);
            btnReg.Location = new Point(30, 394);
            btnReg.Click += (s, e) =>
            {
                var reg = new RegisterForm();
                reg.ShowDialog(this);
            };

            pnl.Controls.AddRange(new Control[] { btnKo, btnEn, picLogo, lblLogo, lblSub, div, lblId, txtId, lblPw, txtPw, lblError, btnLogin, btnReg });
            this.Controls.Add(pnl);
        }

        private void DoLogin()
        {
            string id = txtId.Text.Trim();
            string pw = txtPw.Text.Trim();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                lblError.Text = AppLanguage.Get("login_error_empty");
                return;
            }

            try
            {
                string hash = DatabaseHelper.HashPassword(pw);
                var dt = DatabaseHelper.ExecuteQuery(
                    $"SELECT * FROM users WHERE userid='{id}' AND passwordhash='{hash}'");

                if (dt.Rows.Count == 0)
                {
                    lblError.Text = AppLanguage.Get("login_error_fail");
                    return;
                }

                var row = dt.Rows[0];
                var user = new User
                {
                    Id = Convert.ToInt32(row["id"]),
                    UserId = row["userid"].ToString(),
                    Name = row["name"].ToString(),
                    Role = row["role"].ToString()
                };

                this.Hide();
                Form next = user.IsAdmin ? (Form)new AdminMainForm(user) : new StudentMainForm(user);
                next.FormClosed += (s, e) => this.Show();
                next.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류: " + ex.Message);
            }
        }
    }
}