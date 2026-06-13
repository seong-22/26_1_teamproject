using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;
using BILCAM.Models;

namespace BILCAM.Forms
{
    public class StudentMainForm : Form
    {
        private User _user;
        private TabControl _tabs;
        private FlowLayoutPanel _pnlResources;
        private TabControl _resTabControl;
        private FlowLayoutPanel _pnlPending, _pnlApproved, _pnlRejected;

        public StudentMainForm(User user)
        {
            _user = user;
            InitializeComponent();
            LoadResources();
        }

        private void InitializeComponent()
        {
            this.Text = $"BILCAM — {_user.Name}님 환영합니다";
            this.Size = new Size(800, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.BgTertiary;
            this.MinimumSize = new Size(700, 560);

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Theme.BgPrimary };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Theme.Border), 0, 51, header.Width, 51);

            var lblTitle = new Label
            {
                Text = "BILCAM",
                Font = new Font("맑은 고딕", 14f, FontStyle.Bold),
                ForeColor = Theme.Primary,
                Location = new Point(20, 14),
                AutoSize = true
            };
            var lblUser = new Label
            {
                Text = $"{_user.Name} 님  |  학생",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                AutoSize = true
            };
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 실시간 시계
            var lblClock = new Label
            {
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            lblClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnLogout = new Button
            {
                Text = "로그아웃",
                Font = Theme.FontSmall,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.BgSecondary,
                ForeColor = Theme.TextSecondary,
                Size = new Size(72, 26),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderColor = Theme.Border;
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => { this.Close(); };

            header.Controls.AddRange(new Control[] { lblTitle, lblUser, lblClock, btnLogout });
            header.Layout += (s, e) =>
            {
                btnLogout.Location = new Point(header.Width - 86, 13);
                lblUser.Location = new Point(header.Width - 90 - lblUser.PreferredWidth - 8, 18);
                lblClock.Location = new Point(lblUser.Location.X - lblClock.PreferredWidth - 16, 18);
            };

            // 메인 탭
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = Theme.FontBody,
                Padding = new Point(16, 6)
            };

            var tabResources = new TabPage("  자원 조회  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabMyRes = new TabPage("  내 예약  ") { BackColor = Theme.BgTertiary, Padding = new Padding(8) };

            // 자원 조회 패널
            _pnlResources = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(4)
            };
            tabResources.Controls.Add(_pnlResources);

            // 내 예약 서브탭
            _resTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = Theme.FontSmall,
                Padding = new Point(12, 5)
            };

            var tabPending = new TabPage("  승인 대기  ") { BackColor = Theme.BgTertiary, Padding = new Padding(6) };
            var tabApproved = new TabPage("  승인됨  ") { BackColor = Theme.BgTertiary, Padding = new Padding(6) };
            var tabRejected = new TabPage("  반려됨  ") { BackColor = Theme.BgTertiary, Padding = new Padding(6) };

            _pnlPending = MakeResPanel(); tabPending.Controls.Add(_pnlPending);
            _pnlApproved = MakeResPanel(); tabApproved.Controls.Add(_pnlApproved);
            _pnlRejected = MakeResPanel(); tabRejected.Controls.Add(_pnlRejected);

            _resTabControl.TabPages.AddRange(new[] { tabPending, tabApproved, tabRejected });
            _resTabControl.SelectedIndexChanged += (s, e) => LoadMyReservations();

            tabMyRes.Controls.Add(_resTabControl);

