using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Keyboard.Hook
{
    class KeyHandler
    {
        // Key handling actions
        public const int EAT_KEY = 0;
        public const int PASS_KEY = 1;
        public const int KILL_KEY = 2;
        public const int NA_KEY = 3;
        public const int KILL_HOOK = -1;

        // Internal flags
        private static bool CtrlKey = false;
        private static bool InterceptKeys = true;

        // Random number generator
        private static readonly Random RandomNumber = new Random();

        // Constructor
        public KeyHandler(IntPtr hookWindow) { }

        // Function to start message handling
        public static void Handle()
        {
            // We can send different messages directly from LowLevelKeyboardHook with PostMessage()
            // This is how we know when the hook should be destroyed
            while (GetMessage(out Message msg, LowLevelKeyboardHook.m_hWnd, 0, 0) != 0)
            {
                // Console.WriteLine(msg.WParam.ToInt32());
                if (msg.WParam.ToInt32() == KILL_HOOK)
                    break;

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        // Function to handle a specific key press
        internal static int HandleKey(int key)
        {
            // This key is part of a command
            if (CtrlKey)
                return CtrlCommand(key);

            // Find any special action with this key
            int action = KeyAction(key);

            // We are currently intercepting keys
            if (InterceptKeys)
            {
                // Special action defined
                if (action != NA_KEY)
                    return action;

                // Return how we should intercept the key based on this function
                return InterceptKey(key);
            }
            
            // Let the key through as normal
            return PASS_KEY;
        }

        // Function that does something with this key if it is special
        private static int KeyAction(int key)
        {
            switch (key)
            {
                // Ctrl (Starts a command)
                case 162: CtrlKey = true; return EAT_KEY;
                // Any other key
                default: return NA_KEY;
            }
        }

        // Function to determine how or if we will intercept the key
        private static int InterceptKey(int key)
        {
            if(RandomNumber.Next(2) == 1)
                return EAT_KEY;

            return PASS_KEY;
        }

        // Function to handle keys that follow the Ctrl key
        private static int CtrlCommand(int key)
        {
            CtrlKey = false;
            switch (key)
            {
                // ~ (Destroys hook)
                case 192: return KILL_KEY;
                // - (Stops intercepting keys)
                case 189: InterceptKeys = false; return EAT_KEY;
                // + (Starts intercepting keys)
                case 187: InterceptKeys = true; return EAT_KEY;
                // Any other key
                default: return PASS_KEY;
            }
        }

        // DLL Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool TranslateMessage(ref Message lpMsg);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr DispatchMessage(ref Message lpMsg);
    }
}
