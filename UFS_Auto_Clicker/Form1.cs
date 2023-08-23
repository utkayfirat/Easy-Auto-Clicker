using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UFS_Auto_Clicker{
    public partial class Form1 : Form{

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT{
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        bool isRunning = false;
        bool oneTime = false;

        private const int LEFTUP = 0x0004;
        private const int LEFTDOWN = 0x0002;
        private int intervals = 1;

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp){
            if (nCode >= 0){
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                if (oneTime) {
                    oneTime = false;
                    return (IntPtr)0;
                }
                if (objKeyInfo.key == Keys.F1){
                    isRunning = !isRunning;
                    if (isRunning){
                        headButtonSetText("Press [F1] to stop");
                    }else {
                        headButtonSetText("Press [F1] to click");
                    }
                    oneTime = true;
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        private void headButtonSetText(string text) {
            headButton.Text = text;
        }

        public Form1(){
            CheckForIllegalCrossThreadCalls = false;
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            InitializeComponent();
            objKeyboardProcess = new LowLevelKeyboardProc(CaptureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
            Thread autoClicker = new Thread(AutoClick);
            autoClicker.Start();
        }

        private void AutoClick() {
            while (true) {
                if (isRunning) {
                    mouse_event(dwFlags: LEFTUP, dx: 0, dy: 0, cButtons: 0, dwExtraInfo: 0);
                    Thread.Sleep(1);
                    mouse_event(dwFlags: LEFTDOWN, dx: 0, dy: 0, cButtons: 0, dwExtraInfo: 0);
                    Thread.Sleep(intervals);
                }
                //Thread.Sleep(1);
            }
        }

        private void clickInterval_ValueChanged(object sender, EventArgs e){
            intervals = (int)clickInterval.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Application.Exit();
            Environment.Exit(1);
        }



        [DllImport("user32.dll")]
        public static extern int ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr Hwnd, int msg, int param, int lparam);


        private void specialMover(MouseEventArgs e){
            if (e.Button == MouseButtons.Left){
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e){
            specialMover(e);
        }

        private void label2_MouseDown(object sender, MouseEventArgs e){
            specialMover(e);
        }
    }
}
