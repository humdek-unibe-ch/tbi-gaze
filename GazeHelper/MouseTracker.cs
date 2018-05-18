using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    public class MouseTracker : TrackerHandler
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The hook id for low level mouse input events
        /// </summary>
        private const int WH_MOUSE_LL = 14;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelMouseProc _proc = null;
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public event EventHandler MouseAction = delegate { };

        public MouseTracker(TrackerLogger logger, int ready_timer) : base(logger, ready_timer, "Mouse Tracker")
        {
            State = EyeTrackingDeviceStatus.Initializing;
            _proc = HookCallback;
            Start();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                base.Dispose(true);
            }
        }

        public void Start()
        {
            _hookID = SetHook(_proc);
            State = EyeTrackingDeviceStatus.Tracking;
        }
        public void Stop()
        {
            UnhookWindowsHookEx(_hookID);
            State = EyeTrackingDeviceStatus.DeviceNotConnected;
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
                    if (hook == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
                    return hook;
                }
            }
        }

        private IntPtr HookCallback( int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && ((MouseMessages)wParam == MouseMessages.WM_MOUSEMOVE))
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                GazeDataArgs gaze_data = new GazeDataArgs(TimeSpan.FromMilliseconds(hookStruct.time), hookStruct.pt.x, hookStruct.pt.y);
                OnGazeDataReceived(gaze_data);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
