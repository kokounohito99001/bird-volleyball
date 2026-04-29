using System;
using System.Windows.Forms;

namespace BirdVolleyball
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            Application.Run(new GameForm());
        }
    }
}
