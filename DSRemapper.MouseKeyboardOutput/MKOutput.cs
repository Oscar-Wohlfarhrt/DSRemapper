using System.Runtime.InteropServices;
using static DSRemapper.MouseKeyboardOutput.SendInputApi;

namespace DSRemapper.MouseKeyboardOutput
{
    /// <summary>
    /// Mouse buttons enumeration
    /// </summary>
    public enum MouseButton : int
    {
        /// <summary>Left mouse button</summary>
        LEFT,
        /// <summary>Right mouse button</summary>
        RIGHT,
        /// <summary>Middle mouse button</summary>
        MIDDLE,
        /// <summary>Forward mouse button</summary>
        BUTTON4,
        /// <summary>Back mouse button</summary>
        BUTTON5,
    }
    /// <summary>
    /// Mouse and Keyboard Output/Emulation class
    /// </summary>
    public class MKOutput
    {
        private readonly InputBuilder inputBuilder = new();

        private static void SendInput(INPUT[] inputs) => _ = SendInputApi
            .SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        /// <summary>
        /// Emulates a key press. Doesn't release the key pulsation until KeyUp function for the same code is called.
        /// Doesn't work with DirectX applications.
        /// </summary>
        /// <param name="key">Virtual key code</param>
        public void KeyDown(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyDown(key).ToArray());
        }
        /// <summary>
        /// Emulates a key press. Doesn't release the key pulsation until SCUp function for the same code is called.
        /// Works with DirectX applications.
        /// </summary>
        /// <param name="key">Scan key code</param>
        public void SCDown(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyDown(key).ToArray());
        }
        /// <summary>
        /// Emulates a key release.
        /// Doesn't work with DirectX applications.
        /// </summary>
        /// <param name="key">Virtual key code</param>
        public void KeyUp(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyUp(key).ToArray());
        }
        /// <summary>
        /// Emulates a key release.
        /// Works with DirectX applications.
        /// </summary>
        /// <param name="key">Scan key code</param>
        public void SCUp(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyUp(key).ToArray());
        }
        /// <summary>
        /// Emulates a key press and release (pulsation).
        /// Doesn't work with DirectX applications.
        /// </summary>
        /// <param name="key">Virtual key code</param>
        public void KeyPress(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyPress(key).ToArray());
        }
        /// <summary>
        /// Emulates a key press and release (pulsation).
        /// Works with DirectX applications.
        /// </summary>
        /// <param name="key">Scan key code</param>
        public void SCPress(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyPress(key).ToArray());
        }
        /// <summary>
        /// Converts a scan code to a virtual key code
        /// </summary>
        /// <param name="key">Scan code to convert</param>
        /// <returns>An equivalent virtual key code</returns>
        public static int ConvertSCToKey(ScanCodeShort key) =>
            (int)MapVirtualKeyA((uint)key, MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK);
        /// <summary>
        /// Converts a virtual key code to a scan code
        /// </summary>
        /// <param name="key">Virtual key code to convert</param>
        /// <returns>An equivalent scan code</returns>
        public static int ConvertKeyToSC(VirtualKeyShort key) =>
            (int)MapVirtualKeyA((uint)key, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
        /// <summary>
        /// Moves mouse by the X and Y values
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        public void MoveMouse(int x, int y)
        {
            SendInput(inputBuilder.Clear().MoveMouse(x, y).ToArray());
        }
        /// <summary>
        /// Moves mouse wheel by the X and Y values. One wheel click is equivalente to 120.
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        public void MouseWheel(int x, int y)
        {
            SendInput(inputBuilder.Clear().MouseWheel(x, y).ToArray());
        }
        /// <summary>
        /// Moves the mouse to the X and Y coordinates
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        public void MoveMouseAbsolute(int x, int y)
        {
            SendInput(inputBuilder.Clear().MoveMouse(x, y, true).ToArray());
        }
        /// <summary>
        /// Emulates a mouse button press. Doesn't release the key pulsation until MButtonUp function for the same button is called.
        /// </summary>
        /// <param name="button">Mouse button code</param>
        public void MButtonDown(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonDown(button).ToArray());
        }
        /// <summary>
        /// Emulates a mouse button release.
        /// </summary>
        /// <param name="button">Mouse button code</param>
        public void MButtonUp(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonUp(button).ToArray());
        }
        /// <summary>
        /// Emulates a mouse button press and release (pulsation).
        /// </summary>
        /// <param name="button">Mouse button code</param>
        public void MButtonPress(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonPress(button).ToArray());
        }
    }

    internal class InputBuilder
    {
        readonly List<INPUT> inputList = new();

        public InputBuilder AddKeyDown(VirtualKeyShort key)
        {
            INPUT input = new(InputType.INPUT_KEYBOARD);
            input.U.ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = 0,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };

            inputList.Add(input);

            return this;
        }
        public InputBuilder AddKeyUp(VirtualKeyShort key)
        {
            INPUT input = new(InputType.INPUT_KEYBOARD);
            input.U.ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = KEYEVENTF.KEYUP,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };

            inputList.Add(input);

            return this;
        }
        public InputBuilder AddKeyPress(VirtualKeyShort key)
        {
            AddKeyDown(key);
            AddKeyUp(key);

            return this;
        }
        public InputBuilder AddKeyDown(ScanCodeShort key)
        {
            INPUT input = new(InputType.INPUT_KEYBOARD);
            input.U.ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = key,
                dwFlags = KEYEVENTF.SCANCODE,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };

            inputList.Add(input);

            return this;
        }
        public InputBuilder AddKeyUp(ScanCodeShort key)
        {
            INPUT input = new(InputType.INPUT_KEYBOARD);
            input.U.ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = key,
                dwFlags = KEYEVENTF.KEYUP | KEYEVENTF.SCANCODE,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };

            inputList.Add(input);

            return this;
        }
        public InputBuilder AddKeyPress(ScanCodeShort key)
        {
            AddKeyDown(key);
            AddKeyUp(key);

            return this;
        }
        public InputBuilder Clear()
        {
            inputList.Clear();
            return this;
        }
        public INPUT[] ToArray()
        {
            return inputList.ToArray();
        }
        public InputBuilder MoveMouse(int x, int y, bool absolute = false)
        {
            INPUT input = new(InputType.INPUT_MOUSE);
            input.U.mi = new MOUSEINPUT
            {
                dx = x,
                dy = y,
                mouseData = 0,
                dwFlags = MOUSEEVENTF.MOVE | (absolute ? MOUSEEVENTF.ABSOLUTE : 0),
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };

            inputList.Add(input);

            return this;
        }
        public InputBuilder MouseWheel(int x, int y)
        {
            if (x != 0)
            {
                INPUT inputv = new(InputType.INPUT_MOUSE);
                inputv.U.mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = x,
                    dwFlags = MOUSEEVENTF.WHEEL,
                    time = 0,
                    dwExtraInfo = GetMessageExtraInfo()
                };
                inputList.Add(inputv);
            }
            if (y != 0)
            {
                INPUT inputh = new(InputType.INPUT_MOUSE);
                inputh.U.mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = y,
                    dwFlags = MOUSEEVENTF.HWHEEL,
                    time = 0,
                    dwExtraInfo = GetMessageExtraInfo()
                };
                inputList.Add(inputh);
            }

            return this;
        }
        public InputBuilder MButtonDown(MouseButton button)
        {
            MOUSEEVENTF flags = 0;
            int data = 0;

            switch (button)
            {
                case MouseButton.LEFT:
                    flags = MOUSEEVENTF.LEFTDOWN;
                    break;
                case MouseButton.RIGHT:
                    flags = MOUSEEVENTF.RIGHTDOWN;
                    break;
                case MouseButton.MIDDLE:
                    flags = MOUSEEVENTF.MIDDLEDOWN;
                    break;
                case MouseButton.BUTTON4:
                    data = 1;
                    flags = MOUSEEVENTF.XDOWN;
                    break;
                case MouseButton.BUTTON5:
                    flags = MOUSEEVENTF.XDOWN;
                    data = 2;
                    break;
            }

            INPUT input = new(InputType.INPUT_MOUSE);
            input.U.mi = new MOUSEINPUT
            {
                dx = 0,
                dy = 0,
                mouseData = data,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };
            inputList.Add(input);
            return this;
        }
        public InputBuilder MButtonUp(MouseButton button)
        {
            MOUSEEVENTF flags = 0;
            int data = 0;

            switch (button)
            {
                case MouseButton.LEFT:
                    flags = MOUSEEVENTF.LEFTUP;
                    break;
                case MouseButton.RIGHT:
                    flags = MOUSEEVENTF.RIGHTUP;
                    break;
                case MouseButton.MIDDLE:
                    flags = MOUSEEVENTF.MIDDLEUP;
                    break;
                case MouseButton.BUTTON4:
                    data = 1;
                    flags = MOUSEEVENTF.XUP;
                    break;
                case MouseButton.BUTTON5:
                    flags = MOUSEEVENTF.XUP;
                    data = 2;
                    break;
            }

            INPUT input = new(InputType.INPUT_MOUSE);
            input.U.mi = new MOUSEINPUT
            {
                dx = 0,
                dy = 0,
                mouseData = data,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = GetMessageExtraInfo()
            };
            inputList.Add(input);
            return this;
        }
        public InputBuilder MButtonPress(MouseButton button)
        {
            MButtonDown(button);
            MButtonUp(button);
            return this;
        }
    }
}
