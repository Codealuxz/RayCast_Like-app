using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace RayCast
{
    public class HotKey : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly int _id;
        private readonly IntPtr _handle;
        private bool _disposed;

        public event EventHandler? Pressed;

        public HotKey(IntPtr handle, ModifierKeys modifier, Key key)
        {
            _handle = handle;
            _id = GetHashCode();
            RegisterHotKey(_handle, _id, (uint)modifier, (uint)KeyInterop.VirtualKeyFromKey(key));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterHotKey(_handle, _id);
                _disposed = true;
            }
        }

        public void ProcessHotKey()
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
} 