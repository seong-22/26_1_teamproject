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
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(139, 30, 20) };
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
                BackColor = Color.FromArgb(139, 30, 20),
                ForeColor = Color.White,
                Size = new Size(72, 26),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 1;
            btnLogout.FlatAppearance.BorderColor = Color.White;
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => this.Close();

            header.Layout += (s, e) => btnLogout.Location = new Point(header.Width - 86, 13);
            header.Controls.AddRange(new Control[] { lblTitle, btnLogout });

            // Tabs
            _tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBody, Padding = new Point(16, 6) };

            var tabPending = new TabPage("  승인 대기  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabAll = new TabPage("  전체 예약  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabItems = new TabPage("  자원 관리  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };

            _pnlPending = MakeFlowPanel(); tabPending.Controls.Add(_pnlPending);
            _pnlAll     = MakeFlowPanel(); tabAll.Controls.Add(_pnlAll);
            _pnlItems   = MakeFlowPanel(); tabItems.Controls.Add(_pnlItems);

            _tabs.TabPages.AddRange(new[] { tabPending, tabAll, tabItems });
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedIndex == 0 && _pnlPending.Controls.Count == 0) LoadPending();
                if (_tabs.SelectedIndex == 1) LoadAll();
                if (_tabs.SelectedIndex == 2) LoadItems();
            };

            this.Controls.Add(_tabs);
            this.Controls.Add(header);

            // 30초마다 자동 새로고침
            var refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) =>
            {
                if (_tabs.SelectedIndex == 0) LoadPending();
                else if (_tabs.SelectedIndex == 1) LoadAll();
            };
            refreshTimer.Start();
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
                @"SELECT r.*, res.name as ResourceName, u.studentid
                  FROM reservations r
                  JOIN resources res ON r.resourceid = res.id
                  LEFT JOIN users u ON r.userid = u.userid
                  WHERE r.status = 'pending' ORDER BY r.reservationdate, r.starttime");

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
            int resId = Convert.ToInt32(row["id"]);

            string memo = (row.Table.Columns.Contains("memo") && row["memo"] != DBNull.Value)
                ? row["memo"].ToString() : "";
            bool hasMemo = !string.IsNullOrWhiteSpace(memo);

            int cardH = hasMemo ? 116 : 94;
            int cardW = CardWidth(_pnlPending);

            var card = new Panel { Width = cardW, Height = cardH, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblName = new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true };

            string studentId = row["studentid"] == DBNull.Value ? "미등록" : row["studentid"].ToString();
            var lblDetail = new Label
            {
                Text = $"신청자: {row["userid"]}  |  학번: {studentId}  |  {row["reservationdate"]}  {row["starttime"]} ~ {row["endtime"]}",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(14, 34),
                AutoSize = true
            };

            var badge = new Label { Text = "  대기  ", Font = Theme.FontSmall, BackColor = Theme.WarningLight, ForeColor = Theme.Warning, AutoSize = true, BorderStyle = BorderStyle.FixedSingle, Location = new Point(cardW - 80, 12) };
            badge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            card.Controls.AddRange(new Control[] { lblName, lblDetail, badge });

            int btnY = 56;
            if (hasMemo)
            {
                var lblMemo = new Label
                {
                    Text = $"메모: {memo}",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextMuted,
                    Location = new Point(14, 56),
                    AutoSize = true
                };
                card.Controls.Add(lblMemo);
                btnY = 80;
            }

            var btnApprove = Theme.MakeButton("승인", Theme.SuccessLight, Theme.Success, 80, 28);
            btnApprove.FlatAppearance.BorderColor = Color.FromArgb(93, 202, 165);
            btnApprove.Location = new Point(14, btnY);
            btnApprove.Click += (s, e) => { UpdateStatus(resId, "approved"); LoadPending(); };

            var btnReject = Theme.MakeButton("반려", Theme.DangerLight, Theme.Danger, 80, 28);
            btnReject.FlatAppearance.BorderColor = Color.FromArgb(240, 149, 123);
            btnReject.Location = new Point(100, btnY);
            btnReject.Click += (s, e) => { RejectWithReason(resId); };

            card.Controls.AddRange(new Control[] { btnApprove, btnReject });
            return card;
        }

        // 반려 사유 입력 다이얼로그
        private void RejectWithReason(int reservationId)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "반려 사유 입력";
                dlg.Size = new Size(400, 220);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Theme.BgPrimary;

                var lbl = new Label
                {
                    Text = "반려 사유를 입력해주세요. (선택)",
                    Font = Theme.FontBody,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                var txtReason = new TextBox
                {
                    Location = new Point(20, 50),
                    Width = 340,
                    Height = 60,
                    Multiline = true,
                    Font = Theme.FontBody,
                    BorderStyle = BorderStyle.FixedSingle,
                    MaxLength = 200
                };

                var btnOk = Theme.MakeButton("반려 처리", Theme.Danger, Color.White, 160, 36);
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Location = new Point(20, 130);
                btnOk.Click += (s, e) =>
                {
                    string reason = txtReason.Text.Trim().Replace("'", "''");
                    DatabaseHelper.ExecuteNonQuery(
                        $"UPDATE reservations SET status='rejected', rejectreason='{reason}' WHERE id={reservationId}");
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                var btnCancel = Theme.MakeButton("취소", Theme.BgSecondary, Theme.TextSecondary, 100, 36);
                btnCancel.Location = new Point(190, 130);
                btnCancel.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                dlg.Controls.AddRange(new Control[] { lbl, txtReason, btnOk, btnCancel });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadPending();
            }
        }

        // ── All reservations ────────────────────────────────────────────────
        private void LoadAll()
        {
            _pnlAll.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                @"SELECT r.*, res.name as ResourceName, u.studentid
                  FROM reservations r
                  JOIN resources res ON r.resourceid = res.id
                  LEFT JOIN users u ON r.userid = u.userid
                  ORDER BY r.reservationdate DESC");

            if (dt.Rows.Count == 0) { _pnlAll.Controls.Add(MakeEmptyLabel("예약 내역이 없습니다.")); return; }

            foreach (DataRow row in dt.Rows)
            {
                string status = row["status"].ToString();
                string statusText = status == "pending" ? "대기" : status == "approved" ? "승인" : "반려";
                Color bg = status == "pending" ? Theme.WarningLight : status == "approved" ? Theme.SuccessLight : Theme.DangerLight;
                Color fg = status == "pending" ? Theme.Warning : status == "approved" ? Theme.Success : Theme.Danger;

                string memo = (row.Table.Columns.Contains("memo") && row["memo"] != DBNull.Value)
                    ? row["memo"].ToString() : "";
                string rejectReason = (row.Table.Columns.Contains("rejectreason") && row["rejectreason"] != DBNull.Value)
                    ? row["rejectreason"].ToString() : "";

                bool hasMemo = !string.IsNullOrWhiteSpace(memo);
                bool hasReject = status == "rejected" && !string.IsNullOrWhiteSpace(rejectReason);
                int extraLines = (hasMemo ? 1 : 0) + (hasReject ? 1 : 0);

                int cardW = CardWidth(_pnlAll);
                int cardH = 68 + extraLines * 22;
                var card = new Panel { Width = cardW, Height = cardH, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

                string studentId = row["studentid"] == DBNull.Value ? "미등록" : row["studentid"].ToString();

                card.Controls.Add(new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true });
                card.Controls.Add(new Label
                {
                    Text = $"신청자: {row["userid"]}  |  학번: {studentId}  |  {row["reservationdate"]}  {row["starttime"]} ~ {row["endtime"]}",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(14, 34),
                    AutoSize = true
                });
                card.Controls.Add(new Label { Text = $"  {statusText}  ", Font = Theme.FontSmall, BackColor = bg, ForeColor = fg, AutoSize = true, BorderStyle = BorderStyle.FixedSingle, Location = new Point(cardW - 72, 12), Anchor = AnchorStyles.Top | AnchorStyles.Right });

                int lineY = 56;
                if (hasMemo)
                {
                    card.Controls.Add(new Label { Text = $"메모: {memo}", Font = Theme.FontSmall, ForeColor = Theme.TextMuted, Location = new Point(14, lineY), AutoSize = true });
                    lineY += 22;
                }
                if (hasReject)
                {
                    card.Controls.Add(new Label { Text = $"반려 사유: {rejectReason}", Font = Theme.FontSmall, ForeColor = Theme.Danger, Location = new Point(14, lineY), AutoSize = true });
                    lineY += 22;
                }

                _pnlAll.Controls.Add(card);
            }
        }

        // ── Items management ────────────────────────────────────────────────
        private void LoadItems()
        {
            _pnlItems.Controls.Clear();

            var btnAdd = Theme.MakeButton("+ 새 자원 추가", Theme.Primary, Color.White, 160, 34);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Margin = new Padding(2, 0, 2, 10);
            btnAdd.Click += (s, e) =>
            {
                var dlg = new AddResourceForm();
                if (dlg.ShowDialog(this) == DialogResult.OK) LoadItems();
            };
            _pnlItems.Controls.Add(btnAdd);

            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM resources ORDER BY category, id");
            foreach (DataRow row in dt.Rows)
                _pnlItems.Controls.Add(BuildItemCard(row));
        }

        private Panel BuildItemCard(DataRow row)
        {
            int id = Convert.ToInt32(row["id"]);
            bool avail = Convert.ToInt32(row["isavailable"]) == 1;
            int cardW = CardWidth(_pnlItems);

            var card = new Panel { Width = cardW, Height = 80, BackColor = Theme.BgPrimary, Margin = new Padding(2, 0, 2, 6) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            card.Controls.Add(new Label { Text = row["name"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true });
            card.Controls.Add(new Label { Text = row["location"].ToString(), Font = Theme.FontSmall, ForeColor = Theme.TextMuted, Location = new Point(14, 34), AutoSize = true });

            var badge = new Label
            {
                Text = avail ? " 사용 가능 " : "  사용 불가  ",
                Font = Theme.FontSmall,
                BackColor = avail ? Theme.SuccessLight : Theme.DangerLight,
                ForeColor = avail ? Theme.Success : Theme.Danger,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardW - 90, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnToggle = Theme.MakeButton(avail ? "사용 불가로 변경" : "사용 가능으로 변경", Theme.BgSecondary, Theme.TextSecondary, 160, 26);
            btnToggle.Location = new Point(14, 48);
            btnToggle.Click += (s, e) =>
            {
                DatabaseHelper.ExecuteNonQuery($"UPDATE resources SET isavailable={(avail ? 0 : 1)} WHERE id={id}");
                LoadItems();
            };

            card.Controls.AddRange(new Control[] { badge, btnToggle });
            return card;
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private void UpdateStatus(int id, string status)
        {
            DatabaseHelper.ExecuteNonQuery($"UPDATE reservations SET status='{status}' WHERE id={id}");
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