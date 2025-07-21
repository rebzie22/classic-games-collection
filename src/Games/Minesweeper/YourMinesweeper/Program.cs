using System;
using System.Windows.Forms;

namespace Minesweeper.YourMinesweeper
{
    // This class is not needed when launched from the game collection
    // but kept for reference if needed
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
