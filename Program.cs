using System;
using System.Windows.Forms;
using BILCAM.Database;
using BILCAM.Forms;

namespace BILCAM
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseHelper.Initialize();
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초기화 오류: {ex.Message}", "BILCAM", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
