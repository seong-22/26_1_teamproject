using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BILCAM.Database;
using BILCAM.Models;

namespace BILCAM.Forms
{
    public class ReservationForm : Form
    {
        private User _user;
        private int _resourceId;
        private string _resourceName;
        private MonthCalendar _calendar;
        private Panel _slotPanel;
        private Label _lblSelectedSlot;
        private string _selectedSlot;
        private List<string> _takenSlots = new List<string>();

        private static readonly string[] ALL_SLOTS = {
            "09:00","10:00","11:00","12:00","13:00",
            "14:00","15:00","16:00","17:00","18:00"
        };

        public ReservationForm(User user, int resourceId, string resourceName)
        {
            _user = user;
            _resourceId = resourceId;
            _resourceName = resourceName;
            InitializeComponent();
            LoadTakenSlots(DateTime.Today);
        }

        private void InitializeComponent()
        {
            this.Text = $"예약 신청 — {_resourceName}";
            this.Size = new Size(560, 620);
            this.MinimumSize = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // ── 헤더 ──────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Theme.BgPrimary
            };
            header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Theme.Border), 0, 49, header.Width, 49);
            header.Controls.Add(new Label
            {
                Text = _resourceName,
                Font = Theme.FontBold,
                ForeColor = Theme.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 15)
            });

            // ── 스크롤 메인 영역 ───────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Theme.BgSecondary,
                Padding = new Padding(20, 16, 20, 16)
            };

            var inner = new Panel
            {
                Width = 500,
                Height = 530,
                Location = new Point(0, 0),
                BackColor = Theme.BgSecondary
            };

            int y = 0;

            // 날짜 선택
            inner.Controls.Add(new Label
            {
                Text = "날짜 선택",
                Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                ForeColor = Theme.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            });
            y += 22;

            // 캘린더
            _calendar = new MonthCalendar
            {
                Location = new Point(0, y),
                MaxSelectionCount = 1,
                MinDate = DateTime.Today,
                MaxDate = DateTime.Today.AddMonths(2),
                ShowToday = true,
                Font = Theme.FontSmall
            };
            _calendar.DateChanged += (s, e) =>
            {
                _selectedSlot = null;
                _lblSelectedSlot.Text = "선택된 시간: 없음";
                LoadTakenSlots(_calendar.SelectionStart);
            };
            inner.Controls.Add(_calendar);
            y += _calendar.Height + 16;

            // 시간대 라벨
            inner.Controls.Add(new Label
            {
                Text = "시간대 선택 (1시간 단위)",
                Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                ForeColor = Theme.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            });
            y += 22;

            // 범례
            var legendPanel = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Width = 500,
                Height = 26,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.BgSecondary
            };
            legendPanel.Controls.Add(MakeLegend("예약 가능", Theme.PrimaryLight, Theme.Primary));
            legendPanel.Controls.Add(MakeLegend("이미 예약됨", Color.FromArgb(220, 220, 215), Theme.TextMuted));
            legendPanel.Controls.Add(MakeLegend("선택됨", Theme.Primary, Color.White));
            inner.Controls.Add(legendPanel);
            y += 32;

            // 슬롯 패널 (5열 x 2행)
            _slotPanel = new Panel
            {
                Location = new Point(0, y),
                Width = 500,
                Height = 84,
                BackColor = Theme.BgSecondary
            };
            inner.Controls.Add(_slotPanel);
            y += 92;

            // 선택 시간 표시
            _lblSelectedSlot = new Label
            {
                Text = "선택된 시간: 없음",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            };
            inner.Controls.Add(_lblSelectedSlot);
            y += 30;

            // 예약 신청 버튼
            var btnSubmit = new Button
            {
                Text = "예약 신청",
                Font = Theme.FontBold,
                Size = new Size(500, 42),
                Location = new Point(0, y),
                BackColor = Theme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += DoSubmit;
            inner.Controls.Add(btnSubmit);

            scroll.Controls.Add(inner);
            this.Controls.Add(scroll);
            this.Controls.Add(header);
        }

        private Label MakeLegend(string text, Color bg, Color fg)
        {
            return new Label
            {
                Text = $"  {text}  ",
                Font = Theme.FontSmall,
                BackColor = bg,
                ForeColor = fg,
                AutoSize = true,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private void LoadTakenSlots(DateTime date)
        {
            _takenSlots.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                @"SELECT StartTime FROM Reservations
                  WHERE ResourceId=@rid AND ReservationDate=@date AND Status != 'rejected'",
                new System.Data.SQLite.SQLiteParameter("@rid", _resourceId),
                new System.Data.SQLite.SQLiteParameter("@date", date.ToString("yyyy-MM-dd")));

            foreach (DataRow row in dt.Rows)
                _takenSlots.Add(row["StartTime"].ToString());

            RenderSlots();
        }

        private void RenderSlots()
        {
            _slotPanel.Controls.Clear();

            int slotW = 88;
            int slotH = 34;
            int gapX = 8;
            int gapY = 8;
            int perRow = 5;

            for (int i = 0; i < ALL_SLOTS.Length; i++)
            {
                string slot = ALL_SLOTS[i];
                bool taken = _takenSlots.Contains(slot);
                bool selected = slot == _selectedSlot;

                int col = i % perRow;
                int row = i / perRow;

                var btn = new Button
                {
                    Text = slot,
                    Font = Theme.FontSmall,
                    Size = new Size(slotW, slotH),
                    Location = new Point(col * (slotW + gapX), row * (slotH + gapY)),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = taken ? Cursors.Default : Cursors.Hand,
                    Enabled = !taken,
                    BackColor = taken ? Color.FromArgb(220, 220, 215)
                               : selected ? Theme.Primary
                               : Theme.PrimaryLight,
                    ForeColor = taken ? Theme.TextMuted
                               : selected ? Color.White
                               : Theme.Primary
                };
                btn.FlatAppearance.BorderColor = taken ? Theme.Border : Theme.Primary;
                btn.FlatAppearance.BorderSize = 1;

                if (!taken)
                {
                    string s = slot;
                    btn.Click += (sender, e) =>
                    {
                        _selectedSlot = s;
                        _lblSelectedSlot.Text = $"선택된 시간: {s} ~ {GetEndTime(s)}";
                        RenderSlots();
                    };
                }

                _slotPanel.Controls.Add(btn);
            }
        }

        private string GetEndTime(string start)
        {
            int idx = Array.IndexOf(ALL_SLOTS, start);
            return idx + 1 < ALL_SLOTS.Length
                ? ALL_SLOTS[idx + 1]
                : $"{int.Parse(start.Split(':')[0]) + 1}:00";
        }

        private void DoSubmit(object sender, EventArgs e)
        {
            if (_selectedSlot == null)
            {
                MessageBox.Show("시간대를 선택하세요.", "BILCAM",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string endTime = GetEndTime(_selectedSlot);
            string dateStr = _calendar.SelectionStart.ToString("yyyy-MM-dd");

            // 중복 예약 방지
            var check = DatabaseHelper.ExecuteQuery(
                @"SELECT Id FROM Reservations
                  WHERE ResourceId=@rid AND ReservationDate=@date AND Status != 'rejected'
                    AND @start < EndTime AND @end > StartTime",
                new System.Data.SQLite.SQLiteParameter("@rid", _resourceId),
                new System.Data.SQLite.SQLiteParameter("@date", dateStr),
                new System.Data.SQLite.SQLiteParameter("@start", _selectedSlot),
                new System.Data.SQLite.SQLiteParameter("@end", endTime));

            if (check.Rows.Count > 0)
            {
                MessageBox.Show("해당 시간대는 이미 예약되어 있습니다.", "BILCAM",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadTakenSlots(_calendar.SelectionStart);
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO Reservations
                    (UserId, ResourceId, ReservationDate, StartTime, EndTime, Status, CreatedAt)
                  VALUES (@uid, @rid, @date, @start, @end, 'pending', @now)",
                new System.Data.SQLite.SQLiteParameter("@uid", _user.UserId),
                new System.Data.SQLite.SQLiteParameter("@rid", _resourceId),
                new System.Data.SQLite.SQLiteParameter("@date", dateStr),
                new System.Data.SQLite.SQLiteParameter("@start", _selectedSlot),
                new System.Data.SQLite.SQLiteParameter("@end", endTime),
                new System.Data.SQLite.SQLiteParameter("@now", DateTime.Now.ToString()));

            MessageBox.Show("예약 신청이 완료되었습니다!\n관리자 승인 후 확정됩니다.", "BILCAM",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}