
using System;
using System.Windows.Forms;
using MISS.Browser.Forms;

namespace MISS.Browser
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainBrowserForm());
        }
    }
}
