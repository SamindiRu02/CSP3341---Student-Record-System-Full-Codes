
using System;
using System.Windows.Forms;

namespace StudentRecordSystem;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Initialize windows forms and open the main application window
        ApplicationConfiguration.Initialize();
        System.Windows.Forms.Application.Run(new MainForm());
    }
}
