///<summary>
///
///Tiny tool for quickly adjusting screen brightness of laptops and tablets
///Tested on win7 (x86)
///Does not work on normal pcs...as far as I know...
///free to use and whatever...but not for sale...
///Main code stolen from Samuel Lai http://edgylogic.com/projects/display-brightness-vista-gadget/ :)
///
///</summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenBrightness
{

    static class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1(args));
                }
                else
                {
                    MessageBox.Show("Another Instance of the application is already running !");
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }
        //[STAThread]
        //static void Main(string[] args)
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1(args));
        //}
    }
}
