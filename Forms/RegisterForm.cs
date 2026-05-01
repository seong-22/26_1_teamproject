using System;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;

namespace BILCAM.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtId, txtPw, txtPw2, txtName;
        private Label lblError;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "BILCAM — 회원가입";
            this.Size = new Size(420, 500);
            this.StartPosition = FormStartPosition.CenterParent;
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
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblTitle = new Label { Text = "회원가입", Font = Theme.FontHeader, ForeColor = Theme.TextPrimary, Location = new Point(30, 20), AutoSize = true };

            int y = 60;
            txtId   = AddField(pnl, "아이디", ref y);
            txtPw   = AddField(pnl, "비밀번호", ref y, true);
            txtPw2  = AddField(pnl, "비밀번호 확인", ref y, true);
            txtName = AddField(pnl, "이름", ref y);

            lblError = new Label { Text = "", Font = Theme.FontSmall, ForeColor = Theme.Danger, Location = new Point(30, y), AutoSize = false, Width = 300, Height = 20 };
            y += 24;

            var btnSubmit = Theme.MakeButton("가입 완료", Theme.Primary, Color.White, 300, 38);
            btnSubmit.Location = new Point(30, y);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += DoRegister;

            var btnBack = Theme.MakeButton("취소", Theme.BgSecondary, Theme.TextSecondary, 300, 32);
            btnBack.Location = new Point(30, y + 44);
            btnBack.Click += (s, e) => this.Close();

            pnl.Controls.AddRange(new Control[] { lblTitle, lblError, btnSubmit, btnBack });
            this.Controls.Add(pnl);
        }

        private TextBox AddField(Panel pnl, string label, ref int y, bool isPassword = false)
        {
            var lbl = new Label { Text = label, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(30, y), AutoSize = true };
            y += 20;
            var tb = Theme.MakeInput(isPassword: isPassword);
            tb.Width = 300;
            tb.Location = new Point(30, y);
            y += 44;
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(tb);
            return tb;
        }

        private void DoRegister(object sender, EventArgs e)
        {
            string id = txtId.Text.Trim();
            string pw = txtPw.Text.Trim();
            string pw2 = txtPw2.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(name))
            { lblError.Text = "모든 항목을 입력하세요."; return; }

            if (pw != pw2)
            { lblError.Text = "비밀번호가 일치하지 않습니다."; return; }

            // Check duplicate
            var dt = DatabaseHelper.ExecuteQuery("SELECT Id FROM Users WHERE UserId=@id", new System.Data.SQLite.SQLiteParameter("@id", id));
            if (dt.Rows.Count > 0)
            { lblError.Text = "이미 사용 중인 아이디입니다."; return; }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Users (UserId, PasswordHash, Name, Role) VALUES (@id, @pw, @name, 'student')",
                new System.Data.SQLite.SQLiteParameter("@id", id),
                new System.Data.SQLite.SQLiteParameter("@pw", DatabaseHelper.HashPassword(pw)),
                new System.Data.SQLite.SQLiteParameter("@name", name));

            MessageBox.Show("회원가입이 완료되었습니다!", "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
