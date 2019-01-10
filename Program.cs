using System;

namespace Keyboard.Hook
{
    class Program
    {
        // Change this to DllMain when moved over
        static void Main(string[] args)
        {
            // Here it is!
            var Hook = new LowLevelKeyboardHook();
            try
            {
                if (!Hook.SetHook())
                    throw new Exception("Failed to set keyboard hook.");

                // Console.WriteLine("Hook attched.");

                // Start up the key handler
                KeyHandler.Handle();

                if (!Hook.RemoveHook())
                    throw new Exception("Failed to remove keyboard hook.");

                // Console.WriteLine("Hook removed.");
            }
            catch(Exception error)
            {
                Console.WriteLine(error);
            }
        }
    }
}
