using System;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;

namespace BILCAM.Forms
{
    public class AddResourceForm : Form
    {
        private TextBox txtName, txtLocation;
        private ComboBox cboCategory;

        public AddResourceForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "새 자원 추가";
            this.Size = new Size(380, 360);  // 수정: 320 → 360 (버튼 잘림 방지)
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AutoScaleMode = AutoScaleMode.Dpi;  // 추가: DPI 대응
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            var pnl = new Panel { Width = 320, Height = 280, Location = new Point(30, 20), BackColor = Theme.BgPrimary, Padding = new Padding(20) };  // 수정: 240 → 280
            pnl.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            int y = 16;

            var lblName = new Label { Text = "자원명", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtName = Theme.MakeInput();
            txtName.Width = 280;
            txtName.Location = new Point(16, y);
            y += 44;

            var lblCat = new Label { Text = "카테고리", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(16, y), AutoSize = true };
            y += 20;
            cboCategory = new ComboBox { Width = 280, Location = new Point(16, y), Font = Theme.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategory.Items.AddRange(new[] { "강의실 (classroom)", "노트북 (laptop)", "우산 (umbrella)" });
            cboCategory.SelectedIndex = 0;
            y += 44;

            var lblLoc = new Label { Text = "위치", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(16, y), AutoSize = true };
            y += 20;
            txtLocation = Theme.MakeInput();
            txtLocation.Width = 280;
            txtLocation.Location = new Point(16, y);
            y += 44;

            var btnOk = Theme.MakeButton("추가", Theme.Primary, Color.White, 134, 34);
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Location = new Point(16, y);
            btnOk.Click += DoAdd;

            var btnCancel = Theme.MakeButton("취소", Theme.BgSecondary, Theme.TextSecondary, 134, 34);
            btnCancel.Location = new Point(158, y);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            pnl.Controls.AddRange(new Control[] { lblName, txtName, lblCat, cboCategory, lblLoc, txtLocation, btnOk, btnCancel });
            this.Controls.Add(pnl);
        }

        private void DoAdd(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string loc = txtLocation.Text.Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(loc))
            { MessageBox.Show("모든 항목을 입력하세요.", "BILCAM"); return; }

            string[] cats = { "classroom", "laptop", "umbrella" };
            string cat = cats[cboCategory.SelectedIndex];

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Resources (Name, Category, Location, IsAvailable) VALUES (@n, @c, @l, 1)",
                new System.Data.SQLite.SQLiteParameter("@n", name),
                new System.Data.SQLite.SQLiteParameter("@c", cat),
                new System.Data.SQLite.SQLiteParameter("@l", loc));

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
