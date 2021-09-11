using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PassWinmenu.Utilities
{
	internal class NativeMethods
	{
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint SendInput(uint nInputs,
			[MarshalAs(UnmanagedType.LPArray), In] Input[] pInputs,
		 int cbSize);

		public static Process GetWindowProcess(IntPtr hWnd)
		{
			GetWindowThreadProcessId(hWnd, out uint pid);
			return Process.GetProcessById((int)pid);
		}

		/// <summary>
		/// Creates an overlapped, pop-up, or child window with an extended
		/// window style (otherwise identical to CreateWindow).
		/// </summary>
		/// <param name="dwExtStyle">
		/// The extended window styles to apply to the window.
		/// </param>
		/// <param name="lpClassName">
		/// A pointer to the name of the class to use for the window, or an
		/// atom returned by <see cref="RegisterClass(ref WindowClass)"/>.
		/// </param>
		/// <param name="lpWindowName">
		/// The name of the window to create.
		/// </param>
		/// <param name="dwStyle">
		/// The style values for the window to create.
		/// </param>
		/// <param name="x">
		/// The horizontal position of the window.
		/// </param>
		/// <param name="y">
		/// The vertical position of the window.
		/// </param>
		/// <param name="nWidth">
		/// The width of the window in device units.
		/// </param>
		/// <param name="nHeight">
		/// The height of the window in device units.
		/// </param>
		/// <param name="hWndParent">
		/// The handle to the window that is to be the parent of the
		/// created window.
		/// </param>
		/// <param name="hMenu">
		/// The handle to a menu or child window.
		/// </param>
		/// <param name="hInstance">
		/// The handle to an instance of the module to be associated
		/// with the window.
		/// </param>
		/// <param name="lpParam">
		/// A pointer to a value to be passed as the <c>lParam</c> of the
		/// <c>WM_CREATE</c> message sent to the window on creation.
		/// </param>
		/// <returns></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr CreateWindowEx(
			uint dwExtStyle,
			UIntPtr lpClassName,
			IntPtr lpWindowName,
			uint dwStyle,
			int x,
			int y,
			int nWidth,
			int nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam
		);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Input
	{
		internal InputType Type;
		internal KeyboardInput Data;

		internal static Input FromKeyCode(VirtualKeyCode keyCode, KeyDirection direction)
		{
			KeyEventFlags flags = 0;
			if (keyCode.IsExtendedKey())
			{
				flags |= KeyEventFlags.ExtendedKey;
			}
			if (direction == KeyDirection.Up)
			{
				flags |= KeyEventFlags.KeyUp;
			}

			return new Input
			{
				Type = InputType.Keyboard,
				Data = new KeyboardInput
				{
					KeyCode = keyCode,
					ScanCode = 0,
					Flags = flags,
					Time = 0,
					ExtraInfo = IntPtr.Zero
				}
			};
		}

		internal static Input FromCharacter(char character, KeyDirection direction)
		{
			var flags = KeyEventFlags.Unicode;

			// If the scan code is preceded by a prefix byte that has the value 0xE0 (224),
			// we need to include the ExtendedKey flag in the Flags property.
			if ((character & 0xFF00) == 0xE000)
			{
				flags |= KeyEventFlags.ExtendedKey;
			}
			if (direction == KeyDirection.Up)
			{
				flags |= KeyEventFlags.KeyUp;
			}

			return new Input
			{
				Type = InputType.Keyboard,
				Data = new KeyboardInput
				{
					KeyCode = 0,
					ScanCode = character,
					Flags = flags,
					Time = 0,
					ExtraInfo = IntPtr.Zero
				}
			};
		}
	}

	internal enum KeyDirection
	{
		Up,
		Down,
	}

	/// <summary>
	/// A virtual key code, as defined on https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
	/// </summary>
	internal enum VirtualKeyCode : ushort
	{
		// Note: When adding new keys here, update the implementation of VirtualKeyCodeExtensions.IsExtendedKey
		// if the added key is an extended key. For more information, see:
		// https://docs.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input
		Tab = 0x09,
	}

	internal static class VirtualKeyCodeExtensions
	{
		public static bool IsExtendedKey(this VirtualKeyCode _)
		{
			return false;
		}
	}

	internal enum InputType : uint
	{
		Mouse = 0,
		Keyboard = 1,
		Hardware = 2
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct KeyboardInput
	{
		internal VirtualKeyCode KeyCode;
		internal ushort ScanCode;
		internal KeyEventFlags Flags;
		internal uint Time;
		internal IntPtr ExtraInfo;
		private uint padding;
		private uint padding_;
	}

	[Flags]
	internal enum KeyEventFlags : uint
	{
		ExtendedKey = 0x0001,
		KeyUp = 0x0002,
		ScanCode = 0x0008,
		Unicode = 0x0004
	}
}
