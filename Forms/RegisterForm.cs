using System;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;

namespace BILCAM.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtId, txtPw, txtPw2, txtName, txtStudentId;
        private Label lblError;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = AppLanguage.Get("register_title");
            this.Size = new Size(420, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            var pnl = new Panel
            {
                Width = 360,
                Height = 480,
                BackColor = Theme.BgPrimary,
                Location = new Point(30, 30),
                Padding = new Padding(30)
            };
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblTitle = new Label { Text = AppLanguage.Get("register_header"), Font = Theme.FontHeader, ForeColor = Theme.TextPrimary, Location = new Point(30, 20), AutoSize = true };

            int y = 60;
            txtId        = AddField(pnl, AppLanguage.Get("register_id"), ref y);
            txtPw        = AddField(pnl, AppLanguage.Get("register_pw"), ref y, true);
            txtPw2       = AddField(pnl, AppLanguage.Get("register_pw2"), ref y, true);
            txtName      = AddField(pnl, AppLanguage.Get("register_name"), ref y);
            txtStudentId = AddField(pnl, AppLanguage.Get("register_studentid"), ref y);

            lblError = new Label { Text = "", Font = Theme.FontSmall, ForeColor = Theme.Danger, Location = new Point(30, y), AutoSize = false, Width = 300, Height = 20 };
            y += 24;

            var btnSubmit = Theme.MakeButton(AppLanguage.Get("register_submit"), Theme.Primary, Color.White, 300, 38);
            btnSubmit.Location = new Point(30, y);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += DoRegister;

            var btnBack = Theme.MakeButton(AppLanguage.Get("register_cancel"), Theme.BgSecondary, Theme.TextSecondary, 300, 32);
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
            string studentId = txtStudentId.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(studentId))
            { lblError.Text = AppLanguage.Get("register_err_empty"); return; }

            if (pw != pw2)
            { lblError.Text = AppLanguage.Get("register_err_pw"); return; }

            if (pw.Length < 8)
            { lblError.Text = AppLanguage.Get("register_err_pwlen"); return; }

            if (!System.Text.RegularExpressions.Regex.IsMatch(studentId, @"^\d+$"))
            { lblError.Text = AppLanguage.Get("register_err_studentid"); return; }

            var dt = DatabaseHelper.ExecuteQuery($"SELECT id FROM users WHERE userid='{id}'");
            if (dt.Rows.Count > 0)
            { lblError.Text = AppLanguage.Get("register_err_dup"); return; }

            DatabaseHelper.ExecuteNonQuery(
                $"INSERT INTO users (userid, passwordhash, name, role, studentid) VALUES ('{id}', '{DatabaseHelper.HashPassword(pw)}', '{name}', 'student', '{studentId}')");

            MessageBox.Show(AppLanguage.Get("register_success"), "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}