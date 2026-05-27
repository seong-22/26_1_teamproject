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
        private string _resourceCategory;
        private MonthCalendar _calendar;
        private Panel _slotPanel;
        private Label _lblSelectedSlot;
        private string _selectedSlot;
        private List<string> _takenSlots = new List<string>();

        private static readonly string[] ALL_SLOTS = {
            "09:00","10:00","11:00","12:00","13:00",
            "14:00","15:00","16:00","17:00","18:00"
        };

        // 강의실만 시간 선택, 노트북/우산은 날짜만
        private bool IsDateOnly => _resourceCategory == "laptop" || _resourceCategory == "umbrella";

        public ReservationForm(User user, int resourceId, string resourceName, string resourceCategory)
        {
            _user = user;
            _resourceId = resourceId;
            _resourceName = resourceName;
            _resourceCategory = resourceCategory;
            InitializeComponent();
            if (!IsDateOnly)
                LoadTakenSlots(DateTime.Today);
        }

        private void InitializeComponent()
        {
            this.Text = $"예약 신청 — {_resourceName}";
            this.Size = IsDateOnly ? new Size(560, 460) : new Size(560, 620);
            this.MinimumSize = this.Size;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 헤더
            var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Theme.BgPrimary };
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

            // 스크롤 메인 영역
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
                Height = IsDateOnly ? 360 : 530,
                Location = new Point(0, 0),
                BackColor = Theme.BgSecondary
            };

            int y = 0;

            // 날짜 선택 라벨
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
                if (!IsDateOnly)
                {
                    _selectedSlot = null;
                    if (_lblSelectedSlot != null)
                        _lblSelectedSlot.Text = "선택된 시간: 없음";
                    LoadTakenSlots(_calendar.SelectionStart);
                }
            };
            inner.Controls.Add(_calendar);
            y += _calendar.Height + 16;

            if (!IsDateOnly)
            {
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

                // 슬롯 패널
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
            }
            else
            {
                // 날짜만 선택하는 경우 안내 문구
                inner.Controls.Add(new Label
                {
                    Text = "※ 하루 단위로 대여됩니다.",
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(0, y),
                    AutoSize = true
                });
                y += 30;
            }

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
                $"SELECT starttime FROM reservations WHERE resourceid={_resourceId} AND reservationdate='{date:yyyy-MM-dd}' AND status != 'rejected'");

            foreach (DataRow row in dt.Rows)
                _takenSlots.Add(row["starttime"].ToString());

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
            string dateStr = _calendar.SelectionStart.ToString("yyyy-MM-dd");

            if (IsDateOnly)
            {
                // 날짜만 선택 (노트북/우산) — 하루 단위 중복 확인
                var check = DatabaseHelper.ExecuteQuery(
                    $"SELECT id FROM reservations WHERE resourceid={_resourceId} AND reservationdate='{dateStr}' AND status != 'rejected'");

                if (check.Rows.Count > 0)
                {
                    MessageBox.Show("해당 날짜는 이미 예약되어 있습니다.", "BILCAM",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DatabaseHelper.ExecuteNonQuery(
                    $"INSERT INTO reservations (userid, resourceid, reservationdate, starttime, endtime, status, createdat) " +
                    $"VALUES ('{_user.UserId}', {_resourceId}, '{dateStr}', '00:00', '23:59', 'pending', '{DateTime.Now}')");
            }
            else
            {
                // 시간 선택 (강의실)
                if (_selectedSlot == null)
                {
                    MessageBox.Show("시간대를 선택하세요.", "BILCAM",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string endTime = GetEndTime(_selectedSlot);

                var check = DatabaseHelper.ExecuteQuery(
                    $"SELECT id FROM reservations WHERE resourceid={_resourceId} AND reservationdate='{dateStr}' AND status != 'rejected' AND '{_selectedSlot}' < endtime AND '{endTime}' > starttime");

                if (check.Rows.Count > 0)
                {
                    MessageBox.Show("해당 시간대는 이미 예약되어 있습니다.", "BILCAM",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadTakenSlots(_calendar.SelectionStart);
                    return;
                }

                DatabaseHelper.ExecuteNonQuery(
                    $"INSERT INTO reservations (userid, resourceid, reservationdate, starttime, endtime, status, createdat) " +
                    $"VALUES ('{_user.UserId}', {_resourceId}, '{dateStr}', '{_selectedSlot}', '{endTime}', 'pending', '{DateTime.Now}')");
            }

            MessageBox.Show("예약 신청이 완료되었습니다!\n관리자 승인 후 확정됩니다.", "BILCAM",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}