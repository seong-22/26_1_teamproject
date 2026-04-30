using System.Drawing;
using System.Windows.Forms;

namespace BILCAM
{
    public static class Theme
    {
        // Colors
        public static Color Primary      = Color.FromArgb(24, 95, 165);   // Blue
        public static Color PrimaryLight = Color.FromArgb(230, 241, 251);
        public static Color Success      = Color.FromArgb(15, 110, 86);
        public static Color SuccessLight = Color.FromArgb(225, 245, 238);
        public static Color Warning      = Color.FromArgb(133, 79, 11);
        public static Color WarningLight = Color.FromArgb(250, 238, 218);
        public static Color Danger       = Color.FromArgb(163, 45, 45);
        public static Color DangerLight  = Color.FromArgb(252, 235, 235);
        public static Color Border       = Color.FromArgb(220, 220, 215);
        public static Color BgPrimary    = Color.White;
        public static Color BgSecondary  = Color.FromArgb(248, 248, 246);
        public static Color BgTertiary   = Color.FromArgb(241, 239, 232);
        public static Color TextPrimary  = Color.FromArgb(30, 30, 28);
        public static Color TextSecondary= Color.FromArgb(95, 94, 90);
        public static Color TextMuted    = Color.FromArgb(136, 135, 128);

        // Fonts
        public static Font FontTitle   = new Font("맑은 고딕", 20f, FontStyle.Bold);
        public static Font FontHeader  = new Font("맑은 고딕", 13f, FontStyle.Bold);
        public static Font FontBody    = new Font("맑은 고딕", 10f);
        public static Font FontSmall   = new Font("맑은 고딕", 9f);
        public static Font FontBold    = new Font("맑은 고딕", 10f, FontStyle.Bold);

        public static Button MakeButton(string text, Color bg, Color fg, int width = 120, int height = 34)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = height,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Font = FontBold,
                Cursor = Cursors.Hand
            };
        }

        public static Label MakeBadge(string text, Color bg, Color fg)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                BackColor = bg,
                ForeColor = fg,
                Font = FontSmall,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(4, 2, 4, 2)
            };
        }

        public static Panel MakeCard()
        {
            return new Panel
            {
                BackColor = BgPrimary,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 8)
            };
        }

        public static TextBox MakeInput(string placeholder = "", bool isPassword = false)
        {
            var tb = new TextBox
            {
                Font = FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 32,
                BackColor = BgPrimary,
                ForeColor = TextPrimary
            };
            if (isPassword) tb.UseSystemPasswordChar = true;
            return tb;
        }
    }
}
