using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Panel _slotPanel;
        private Label _lblSelectedSlot;
        private Label _lblDateWarning;
        private TextBox _txtMemo;
        private string _selectedSlot;
        private List<string> _takenSlots = new List<string>();
        private List<DateTime> _takenDates = new List<DateTime>();

        private Panel _calPanel;
        private DateTime _currentMonth;
        private DateTime _selectedDate;
        private DateTime? _hoveredDate;

        private static readonly string[] ALL_SLOTS = {
            "09:00","10:00","11:00","12:00","13:00",
            "14:00","15:00","16:00","17:00","18:00"
        };

        private string[] DayNames => AppLanguage.Current == "en"
            ? new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" }
            : new[] { "일", "월", "화", "수", "목", "금", "토" };

        private const int CAL_WIDTH = 500;
        private const int CELL_W = 500 / 7;
        private const int CELL_H = 40;
        private const int HEADER_H = 48;
        private const int DAYNAME_H = 32;

        private bool IsDateOnly => _resourceCategory == "laptop" || _resourceCategory == "umbrella";

        public ReservationForm(User user, int resourceId, string resourceName, string resourceCategory)
        {
            _user = user;
            _resourceId = resourceId;
            _resourceName = resourceName;
            _resourceCategory = resourceCategory;
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _selectedDate = DateTime.Today;
            InitializeComponent();

            if (IsDateOnly)
                LoadTakenDates();
            else
                LoadTakenSlots(DateTime.Today);
        }

        private void InitializeComponent()
        {
            this.Text = $"{AppLanguage.Get("res_submit")} — {_resourceName}";
            this.Size = IsDateOnly ? new Size(560, 660) : new Size(560, 860);
            this.MinimumSize = this.Size;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.BgSecondary;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            var header = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Theme.BgPrimary };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Theme.Border), 0, 49, header.Width, 49);
            header.Controls.Add(new Label { Text = _resourceName, Font = Theme.FontBold, ForeColor = Theme.TextPrimary, AutoSize = true, Location = new Point(16, 15) });

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.BgSecondary, Padding = new Padding(20, 16, 20, 16) };
            var inner = new Panel { Width = 500, Height = IsDateOnly ? 560 : 780, Location = new Point(0, 0), BackColor = Theme.BgSecondary };

            int y = 0;

            inner.Controls.Add(new Label
            {
                Text = AppLanguage.Get("res_date"),
                Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                ForeColor = Theme.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            });
            y += 24;

            _calPanel = new Panel
            {
                Location = new Point(0, y),
                Width = CAL_WIDTH,
                Height = HEADER_H + DAYNAME_H + CELL_H * 6 + 8,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            _calPanel.Paint += DrawCalendar;
            _calPanel.MouseClick += CalMouseClick;
            _calPanel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _calPanel.ClientRectangle, Theme.Border, ButtonBorderStyle.Solid);

            inner.Controls.Add(_calPanel);
            y += _calPanel.Height + 12;

            if (IsDateOnly)
            {
                var legendPanel = new FlowLayoutPanel
                {
                    Location = new Point(0, y),
                    Width = 500,
                    Height = 22,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    BackColor = Theme.BgSecondary
                };
                legendPanel.Controls.Add(MakeLegendDot(Color.FromArgb(220, 220, 215), AppLanguage.Get("res_legend_unavail")));
                legendPanel.Controls.Add(MakeLegendDot(Theme.Primary, AppLanguage.Get("res_legend_selected")));
                inner.Controls.Add(legendPanel);
                y += 28;

                _lblDateWarning = new Label
                {
                    Text = AppLanguage.Get("res_date_select"),
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(0, y),
                    AutoSize = true
                };
                inner.Controls.Add(_lblDateWarning);
                y += 28;
            }
            else
            {
                inner.Controls.Add(new Label
                {
                    Text = AppLanguage.Get("res_time"),
                    Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(0, y),
                    AutoSize = true
                });
                y += 22;

                var legendPanel = new FlowLayoutPanel
                {
                    Location = new Point(0, y),
                    Width = 500,
                    Height = 26,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    BackColor = Theme.BgSecondary
                };
                legendPanel.Controls.Add(MakeLegend(AppLanguage.Get("res_legend_avail"), Theme.PrimaryLight, Theme.Primary));
                legendPanel.Controls.Add(MakeLegend(AppLanguage.Get("res_legend_taken"), Color.FromArgb(220, 220, 215), Theme.TextMuted));
                legendPanel.Controls.Add(MakeLegend(AppLanguage.Get("res_legend_selected"), Theme.Primary, Color.White));
                inner.Controls.Add(legendPanel);
                y += 32;

                _slotPanel = new Panel { Location = new Point(0, y), Width = 500, Height = 84, BackColor = Theme.BgSecondary };
                inner.Controls.Add(_slotPanel);
                y += 92;

                _lblSelectedSlot = new Label
                {
                    Text = AppLanguage.Get("res_no_time"),
                    Font = Theme.FontSmall,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(0, y),
                    AutoSize = true
                };
                inner.Controls.Add(_lblSelectedSlot);
                y += 30;
            }

            y += 6;
            inner.Controls.Add(new Label
            {
                Text = AppLanguage.Get("res_memo"),
                Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
                ForeColor = Theme.TextSecondary,
                Location = new Point(0, y),
                AutoSize = true
            });
            y += 22;

            _txtMemo = new TextBox
            {
                Location = new Point(0, y),
                Width = 500,
                Height = 60,
                Multiline = true,
                Font = Theme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 200
            };
            inner.Controls.Add(_txtMemo);
            y += _txtMemo.Height + 16;

            var btnSubmit = new Button
            {
                Text = AppLanguage.Get("res_submit"),
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

        private void DrawCalendar(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.FillRectangle(new SolidBrush(Theme.Primary), 0, 0, CAL_WIDTH, HEADER_H);

            var arrowFont = new Font("맑은 고딕", 14f, FontStyle.Bold);
            g.DrawString("‹", arrowFont, Brushes.White, new RectangleF(8, 8, 32, 32), CenterFormat());
            g.DrawString("›", arrowFont, Brushes.White, new RectangleF(CAL_WIDTH - 40, 8, 32, 32), CenterFormat());

            string monthTitle = AppLanguage.Current == "en"
                ? _currentMonth.ToString("MMMM yyyy")
                : _currentMonth.ToString("yyyy년 MM월");
            var titleFont = new Font("맑은 고딕", 12f, FontStyle.Bold);
            g.DrawString(monthTitle, titleFont, Brushes.White, new RectangleF(40, 0, CAL_WIDTH - 80, HEADER_H), CenterFormat());

            string[] dayNames = DayNames;
            for (int i = 0; i < 7; i++)
            {
                Color dayColor = i == 0 ? Color.FromArgb(255, 100, 100) : i == 6 ? Color.FromArgb(100, 150, 255) : Theme.TextSecondary;
                var dayFont = new Font("맑은 고딕", 9f, FontStyle.Bold);
                g.DrawString(dayNames[i], dayFont, new SolidBrush(dayColor),
                    new RectangleF(i * CELL_W, HEADER_H, CELL_W, DAYNAME_H), CenterFormat());
            }

            g.DrawLine(new Pen(Theme.Border), 0, HEADER_H + DAYNAME_H - 1, CAL_WIDTH, HEADER_H + DAYNAME_H - 1);

            int firstDay = (int)_currentMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            DateTime maxDate = DateTime.Today.AddMonths(1);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
                int idx = day - 1 + firstDay;
                int col = idx % 7;
                int row = idx / 7;

                float x = col * CELL_W;
                float cellY = HEADER_H + DAYNAME_H + row * CELL_H;
                var cellRect = new RectangleF(x, cellY, CELL_W, CELL_H);

                bool isToday = date.Date == DateTime.Today;
                bool isSelected = date.Date == _selectedDate.Date;
                bool isTaken = _takenDates.Contains(date.Date);
                bool isPast = date.Date < DateTime.Today;
                bool isFuture = date.Date > maxDate;
                bool isDisabled = isPast || isFuture;

                float circleSize = 32f;
                float cx = x + CELL_W / 2f - circleSize / 2f;
                float cy = cellY + CELL_H / 2f - circleSize / 2f;
                var circleRect = new RectangleF(cx, cy, circleSize, circleSize);

                if (isSelected && !isDisabled)
                    g.FillEllipse(new SolidBrush(Theme.Primary), circleRect);
                else if (isToday && !isSelected)
                    g.DrawEllipse(new Pen(Theme.Primary, 2f), circleRect);
                else if (isTaken && !isDisabled)
                    g.FillEllipse(new SolidBrush(Color.FromArgb(220, 220, 215)), circleRect);

                Color textColor;
                if (isSelected && !isDisabled) textColor = Color.White;
                else if (isDisabled) textColor = Color.FromArgb(200, 200, 200);
                else if (isTaken) textColor = Color.FromArgb(160, 160, 160);
                else if (col == 0) textColor = Color.FromArgb(220, 80, 80);
                else if (col == 6) textColor = Color.FromArgb(80, 120, 220);
                else textColor = Theme.TextPrimary;

                var dateFont = new Font("맑은 고딕", 10f, isToday ? FontStyle.Bold : FontStyle.Regular);
                g.DrawString(day.ToString(), dateFont, new SolidBrush(textColor), cellRect, CenterFormat());
            }
        }

        private StringFormat CenterFormat() => new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private DateTime? GetDateFromPoint(Point p)
        {
            if (p.Y < HEADER_H + DAYNAME_H) return null;
            int col = p.X / CELL_W;
            int row = (p.Y - HEADER_H - DAYNAME_H) / CELL_H;
            if (col < 0 || col > 6) return null;
            int firstDay = (int)_currentMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            int dayIdx = row * 7 + col - firstDay + 1;
            if (dayIdx < 1 || dayIdx > daysInMonth) return null;
            return new DateTime(_currentMonth.Year, _currentMonth.Month, dayIdx);
        }

        private void CalMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Y < HEADER_H)
            {
                if (e.X < 48)
                {
                    _currentMonth = _currentMonth.AddMonths(-1);
                    if (_currentMonth < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1))
                        _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                else if (e.X > CAL_WIDTH - 48)
                {
                    _currentMonth = _currentMonth.AddMonths(1);
                    if (_currentMonth > DateTime.Today.AddMonths(1))
                        _currentMonth = new DateTime(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month, 1);
                }
                _calPanel.Invalidate();
                return;
            }

            var date = GetDateFromPoint(e.Location);
            if (date == null) return;

            DateTime d = date.Value;
            if (d.Date < DateTime.Today || d.Date > DateTime.Today.AddMonths(1)) return;
            if (IsDateOnly && _takenDates.Contains(d.Date)) return;

            _selectedDate = d;
            _calPanel.Invalidate();

            if (IsDateOnly)
            {
                bool isTaken = _takenDates.Contains(d.Date);
                _lblDateWarning.Text = isTaken ? AppLanguage.Get("res_date_no") : AppLanguage.Get("res_date_ok");
                _lblDateWarning.ForeColor = isTaken ? Theme.Danger : Theme.Success;
            }
            else
            {
                _selectedSlot = null;
                if (_lblSelectedSlot != null) _lblSelectedSlot.Text = AppLanguage.Get("res_no_time");
                LoadTakenSlots(d);
            }
        }

        private void LoadTakenDates()
        {
            _takenDates.Clear();
            var dt = DatabaseHelper.ExecuteQuery(
                $"SELECT reservationdate FROM reservations WHERE resourceid={_resourceId} AND status != 'rejected'");
            foreach (DataRow row in dt.Rows)
            {
                if (DateTime.TryParse(row["reservationdate"].ToString(), out DateTime d))
                    _takenDates.Add(d.Date);
            }
            _calPanel?.Invalidate();
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
            int slotW = 88; int slotH = 34; int gapX = 8; int gapY = 8; int perRow = 5;

            for (int i = 0; i < ALL_SLOTS.Length; i++)
            {
                string slot = ALL_SLOTS[i];
                bool taken = _takenSlots.Contains(slot);
                bool selected = slot == _selectedSlot;
                int col = i % perRow; int row = i / perRow;

                var btn = new Button
                {
                    Text = slot,
                    Font = Theme.FontSmall,
                    Size = new Size(slotW, slotH),
                    Location = new Point(col * (slotW + gapX), row * (slotH + gapY)),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = taken ? Cursors.Default : Cursors.Hand,
                    Enabled = !taken,
                    BackColor = taken ? Color.FromArgb(220, 220, 215) : selected ? Theme.Primary : Theme.PrimaryLight,
                    ForeColor = taken ? Theme.TextMuted : selected ? Color.White : Theme.Primary
                };
                btn.FlatAppearance.BorderColor = taken ? Theme.Border : Theme.Primary;
                btn.FlatAppearance.BorderSize = 1;

                if (!taken)
                {
                    string s = slot;
                    btn.Click += (sender, e) =>
                    {
                        _selectedSlot = s;
                        _lblSelectedSlot.Text = $"{AppLanguage.Get("res_selected_time")}{s} ~ {GetEndTime(s)}";
                        RenderSlots();
                    };
                }
                _slotPanel.Controls.Add(btn);
            }
        }

        private string GetEndTime(string start)
        {
            int idx = Array.IndexOf(ALL_SLOTS, start);
            return idx + 1 < ALL_SLOTS.Length ? ALL_SLOTS[idx + 1] : $"{int.Parse(start.Split(':')[0]) + 1}:00";
        }

        private Label MakeLegend(string text, Color bg, Color fg) => new Label
        {
            Text = $"  {text}  ",
            Font = Theme.FontSmall,
            BackColor = bg,
            ForeColor = fg,
            AutoSize = true,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 8, 0)
        };

        private FlowLayoutPanel MakeLegendDot(Color color, string text)
        {
            var pnl = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Theme.BgSecondary, Margin = new Padding(0, 0, 12, 0) };
            pnl.Controls.Add(new Panel { Width = 12, Height = 12, BackColor = color, Margin = new Padding(0, 4, 4, 0) });
            pnl.Controls.Add(new Label { Text = text, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Margin = new Padding(0, 2, 0, 0) });
            return pnl;
        }

        private void DoSubmit(object sender, EventArgs e)
        {
            string dateStr = _selectedDate.ToString("yyyy-MM-dd");
            string memo = _txtMemo.Text.Trim().Replace("'", "''");

            if (IsDateOnly)
            {
                if (_takenDates.Contains(_selectedDate.Date))
                {
                    MessageBox.Show(AppLanguage.Get("res_warn_taken_date"), "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DatabaseHelper.ExecuteNonQuery(
                    $"INSERT INTO reservations (userid, resourceid, reservationdate, starttime, endtime, status, createdat, memo) " +
                    $"VALUES ('{_user.UserId}', {_resourceId}, '{dateStr}', '00:00', '23:59', 'pending', '{DateTime.Now}', '{memo}')");
            }
            else
            {
                if (_selectedSlot == null)
                {
                    MessageBox.Show(AppLanguage.Get("res_warn_no_slot"), "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string endTime = GetEndTime(_selectedSlot);
                var check = DatabaseHelper.ExecuteQuery(
                    $"SELECT id FROM reservations WHERE resourceid={_resourceId} AND reservationdate='{dateStr}' AND status != 'rejected' AND '{_selectedSlot}' < endtime AND '{endTime}' > starttime");

                if (check.Rows.Count > 0)
                {
                    MessageBox.Show(AppLanguage.Get("res_warn_slot_taken"), "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadTakenSlots(_selectedDate);
                    return;
                }
                DatabaseHelper.ExecuteNonQuery(
                    $"INSERT INTO reservations (userid, resourceid, reservationdate, starttime, endtime, status, createdat, memo) " +
                    $"VALUES ('{_user.UserId}', {_resourceId}, '{dateStr}', '{_selectedSlot}', '{endTime}', 'pending', '{DateTime.Now}', '{memo}')");
            }

            MessageBox.Show(AppLanguage.Get("res_success"), "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}