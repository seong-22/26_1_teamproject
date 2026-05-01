using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;
using BILCAM.Models;

namespace BILCAM.Forms
{
    public class AdminMainForm : Form
    {
        private User _user;
        private TabControl _tabs;
        private FlowLayoutPanel _pnlPending, _pnlAll, _pnlItems;

        public AdminMainForm(User user)
        {
            _user = user;
            InitializeComponent();
            LoadPending();
        }

        private void InitializeComponent()
        {
            this.Text = "BILCAM — 관리자";
            this.Size = new Size(900, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BgTertiary;
            this.MinimumSize = new Size(700, 560);
            this.AutoScaleMode = AutoScaleMode.Dpi;  // 추가: DPI 대응
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(24, 60, 137) };
            var lblTitle = new Label
            {
                Text = "BILCAM  관리자 패널",
                Font = new Font("맑은 고딕", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 14),
                AutoSize = true
            };
            var btnLogout = new Button
            {
                Text = "로그아웃",
                Font = Theme.FontSmall,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 100, 180),
                ForeColor = Color.White,
                Size = new Size(72, 26),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => this.Close();

            header.Layout += (s, e) => btnLogout.Location = new Point(header.Width - 86, 13);
            header.Controls.AddRange(new Control[] { lblTitle, btnLogout });

            // Tabs
            _tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBody, Padding = new Point(16, 6) };

            var tabPending = new TabPage("  승인 대기  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabAll     = new TabPage("  전체 예약  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabItems   = new TabPage("  자원 관리  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };

            _pnlPending = MakeFlowPanel(); tabPending.Controls.Add(_pnlPending);
            _pnlAll     = MakeFlowPanel(); tabAll.Controls.Add(_pnlAll);
            _pnlItems   = MakeFlowPanel(); tabItems.Controls.Add(_pnlItems);

            _tabs.TabPages.AddRange(new[] { tabPending, tabAll, tabItems });
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                // 수정: 탭 0은 생성자에서 이미 로드했으므로 중복 호출 방지
                if (_tabs.SelectedIndex == 0 && _pnlPending.Controls.Count == 0) LoadPending();
                if (_tabs.SelectedIndex == 1) LoadAll();
                if (_tabs.SelectedIndex == 2) LoadItems();
            };

            this.Controls.Add(_tabs);
            this.Controls.Add(header);
        }

