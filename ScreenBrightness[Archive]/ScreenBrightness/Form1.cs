///<summary>
///
///Tiny tool for quickly adjusting screen brightness of laptops and tablets
///Tested on win7 (x86)
///needs .net 4 (but should compile on lower versions, too...probably You have to rebuild the form - dunno)
///Does not work on normal pcs...as far as I know...
///free to use and whatever...but not for sale...^^
///code for wmi queries stolen from Samuel Lai http://edgylogic.com/projects/display-brightness-vista-gadget/ :)
///
///</summary>


///<usage>
///
/// started without arguments, the prog icon will show up in the taskbar info area
/// you can access the trackbar by left-clicking the icon
/// right-clicking will show an exit button to close the app
/// 
/// Arguments:
/// started with the argument "show" (without quotes) the trackbar will be shown initially and hide after 4 secs of inactivity
/// (update: "show" is no longer queried, but never the less You will gain the same effect, when using it or any other argument that is not listet below...(e.g. qwerty)
///
/// started with the argument "quit" (without quotes) the trackbar shows up and the prog will be closed after 4 secs of inactivity
/// 
/// started with the argument "hide" (without quotes) the trackbar won't show up - this can be used in combination with a level value as follows:
///
/// You can set the desired level also as argument, e.g. 55%  - the level is set to the next higher available level, if the argument is not in the level array
/// 
/// arguments are seperated by blanks: e.g ScreenBrightness 88% hide -> the level will be set to 88% and the trackbar is hidden
/// 
/// the order of arguments is not relevant
/// 
///</usage>
///
///<todo>
///changing the brightness elsewere while the prog is running is not recognized by the prog (so the slider will show a diffrent value..)
///if You don't like that, You could update the trackbar in a separate timerevent by calling "check_brightness()"
///</todo>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenBrightness
{    
    public partial class Form1 : Form
    {
        //global current value
        int current;
        int iCount = 0; //global counter for hiding/closing form after certian period of inactivity
        byte[] bLevels; //array of valid level values
        string[] arguments; 

        public Form1(string[]args)
        {
            arguments = args;
            InitializeComponent();
        }

        //Custom trackbar
        //In case of an incompatible system, the form has to be shown in order to close the app...as far as I know ^^
        private void Form1_Shown(object sender, EventArgs e)
        {
            bLevels = GetBrightnessLevels(); //get the level array for this system
            if (bLevels.Count() == 0) //"WmiMonitorBrightness" is not supported by the system
            {
                Application.Exit();
            }
            else
            {
                //trackBar1.TickFrequency = bLevels.Count(); //adjust the trackbar ticks according the number of possible brightness levels
                trackBar1.Maximum = bLevels.Count() - 1;
                trackBar1.Update();
                trackBar1.Refresh();
                check_brightness();
                timer1.Enabled = true;  //timer for closing form
                //check the arguments
                if (Array.FindIndex(arguments, item => item.Contains("%") ) > -1)
                    startup_brightness();
                //if (arguments.Length == 0 || Array.IndexOf(arguments, "hide")>-1) //hide the trackbar initially if no arguments are passed
                //   // this.Hide();
            }
        }
        private void check_brightness()
        {
                int iBrightness = GetBrightness(); //get the actual value of brightness
                int i = Array.IndexOf(bLevels, (byte)iBrightness); 
                if (i < 0) i = 1;
                change_icon(iBrightness); 
                trackBar1.Value = i;
                if ((iBrightness < current) || (iBrightness > current))
                {
                    current = iBrightness;
                }
        }

        private void startup_brightness()
        {
            string sPercent = arguments[Array.FindIndex(arguments, item => item.Contains("%"))];
            if (sPercent.Length > 1)
            {
                int iPercent = Convert.ToInt16(sPercent.Split('%').ElementAt(0));
                if (iPercent >= 0 && iPercent <= bLevels[bLevels.Count() - 1])
                {
                    byte level =100;
                    foreach (byte item in bLevels)
                    {
                        if (item >= iPercent)
                        {
                            level = item;
                            break;
                        }
                    }
                    SetBrightness(level);
                    check_brightness();
                }
                    
            }
        }

        //change the icon according to brightness
        private void change_icon(int iBrightness)
        {            
            label1.Text = iBrightness.ToString();
            //visibility toggle
            label1.Visible = true;
            label2.Visible = true;
            //positionfor label 1
            label1.Left = trackBar1.Left - 40;
            label1.Top = trackBar1.Top + (100 - iBrightness);
            label2.Left = label1.Left - 2;
            label2.Top = label1.Top - 2;
            // heights for label 2
            label2.Width = label1.Width + 4;
            label2.Height = label1.Height + 4;            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            SetBrightness(bLevels[trackBar1.Value]);
            change_icon(bLevels[trackBar1.Value]);
            iCount = 0; //reset inactivity counter
            a = 0;
            this.Opacity = 100;
            this.Visible = true;
        }



        //get the actual percentage of brightness
        static int GetBrightness()
        {
            //define scope (namespace)
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

            //define query
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightness");

            //output current brightness
            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);

            System.Management.ManagementObjectCollection moc = mos.Get();

            //store result
            byte curBrightness = 0;
            foreach (System.Management.ManagementObject o in moc)
            {
                curBrightness = (byte)o.GetPropertyValue("CurrentBrightness");
                break; //only work on the first object
            }

            moc.Dispose();
            mos.Dispose();

            return (int)curBrightness;
        }

        //array of valid brightness values in percent
        static byte[] GetBrightnessLevels()
        {
            //define scope (namespace)
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

            //define query
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightness");

            //output current brightness
            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);
            byte[] BrightnessLevels = new byte[0];

            try
            {
                System.Management.ManagementObjectCollection moc = mos.Get();

                    //store result
 

                foreach (System.Management.ManagementObject o in moc)
                {
                    BrightnessLevels = (byte[])o.GetPropertyValue("Level");
                    break; //only work on the first object
                }

                moc.Dispose();
                mos.Dispose();

            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, Your System does not support this brightness control...");
                
            }

            return BrightnessLevels;
        }

        static void SetBrightness(byte targetBrightness)
        {
            //define scope (namespace)
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

            //define query
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");

            //output current brightness
            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);

            System.Management.ManagementObjectCollection moc = mos.Get();

            foreach (System.Management.ManagementObject o in moc)
            {
                o.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, targetBrightness }); //note the reversed order - won't work otherwise!
                break; //only work on the first object
            }

            moc.Dispose();
            mos.Dispose();
        }

        //timer for hiding/closing form
        double a = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            iCount++;            
            if (iCount > 5)
            {
                label1.Visible = false;
                label2.Visible = false;
            }
            if ((iCount > 18) && (this.Opacity >= 0.0))
            {
                a += 0.5;
                this.Opacity -= (System.Math.Log(a) / 10);
            }

            if (this.Opacity <= 0)
            {
                this.Visible = false;
            }
            //for change detection
            int iBrightness = GetBrightness(); //get the actual value of brightness
            int i = Array.IndexOf(bLevels, (byte)iBrightness);
            if ((iBrightness < current) || (iBrightness > current))
            {
                iCount = 0;
                a = 0;
                this.Opacity = 100;
                this.Visible = true;
                current = iBrightness;
                check_brightness();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {                
                check_brightness();
                this.Show();
                this.Activate();
            }           
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void notifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
            notifyIcon1.Text = "screen brightness " + GetBrightness().ToString() + "%";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.BackgroundBrush = new SolidBrush(SystemColors.ControlDarkDark);
            trackBar1.ProgressBackgroundBrush = new SolidBrush(StartMenu.getSecondary());
        }

        private void trackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            SetBrightness(bLevels[trackBar1.Value]);
            change_icon(bLevels[trackBar1.Value]);
            iCount = 0; //reset inactivity counter
        }

        private void trackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            Color now = StartMenu.getSecondary();
            now = ControlPaint.Light(now, 25);
            trackBar1.BackgroundBrush = new SolidBrush(SystemColors.ControlDark);
            trackBar1.ProgressBackgroundBrush = new SolidBrush(now);
        }

        private void trackBar1_MouseLeave(object sender, EventArgs e)
        {
            trackBar1.BackgroundBrush = new SolidBrush(SystemColors.ControlDarkDark);
            trackBar1.ProgressBackgroundBrush = new SolidBrush(StartMenu.getSecondary());
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            SetBrightness(bLevels[trackBar1.Value]);
            change_icon(bLevels[trackBar1.Value]);
            iCount = 0; //reset inactivity counter
        }
    }
    //getting system window colour
    public class StartMenu
    {
        private static string[] PrimaryHex = new string[] { "252525", "252525", "252525", "252525", "311600", "550004", "4f023a", "2e0251", "1d0065", "00214f", "00485e", "004900", "17992c", "e66b1b", "b91d1e", "b31a6a", "681cb4", "1957ba", "599be5", "00a8a8", "82ba1b", "d39d09", "e165bb", "686868", "686868" };
        private static string[] SecondaryHex = new string[] { "f4b300", "78ba00", "2773ed", "ae113e", "632f00", "b11d01", "c1004f", "7200ad", "4617b5", "006ac0", "008387", "189900", "00c140", "ff991d", "ff2e13", "ff1d77", "aa3fff", "20aeff", "57c5ff", "00d8cd", "91d100", "e1b700", "ff76bc", "00a4a5", "ff7d23" };

        private static int _Default = 0;
        public static int Default
        {
            get
            {
                return _Default;
            }
            set
            {
                if (value < 0 || value > 25)
                    throw new IndexOutOfRangeException("Default value can only be 0-25");
                _Default = value;
            }
        }
        public static Color getPrimary()
        {
            return Color.FromArgb(Int32.Parse("FF" + PrimaryHex[getKey()], System.Globalization.NumberStyles.HexNumber));
        }

        public static Color getSecondary()
        {
            return Color.FromArgb(Int32.Parse("FF" + SecondaryHex[getKey()], System.Globalization.NumberStyles.HexNumber));
        }

        private static int getKey()
        {
            return (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent", "ColorSet_Version3", Default);
        }
    }


}
