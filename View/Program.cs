using System;
using System.Windows.Forms;

namespace View
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var view = new View();

            var scene = new Scene(view);
            scene.Show();

            Application.Run(view);
        }
    }
}