        private FlowLayoutPanel MakeFlowPanel() => new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4)
        };

        // ── Pending ─────────────────────────────────────────────────────────
        private void LoadPending()
        {
            _pnlPending.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                @"SELECT r.*, res.Name as ResourceName FROM Reservations r
                  JOIN Resources res ON r.ResourceId = res.Id
                  WHERE r.Status = 'pending' ORDER BY r.ReservationDate, r.StartTime");

            if (dt.Rows.Count == 0)
            {
                _pnlPending.Controls.Add(MakeEmptyLabel("승인 대기 중인 예약이 없습니다."));
                return;
            }

            foreach (DataRow row in dt.Rows)
                _pnlPending.Controls.Add(BuildPendingCard(row));
        }

        private Panel BuildPendingCard(DataRow row)
        {
            int resId = Convert.ToInt32(row["Id"]);
            int cardW = CardWidth(_pnlPending);

            var card = new Panel { Width = cardW, Height = 94, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblName  = new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true };
            var lblDetail = new Label
            {
                Text = $"신청자: {row["UserId"]}  |  {row["ReservationDate"]}  {row["StartTime"]} ~ {row["EndTime"]}",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(14, 34),
                AutoSize = true
            };
            var badge = new Label { Text = "  대기  ", Font = Theme.FontSmall, BackColor = Theme.WarningLight, ForeColor = Theme.Warning, AutoSize = true, BorderStyle = BorderStyle.FixedSingle, Location = new Point(cardW - 80, 12) };
            badge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnApprove = Theme.MakeButton("승인", Theme.SuccessLight, Theme.Success, 80, 28);
            btnApprove.FlatAppearance.BorderColor = Color.FromArgb(93, 202, 165);
            btnApprove.Location = new Point(14, 56);
            btnApprove.Click += (s, e) => { UpdateStatus(resId, "approved"); LoadPending(); };

            var btnReject = Theme.MakeButton("반려", Theme.DangerLight, Theme.Danger, 80, 28);
            btnReject.FlatAppearance.BorderColor = Color.FromArgb(240, 149, 123);
            btnReject.Location = new Point(100, 56);
            btnReject.Click += (s, e) => { UpdateStatus(resId, "rejected"); LoadPending(); };

            card.Controls.AddRange(new Control[] { lblName, lblDetail, badge, btnApprove, btnReject });
            return card;
        }

        // ── All reservations ────────────────────────────────────────────────
        private void LoadAll()
        {
            _pnlAll.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                @"SELECT r.*, res.Name as ResourceName FROM Reservations r
                  JOIN Resources res ON r.ResourceId = res.Id
                  ORDER BY r.ReservationDate DESC");

            if (dt.Rows.Count == 0) { _pnlAll.Controls.Add(MakeEmptyLabel("예약 내역이 없습니다.")); return; }

            foreach (DataRow row in dt.Rows)
            {
                string status = row["Status"].ToString();
                string statusText = status == "pending" ? "대기" : status == "approved" ? "승인" : "반려";
                Color bg = status == "pending" ? Theme.WarningLight : status == "approved" ? Theme.SuccessLight : Theme.DangerLight;
                Color fg = status == "pending" ? Theme.Warning : status == "approved" ? Theme.Success : Theme.Danger;

                int cardW = CardWidth(_pnlAll);
                var card = new Panel { Width = cardW, Height = 68, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

                card.Controls.Add(new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true });
                card.Controls.Add(new Label
                {
                    Text = $"신청자: {row["UserId"]}  |  {row["ReservationDate"]}  {row["StartTime"]} ~ {row["EndTime"]}",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(14, 34),
                    AutoSize = true
                });
                card.Controls.Add(new Label { Text = $"  {statusText}  ", Font = Theme.FontSmall, BackColor = bg, ForeColor = fg, AutoSize = true, BorderStyle = BorderStyle.FixedSingle, Location = new Point(cardW - 72, 12), Anchor = AnchorStyles.Top | AnchorStyles.Right });

                _pnlAll.Controls.Add(card);
            }
        }

        // ── Items management ────────────────────────────────────────────────
        private void LoadItems()
        {
            _pnlItems.Controls.Clear();

            // Add resource button
            var btnAdd = Theme.MakeButton("+ 새 자원 추가", Theme.Primary, Color.White, 160, 34);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Margin = new Padding(2, 0, 2, 10);
            btnAdd.Click += (s, e) =>
            {
                var dlg = new AddResourceForm();
                if (dlg.ShowDialog(this) == DialogResult.OK) LoadItems();
            };
            _pnlItems.Controls.Add(btnAdd);

            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Resources ORDER BY Category, Id");
            foreach (DataRow row in dt.Rows)
                _pnlItems.Controls.Add(BuildItemCard(row));
        }

        private Panel BuildItemCard(DataRow row)
        {
            int id = Convert.ToInt32(row["Id"]);
            bool avail = Convert.ToInt32(row["IsAvailable"]) == 1;
            int cardW = CardWidth(_pnlItems);

            var card = new Panel { Width = cardW, Height = 80, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            card.Controls.Add(new Label { Text = row["Name"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true });
            card.Controls.Add(new Label { Text = row["Location"].ToString(), Font = Theme.FontSmall, ForeColor = Theme.TextMuted, Location = new Point(14, 34), AutoSize = true });

            var badge = new Label
            {
                Text = avail ? "  가용  " : "  사용 중  ",
                Font = Theme.FontSmall,
                BackColor = avail ? Theme.SuccessLight : Theme.DangerLight,
                ForeColor = avail ? Theme.Success : Theme.Danger,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardW - 90, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnToggle = Theme.MakeButton(avail ? "사용 중으로 변경" : "가용으로 변경", Theme.BgSecondary, Theme.TextSecondary, 140, 26);
            btnToggle.Location = new Point(14, 48);
            btnToggle.Click += (s, e) =>
            {
                DatabaseHelper.ExecuteNonQuery("UPDATE Resources SET IsAvailable=@v WHERE Id=@id",
                    new System.Data.SQLite.SQLiteParameter("@v", avail ? 0 : 1),
                    new System.Data.SQLite.SQLiteParameter("@id", id));
                LoadItems();
            };

            card.Controls.AddRange(new Control[] { badge, btnToggle });
            return card;
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private void UpdateStatus(int id, string status)
        {
            DatabaseHelper.ExecuteNonQuery("UPDATE Reservations SET Status=@s WHERE Id=@id",
                new System.Data.SQLite.SQLiteParameter("@s", status),
                new System.Data.SQLite.SQLiteParameter("@id", id));
        }

        private int CardWidth(FlowLayoutPanel pnl) => Math.Max(600, pnl.ClientSize.Width - 24);

        private Label MakeEmptyLabel(string text) => new Label
        {
            Text = text,
            Font = Theme.FontBody,
            ForeColor = Theme.TextMuted,
            AutoSize = true,
            Margin = new Padding(8, 24, 0, 0)
        };
    }
}
