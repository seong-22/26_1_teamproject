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
        private FlowLayoutPanel _pnlResources, _pnlMyRes;

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
            lblUser.Location = new Point(this.Width - lblUser.PreferredWidth - 100, 18);
            lblUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;

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

            header.Controls.AddRange(new Control[] { lblTitle, lblUser, btnLogout });
            header.Layout += (s, e) =>
            {
                btnLogout.Location = new Point(header.Width - 86, 13);
                lblUser.Location = new Point(header.Width - 90 - lblUser.PreferredWidth - 8, 18);
            };

            // Tabs
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = Theme.FontBody,
                Padding = new Point(16, 6)
            };

            var tabResources = new TabPage("  자원 조회  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };
            var tabMyRes = new TabPage("  내 예약  ") { BackColor = Theme.BgTertiary, Padding = new Padding(10) };

            // Resources scroll panel
            _pnlResources = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(4)
            };
            tabResources.Controls.Add(_pnlResources);

            // My reservations panel
            _pnlMyRes = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(4)
            };
            tabMyRes.Controls.Add(_pnlMyRes);

            _tabs.TabPages.Add(tabResources);
            _tabs.TabPages.Add(tabMyRes);
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (_tabs.SelectedIndex == 1) LoadMyReservations();
            };

            this.Controls.Add(_tabs);
            this.Controls.Add(header);
        }

        // ── Resources ──────────────────────────────────────────────────────
        private void LoadResources()
        {
            _pnlResources.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Resources ORDER BY Category, Id");

            string currentCat = "";
            foreach (DataRow row in dt.Rows)
            {
                string cat = row["Category"].ToString();
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

                bool avail = Convert.ToInt32(row["IsAvailable"]) == 1;
                var card = BuildResourceCard(
                    Convert.ToInt32(row["Id"]),
                    row["Name"].ToString(),
                    row["Location"].ToString(),
                    avail);
                _pnlResources.Controls.Add(card);
            }
        }

        private Panel BuildResourceCard(int id, string name, string location, bool available)
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

            var lblName = new Label
            {
                Text = name,
                Font = Theme.FontBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(14, 14),
                AutoSize = true
            };
            var lblLoc = new Label
            {
                Text = location,
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                Location = new Point(14, 36),
                AutoSize = true
            };

            Color dotColor = available ? Theme.Success : Theme.Danger;
            Color dotBg = available ? Theme.SuccessLight : Theme.DangerLight;
            string statusText = available ? "예약 가능" : "사용 중";

            var badge = new Label
            {
                Text = $"  {statusText}  ",
                Font = Theme.FontSmall,
                BackColor = dotBg,
                ForeColor = dotColor,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardWidth - 100, 22)
            };
            badge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            card.Controls.AddRange(new Control[] { lblName, lblLoc, badge });

            if (available)
            {
                card.Click += (s, e) => OpenReservation(id, name);
                lblName.Click += (s, e) => OpenReservation(id, name);
                lblLoc.Click += (s, e) => OpenReservation(id, name);
                card.MouseEnter += (s, e) => card.BackColor = Theme.BgSecondary;
                card.MouseLeave += (s, e) => card.BackColor = Theme.BgPrimary;
            }

            return card;
        }

        private void OpenReservation(int resourceId, string resourceName)
        {
            var form = new ReservationForm(_user, resourceId, resourceName);
            form.ShowDialog(this);
            LoadResources();
        }

        // ── My Reservations ────────────────────────────────────────────────
        private void LoadMyReservations()
        {
            _pnlMyRes.Controls.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                @"SELECT r.*, res.Name as ResourceName FROM Reservations r
                  JOIN Resources res ON r.ResourceId = res.Id
                  WHERE r.UserId = @uid ORDER BY r.ReservationDate DESC",
                new System.Data.SQLite.SQLiteParameter("@uid", _user.UserId));

            if (dt.Rows.Count == 0)
            {
                _pnlMyRes.Controls.Add(new Label
                {
                    Text = "예약 내역이 없습니다.",
                    Font = Theme.FontBody,
                    ForeColor = Theme.TextMuted,
                    AutoSize = true,
                    Margin = new Padding(8, 24, 0, 0)
                });
                return;
            }

            foreach (DataRow row in dt.Rows)
                _pnlMyRes.Controls.Add(BuildMyResCard(row));
        }

        private Panel BuildMyResCard(DataRow row)
        {
            int id = Convert.ToInt32(row["Id"]);
            string status = row["Status"].ToString();
            string statusText = status == "pending" ? "승인 대기" : status == "approved" ? "승인됨" : "반려됨";
            Color statusBg = status == "pending" ? Theme.WarningLight : status == "approved" ? Theme.SuccessLight : Theme.DangerLight;
            Color statusFg = status == "pending" ? Theme.Warning : status == "approved" ? Theme.Success : Theme.Danger;

            int cardWidth = Math.Max(500, _pnlMyRes.ClientSize.Width - 24);
            var card = new Panel
            {
                Width = cardWidth,
                Height = status == "pending" ? 90 : 72,
                BackColor = Theme.BgPrimary,
                Margin = new Padding(2, 0, 2, 6)
            };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            var lblName = new Label { Text = row["ResourceName"].ToString(), Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Location = new Point(14, 12), AutoSize = true };
            var lblDetail = new Label
            {
                Text = $"{row["ReservationDate"]}  {row["StartTime"]} ~ {row["EndTime"]}",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(14, 34),
                AutoSize = true
            };
            var badge = new Label
            {
                Text = $"  {statusText}  ",
                Font = Theme.FontSmall,
                BackColor = statusBg,
                ForeColor = statusFg,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(cardWidth - 100, 12)
            };
            badge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            card.Controls.AddRange(new Control[] { lblName, lblDetail, badge });

            if (status == "pending")
            {
                var btnCancel = new Button
                {
                    Text = "예약 취소",
                    Font = Theme.FontSmall,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.BgSecondary,
                    ForeColor = Theme.Danger,
                    Size = new Size(72, 24),
                    Location = new Point(14, 56),
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderColor = Theme.DangerLight;
                btnCancel.Click += (s, e) =>
                {
                    if (MessageBox.Show("예약을 취소하시겠습니까?", "확인", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM Reservations WHERE Id=@id",
                            new System.Data.SQLite.SQLiteParameter("@id", id));
                        LoadMyReservations();
                    }
                };
                card.Controls.Add(btnCancel);
            }

            return card;
        }
    }
}
