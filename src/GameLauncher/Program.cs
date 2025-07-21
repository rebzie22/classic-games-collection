using GameLauncher.Forms;

namespace GameLauncher
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Set up global exception handling
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // Enable visual styles for better rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Use the standard Application.Run with main form
            Application.Run(new MainForm());
        }
        
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"An unexpected error occurred:\n{e.Exception.Message}", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"A fatal error occurred:\n{ex.Message}", 
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
