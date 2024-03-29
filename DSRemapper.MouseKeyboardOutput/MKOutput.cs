﻿using System.Runtime.InteropServices;
using static DSRemapper.MouseKeyboardOutput.SendInputApi;

namespace DSRemapper.MouseKeyboardOutput
{
    public enum MouseButton : int
    {
        LEFT,
        RIGHT,
        MIDDLE,
        BUTTON4,
        BUTTON5,
    }
    public class MKOutput
    {
        private InputBuilder inputBuilder = new InputBuilder();

        private void SendInput(INPUT[] inputs)
        {
            SendInputApi.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        }

        public void KeyDown(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyDown(key).ToArray());
        }
        public void SCDown(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyDown(key).ToArray());
        }
        public void KeyUp(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyUp(key).ToArray());
        }
        public void SCUp(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyUp(key).ToArray());
        }
        public void KeyPress(VirtualKeyShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyPress(key).ToArray());
        }
        public void SCPress(ScanCodeShort key)
        {
            SendInput(inputBuilder.Clear().AddKeyPress(key).ToArray());
        }

        public int ConvertSCToKey(ScanCodeShort key)
        {
            return (int)MapVirtualKeyA((uint)key, MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK);
        }
        public int ConvertKeyToSC(VirtualKeyShort key)
        {
            return (int)MapVirtualKeyA((uint)key, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC);
        }

        public void MoveMouse(int x, int y)
        {
            SendInput(inputBuilder.Clear().MoveMouse(x, y).ToArray());
        }
        public void MoveMouseAbsolute(int x, int y)
        {
            SendInput(inputBuilder.Clear().MoveMouse(x, y, true).ToArray());
        }
        public void MButtonDown(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonDown(button).ToArray());
        }
        public void MButtonUp(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonUp(button).ToArray());
        }
        public void MButtonPress(MouseButton button)
        {
            SendInput(inputBuilder.Clear().MButtonPress(button).ToArray());
        }
    }

    internal class InputBuilder
    {
        List<INPUT> inputList = new List<INPUT>();

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
