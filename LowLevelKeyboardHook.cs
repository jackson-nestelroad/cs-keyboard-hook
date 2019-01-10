using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Keyboard.Hook
{
    class LowLevelKeyboardHook
    {
        // Useful constants
        private const int WH_KEYBOARD_LL = 13;
        private const int HC_NOREMOVE = 3;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;
        private const int WM_USER = 0x400;
        private const int WM_KEYINPUT = WM_USER + 102;

        // Hook ID
        private static IntPtr m_hHook = IntPtr.Zero;
        internal static IntPtr m_hWnd = IntPtr.Zero;

        // Callback function for keyboard hook
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc m_proc = HookCallback;

        // Event handlers for keyboard hook
        // public event EventHandler<Keys> OnKeyPressed;
        // public event EventHandler<Keys> OnKeyUnpressed;

        // Constructor
        public LowLevelKeyboardHook() { }

        // Set the hook
        public bool SetHook()
        {
            return SetLowLevelKeyboardHook(true, 0, GetActiveWindow());
        }

        // Remove the hook
        public bool RemoveHook()
        {
            return SetLowLevelKeyboardHook(false, 0, GetActiveWindow());
        }

        // Function that actually sets the keyboard hook
        private bool SetLowLevelKeyboardHook(bool bInstall, uint dwThreadId, IntPtr hWndCaller)
        {
            bool bOk;
            m_hWnd = hWndCaller;

            // Set keyboard hook
            if (bInstall)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    m_hHook = SetWindowsHookEx(WH_KEYBOARD_LL, m_proc, GetModuleHandle(curModule.ModuleName), 0);
                }
                bOk = (m_hHook != IntPtr.Zero);
            }
            // Remove keyboard hook
            else
            {
                bOk = UnhookWindowsHookEx(m_hHook);
                m_hHook = IntPtr.Zero;
            }

            return bOk;
        }

        // Function that runs on every key press
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || nCode == HC_NOREMOVE)
                return CallNextHookEx(m_hHook, nCode, wParam, lParam);

            // Key down
            if(wParam == (IntPtr)WM_KEYDOWN)
            {
                // Get key code
                int vkCode = Marshal.ReadInt32(lParam);

                // Console.WriteLine((Keys)vkCode);

                // Receive what to do with the key
                switch (KeyHandler.HandleKey(vkCode))
                {
                    // Pass the key through
                    case KeyHandler.PASS_KEY:
                        return CallNextHookEx(m_hHook, nCode, wParam, lParam);

                    // Remove the hook entirely
                    case KeyHandler.KILL_KEY:
                        PostMessage(m_hWnd, WM_KEYINPUT, KeyHandler.KILL_HOOK, KeyHandler.KILL_HOOK);
                        return (IntPtr)1;

                    // Do not let the key through
                    case KeyHandler.EAT_KEY:
                        return (IntPtr)1;
                }
            }

            return CallNextHookEx(m_hHook, nCode, wParam, lParam);
        }

        // DLL Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(int hWnd, IntPtr msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetActiveWindow();
    }
}
