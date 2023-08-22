using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using GazeUtilityLibrary.DataStructs;

namespace GazeUtilityLibrary.Tracker
{
    /// <summary>
    /// This class is used to hook into the system mouse events and track the position
    /// </summary>
    /// <seealso cref="GazeHelper.TrackerHandler" />
    public class MouseTracker : BaseTracker
    {
        /// <summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644990(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="idHook">The type of hook procedure to be installed.</param>
        /// <param name="lpfn">A pointer to the hook procedure.</param>
        /// <param name="hMod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
        /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure, NULL otherwise.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        /// <summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644993(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="hhk">A handle to the hook to be removed.</param>
        /// <returns>If the function succeeds, the return value is nonzero, zero otherwise.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        /// <summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644974(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="hhk">This parameter is ignored.</param>
        /// <param name="nCode">The hook code passed to the current hook procedure.</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure.</param>
        /// <param name="lParam">The lParam value passed to the current hook procedure.</param>
        /// <returns>This value is returned by the next hook procedure in the chain</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        /// <summary>i
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms683199(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="lpModuleName">Name of the loaded module.</param>
        /// <returns>If the function succeeds, the return value is a handle to the specified module, NULL otherwise.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644986(v=vs.85).aspx"/>
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">The identifier of the mouse message.</param>
        /// <param name="lParam">A pointer to an <see cref="MSLLHOOKSTRUCT"/> structure.</param>
        /// <returns>The value returend by <see cref="CallNextHookEx"/></returns>
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// The hook id for low level mouse input events
        /// </summary>
        private const int WH_MOUSE_LL = 14;
        /// <summary>
        /// The hook identifier
        /// </summary>
        private IntPtr _hookID = IntPtr.Zero;
        /// <summary>
        /// The reference to the delegate function hooked to the mouse events.
        /// It is assigned to a class attribute to keep it alive as long as the class is used.
        /// </summary>
        private LowLevelMouseProc _proc;
        /// <summary>
        /// An enumeration of mouse events
        /// </summary>
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        /// <summary>
        /// The point structure that is used in the <see cref="MSLLHOOKSTRUCT"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
        /// <summary>
        /// This structure is passed to the mouse hook callback
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseTracker"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config item.</param>
        public MouseTracker(TrackerLogger logger, ConfigItem config) : base(logger, config, "Mouse Tracker")
        {
            State = DeviceStatus.Initializing;
            _proc = HookCallback;
            Start();
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                base.Dispose(true);
            }
        }

        protected override int GetFixationFrameCount()
        {
            throw new NotImplementedException();
        }

        protected override Vector3 GetUnitDirection()
        {
            throw new NotImplementedException();
        }

        public override Task<List<GazeCalibrationData>> ApplyCalibration()
        {
            throw new NotImplementedException();
        }

        protected override void InitDriftCompensation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hooks the callback function <see cref="HookCallback(int, IntPtr, IntPtr)"/> to mouse events.
        /// </summary>
        public void Start()
        {
            _hookID = SetHook(_proc);
            State = DeviceStatus.Tracking;
        }
        /// <summary>
        /// Removes to mouse event hook.
        /// </summary>
        public void Stop()
        {
            UnhookWindowsHookEx(_hookID);
            State = DeviceStatus.DeviceNotConnected;
        }

        /// <summary>
        /// Sets the mouse event hook.
        /// </summary>
        /// <param name="proc">The proc.</param>
        /// <returns>the hook id</returns>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule? curModule = curProcess.MainModule)
                {
                    IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
                    if (hook == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
                    return hook;
                }
            }
        }

        /// <summary>
        /// On each mouse move event, the coordinates of the mouse pointer and the timestamp is passed to the gaze event handler.
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">The identifier of the mouse message.</param>
        /// <param name="lParam">A pointer to an <see cref="MSLLHOOKSTRUCT"/> structure.</param>
        /// <returns>The value returend by <see cref="CallNextHookEx"/></returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (MouseMessages)wParam == MouseMessages.WM_MOUSEMOVE)
            {
                MSLLHOOKSTRUCT? hookStruct = (MSLLHOOKSTRUCT?)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT?));
                if (hookStruct != null)
                {
                    GazeData gaze_data = new GazeData(TimeSpan.FromMilliseconds(hookStruct?.time ?? 0), new Vector2(hookStruct?.pt.x ?? 0, hookStruct?.pt.y ?? 0), true);
                    OnGazeDataReceived(gaze_data);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public override Task InitCalibrationAsync()
        {
            throw new NotImplementedException();
        }

        public override void InitValidation()
        {
            throw new NotImplementedException();
        }

        public override Task FinishCalibrationAsync()
        {
            throw new NotImplementedException();
        }

        public override void FinishValidation()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CollectCalibrationDataAsync(Point point)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> CollectValidationDataAsync(Point point)
        {
            throw new NotImplementedException();
        }

        public override void InitCalibration()
        {
            throw new NotImplementedException();
        }

        public override void FinishCalibration()
        {
            throw new NotImplementedException();
        }

        public override GazeValidationData? ComputeValidation()
        {
            throw new NotImplementedException();
        }
    }
}
