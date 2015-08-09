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

namespace ScreenBrightness
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
        }
    }
}