            _tabs.TabPages.Add(tabResources);
            _tabs.TabPages.Add(tabMyRes);
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedIndex == 1) LoadMyReservations();
            };

            this.Controls.Add(_tabs);
            this.Controls.Add(header);

            // 30초마다 자동 새로고침
            var refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) =>
            {
                if (_tabs.SelectedIndex == 0) LoadResources();
                else LoadMyReservations();
            };
            refreshTimer.Start();

            // 1초마다 시계 업데이트
            var clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
            {
                lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                header.PerformLayout();
            };
            clockTimer.Start();
        }

        private FlowLayoutPanel MakeResPanel() => new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4)
        };

        // ── Resources ──────────────────────────────────────────────────────
        private void LoadResources()
        {
            _pnlResources.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM resources ORDER BY category, id");

            string currentCat = "";
            foreach (DataRow row in dt.Rows)
            {
                string cat = row["category"].ToString();
                if (cat != currentCat)
                {
                    currentCat = cat;
                    string catName = cat == "classroom" ? "강의실" : cat == "laptop" ? "공용 노트북" : "우산";
                    var lbl = new Label
                    {
                        Text = catName,
                        Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                        ForeColor = Theme.TextSecondary,
                        AutoSize = false,
                        Width = _pnlResources.ClientSize.Width - 20,
                        Height = 28,
                        Margin = new Padding(2, 12, 2, 2)
                    };
                    _pnlResources.Controls.Add(lbl);
                }

                bool avail = Convert.ToInt32(row["isavailable"]) == 1;
                var card = BuildResourceCard(
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString(),
                    row["location"].ToString(),
                    row["category"].ToString(),
                    avail);
                _pnlResources.Controls.Add(card);
            }
        }

        private Panel BuildResourceCard(int id, string name, string location, string category, bool available)
        {
            int cardWidth = Math.Max(500, _pnlResources.ClientSize.Width - 24);
            var card = new Panel
            {
                Width = cardWidth,
                Height = 72,
                BackColor = Theme.BgPrimary,
                Margin = new Padding(2, 0, 2, 6),
                Cursor = available ? Cursors.Hand : Cursors.Default
            };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblName = new Label { Text = name, Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 14), AutoSize = true };
            var lblLoc = new Label { Text = location, Font = Theme.FontSmall, ForeColor = Theme.TextMuted, Location = new Point(14, 36), AutoSize = true };

            Color dotBg = available ? Theme.SuccessLight : Theme.DangerLight;
            Color dotFg = available ? Theme.Success : Theme.Danger;
            string statusText = available ? "예약 가능" : "사용 중";

            var badge = new Label
            {
                Text = $"  {statusText}  ",
                Font = Theme.FontSmall,
                BackColor = dotBg,
                ForeColor = dotFg,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardWidth - 100, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            card.Controls.AddRange(new Control[] { lblName, lblLoc, badge });

            if (available)
            {
                card.Click    += (s, e) => OpenReservation(id, name, category);
                lblName.Click += (s, e) => OpenReservation(id, name, category);
                lblLoc.Click  += (s, e) => OpenReservation(id, name, category);
                card.MouseEnter += (s, e) => card.BackColor = Theme.BgSecondary;
                card.MouseLeave += (s, e) => card.BackColor = Theme.BgPrimary;
            }

            return card;
        }

        private void OpenReservation(int resourceId, string resourceName, string category)
        {
            var form = new ReservationForm(_user, resourceId, resourceName, category);
            form.ShowDialog(this);
            LoadResources();
        }

        // ── My Reservations ────────────────────────────────────────────────
        private void LoadMyReservations()
        {
            _pnlPending.Controls.Clear();
            _pnlApproved.Controls.Clear();
            _pnlRejected.Controls.Clear();

            var dt = DatabaseHelper.ExecuteQuery(
                $"SELECT r.*, res.name as ResourceName FROM reservations r " +
                $"JOIN resources res ON r.resourceid = res.id " +
                $"WHERE r.userid = '{_user.UserId}' ORDER BY r.reservationdate DESC");

            bool hasPending = false, hasApproved = false, hasRejected = false;

            foreach (DataRow row in dt.Rows)
            {
                string status = row["status"].ToString();
                var card = BuildMyResCard(row);
                if (status == "pending") { _pnlPending.Controls.Add(card); hasPending = true; }
                else if (status == "approved") { _pnlApproved.Controls.Add(card); hasApproved = true; }
                else { _pnlRejected.Controls.Add(card); hasRejected = true; }
            }

            if (!hasPending) _pnlPending.Controls.Add(MakeEmptyLabel("승인 대기 중인 예약이 없습니다."));
            if (!hasApproved) _pnlApproved.Controls.Add(MakeEmptyLabel("승인된 예약이 없습니다."));
            if (!hasRejected) _pnlRejected.Controls.Add(MakeEmptyLabel("반려된 예약이 없습니다."));
        }

        private bool IsPast(DataRow row)
        {
            string dateStr = row["reservationdate"].ToString();
            string endTime = row["endtime"].ToString();

            if (!DateTime.TryParse(dateStr, out DateTime date)) return false;
            if (!TimeSpan.TryParse(endTime, out TimeSpan time)) return false;

            DateTime endDateTime = date.Date + time;
            return DateTime.Now > endDateTime;
        }

        private Panel BuildMyResCard(DataRow row)
        {
            int id = Convert.ToInt32(row["id"]);
            string status = row["status"].ToString();
            Color statusBg = status == "pending" ? Theme.WarningLight : status == "approved" ? Theme.SuccessLight : Theme.DangerLight;
            Color statusFg = status == "pending" ? Theme.Warning : status == "approved" ? Theme.Success : Theme.Danger;
            string statusText = status == "pending" ? "승인 대기" : status == "approved" ? "승인됨" : "반려됨";

            var pnl = status == "pending" ? _pnlPending : status == "approved" ? _pnlApproved : _pnlRejected;
            int cardWidth = Math.Max(500, pnl.ClientSize.Width - 24);
            bool canCancel = (status == "pending" || status == "approved") && !IsPast(row);

            // 메모 / 반려 사유 확인
            string memo = (row.Table.Columns.Contains("memo") && row["memo"] != DBNull.Value)
                ? row["memo"].ToString() : "";
            string rejectReason = (row.Table.Columns.Contains("rejectreason") && row["rejectreason"] != DBNull.Value)
                ? row["rejectreason"].ToString() : "";

            bool hasMemo = !string.IsNullOrWhiteSpace(memo);
            bool hasReject = status == "rejected" && !string.IsNullOrWhiteSpace(rejectReason);
            int extraLines = (hasMemo ? 1 : 0) + (hasReject ? 1 : 0);

            int baseHeight = canCancel ? 90 : 72;
            int cardHeight = baseHeight + extraLines * 22;

            var card = new Panel
            {
                Width = cardWidth,
                Height = cardHeight,
                BackColor = Theme.BgPrimary,
                Margin = new Padding(2, 0, 2, 8)
            };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblName = new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true };

            string startTime = row["starttime"].ToString();
            string detailText = startTime == "00:00"
                ? $"{row["reservationdate"]}  (하루 대여)"
                : $"{row["reservationdate"]}  {row["starttime"]} ~ {row["endtime"]}";

            var lblDetail = new Label { Text = detailText, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Location = new Point(14, 34), AutoSize = true };

            var badge = new Label
            {
                Text = $"  {statusText}  ",
                Font = Theme.FontSmall,
                BackColor = statusBg,
                ForeColor = statusFg,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardWidth - 100, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            card.Controls.AddRange(new Control[] { lblName, lblDetail, badge });

            int lineY = 56;
            if (hasMemo)
            {
                var lblMemo = new Label
                {
                    Text = $"메모: {memo}",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextMuted,
                    Location = new Point(14, lineY),
                    AutoSize = true
                };
                card.Controls.Add(lblMemo);
                lineY += 22;
            }
            if (hasReject)
            {
                var lblReject = new Label
                {
                    Text = $"반려 사유: {rejectReason}",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.Danger,
                    Location = new Point(14, lineY),
                    AutoSize = true
                };
                card.Controls.Add(lblReject);
                lineY += 22;
            }

            if (canCancel)
            {
                var btnCancel = new Button
                {
                    Text = "예약 취소",
                    Font = Theme.FontSmall,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.BgSecondary,
                    ForeColor = Theme.Danger,
                    Size = new Size(72, 24),
                    Location = new Point(14, lineY),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Theme.DangerLight;
                btnCancel.Click += (s, e) =>
                {
                    if (MessageBox.Show("예약을 취소하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        DatabaseHelper.ExecuteNonQuery($"DELETE FROM reservations WHERE id={id}");
                        LoadMyReservations();
                    }
                };
                card.Controls.Add(btnCancel);
            }

            return card;
        }

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