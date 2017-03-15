using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CCS_EDIT_TERMINAL
{
    

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern int FindWindow(string strclassName, string strWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
        internal static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwindow, Int32 unindex);
        public const Int32 GWL_WNDPROC = -4;
        public const Int32 GWL_HINSTANCE = -6;
        public const Int32 GWL_HWNDPARENT = -8;
        public const Int32 GWL_STYLE = -16;
        public const Int32 GWL_EXSTYLE = -20;
        public const Int32 GWL_USERDATA = -21;
        public const Int32 GWL_ID = -12;

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        static extern uint GetWindowsDirectory(string buffer, uint length);



        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong(IntPtr hWnd, Int32 nIndex);
        public const Int32 GCW_ATOM = -32;
        public const Int32 GCL_CBCLSEXTRA = -20;
        public const Int32 GCL_CBWNDEXTRA = -18;
        public const Int32 GCL_HBRBACKGROUND = -10;
        public const Int32 GCL_HCURSOR = -12;
        public const Int32 GCL_HICON = -14;
        public const Int32 GCL_HMODULE = -16;
        public const Int32 GCL_MENUNAME = -8;
        public const Int32 GCL_STYLE = -26;
        public const Int32 GCL_WNDPROC = -24;

        public const Int32 WM_CHAR = 0x0102;
        public const Int32 WM_KEYDOWN = 0x0100;
        public const Int32 WM_KEYUP = 0x0101;
        public const Int32 VK_RETURN = 0x0D;

        private IntPtr g_lastHwnd = IntPtr.Zero;
        public Form1()
        {
            InitializeComponent();

            String[] args = Environment.GetCommandLineArgs();

            if (args.Length >= 2)
            {

                //IntPtr windowHandle = new IntPtr(Int32.Parse(args[1]));
                IntPtr windowHandle = GetCCS_TerminalHandle();

                if (windowHandle.Equals(IntPtr.Zero))
                {
                    Console.WriteLine("can't find handle");
                    Environment.Exit(0);
                }
                String cmd = string.Join(" ", System.Environment.GetCommandLineArgs().Skip(1));
                
                Console.WriteLine("Handle:" + windowHandle.ToString("X4") + " Exexute:" + cmd);
               
                for (int i = 0; i < cmd.Length; i++)
                {
                    PostMessage(windowHandle, WM_CHAR, (IntPtr)cmd[i], new IntPtr(0));
                }

                IntPtr val = new IntPtr(0x0D); //Enter code
                PostMessage(windowHandle, WM_KEYDOWN, val, new IntPtr(0));
                PostMessage(windowHandle, WM_KEYUP, val, new IntPtr(0));
                Environment.Exit(0);
            }
            

        }
        private IntPtr GetSubHandle(IntPtr parentHandle, int index)
        {
            if (index == 0)
            {
                IntPtr childAfter = new IntPtr(index);
                IntPtr hWnd = FindWindowEx(parentHandle, childAfter, "SWT_Window0", string.Empty);

                StringBuilder title = new StringBuilder(255);
                GetWindowText(hWnd, title, 255);

                String clasName = GetClassNameOfWindow(hWnd);

                int winlong1 = GetWindowLong(hWnd, GWL_STYLE);
                int winlong2 = GetWindowLong(hWnd, GWL_EXSTYLE);
                int winlong3 = GetWindowLong(hWnd, GWL_USERDATA);
                int winlong4 = GetWindowLong(hWnd, GWL_ID);
                String aa = "H:" + hWnd.ToString("X4") + " T:" + title + " C:" + clasName + " 1:" + winlong1 + " 2:" + winlong2 + " 3:" + winlong3 + " 4:" + winlong4;
                Console.WriteLine(aa);

                int length = GetWindowTextLength(hWnd);

                return hWnd;
            }
            
            List<IntPtr> list = GetAllChildrenWindowHandles(parentHandle, 10);
            if (list.Count > index)
            {
                String title = "";
                String clasName = "";
                IntPtr hWnd = list.ElementAt(index);
                int winlong1 = GetWindowLong(hWnd, GWL_STYLE);
                int winlong2 = GetWindowLong(hWnd, GWL_EXSTYLE);
                int winlong3 = GetWindowLong(hWnd, GWL_USERDATA);
                int winlong4 = GetWindowLong(hWnd, GWL_ID);
                String aa = "H:" + hWnd.ToString("X4") + " T:" + title + " C:" + clasName + " 1:" + winlong1 + " 2:" + winlong2 + " 3:" + winlong3 + " 4:" + winlong4;
                Console.WriteLine(aa);
                return hWnd;
            }
            return IntPtr.Zero;
        }
  

        private List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;

                int winlong1 = GetWindowLong(currChild, GWL_STYLE);
                //Console.WriteLine("(" + currChild.ToString("X4") + ") "+ winlong1);

            }
            return result;
        }

        private IntPtr GetCCS_TerminalHandle()
        {
            Process[] processes = Process.GetProcessesByName("ccstudio");

            foreach (Process p in processes)
            {
                IntPtr windowHandle = p.MainWindowHandle;

                StringBuilder title = new StringBuilder(255);
                GetWindowText(windowHandle, title, 255);
                //Console.WriteLine(p.ProcessName + "Handle:" + windowHandle.ToString("X4") + " " + title);

                //g_lastHwnd = IntPtr.Zero;
                g_lastHwnd = EnumFind(windowHandle, 0, 1445003264);
            }
            Console.WriteLine("hwnd:" + g_lastHwnd.ToString("X4"));
            return g_lastHwnd;
        }

        string GetClassNameOfWindow(IntPtr hwnd)
        {
            return "";
            string className = "";
            StringBuilder classText = null;
            try
            {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!String.IsNullOrEmpty(classText.ToString()) && !String.IsNullOrWhiteSpace(classText.ToString()))
                    className = classText.ToString();
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            finally
            {
                classText = null;
            }
            return className;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            IntPtr hWnd = GetCCS_TerminalHandle();
            String cmd = "test";
            for (int i = 0; i < cmd.Length; i++)
            {
                PostMessage(hWnd, WM_CHAR, (IntPtr)cmd[i], new IntPtr(0));
            }



            int total_accout = FindWindow("SWT_Window0", "");

            if (total_accout > 0)
            {
                //SendMessage(total_accout, WM_SYSCOMMAND, SC_CLOSE, 0);
                Console.WriteLine("total_accout=" + total_accout);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("ccstudio");

            foreach (Process p in processes)
            {
                IntPtr windowHandle = p.MainWindowHandle;

                StringBuilder title = new StringBuilder(255);
                GetWindowText(windowHandle, title, 255);
                Console.WriteLine(p.ProcessName + "Handle:" + windowHandle.ToString("X4") + " " + title);


                Enum(windowHandle, 0);
            }
        }

        private void Enum(IntPtr parentHandle, int level)
        {

            String space = "";
            for (int i = 0; i < level; i++)
            {
                space += "  ";
            }

            foreach (IntPtr hwnd in GetAllChildrenWindowHandles(parentHandle, 10))
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                int id = GetWindowLong(hwnd, GWL_ID);


                uint ll = GetClassLong(hwnd, GCL_WNDPROC);


                Console.WriteLine(space+" hwnd:"+ hwnd.ToString("X4") + " style:" + style + " id:" + id + " ret:" + ll);
                Enum(hwnd, level+1);
            }
        }

        private IntPtr EnumFind(IntPtr parentHandle, int level, int target)
        {
            String space = "";
            for (int i = 0; i < level; i++)
            {
                space += "  ";
            }
            List<IntPtr> list = GetAllChildrenWindowHandles(parentHandle, 10);
            foreach (IntPtr hwnd in list)
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                int id = GetWindowLong(hwnd, GWL_ID);

                //Console.WriteLine(space + " hwnd:" + hwnd.ToString("X4") + " style:" + style + " id:" + id);
                if (target == style)
                {
                    Console.WriteLine(space + " hwnd:" + hwnd.ToString("X4") + " style:" + style + " id:" + id);
                    if (GetAllChildrenWindowHandles(hwnd, 10).Count == 0 && list.Count == 1)
                    {
                        g_lastHwnd = hwnd;
                        return hwnd;
                    }
                }

                IntPtr findHwnd = EnumFind(hwnd, level + 1, target);
                if (findHwnd != IntPtr.Zero) return findHwnd;
                
            }
            return IntPtr.Zero;
        }

    }
}
