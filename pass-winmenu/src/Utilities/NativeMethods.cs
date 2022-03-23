using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PassWinmenu.Hotkeys;

namespace PassWinmenu.Utilities
{
	internal partial class NativeMethods
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
			IntPtr lpParam);

		/// <summary>
		/// Registers a system-wide hotkey.
		/// </summary>
		/// <param name="hWnd">
		/// The handle to the window that is to receive notification of
		/// the hotkey being triggered.
		/// </param>
		/// <param name="id">
		/// A handle-unique identifier for the hotkey.
		/// </param>
		/// <param name="fsModifiers">
		/// The modifier keys to be pressed with the hotkey, and other
		/// behavioural flags.
		/// </param>
		/// <param name="vk">
		/// The virtual-key code of the hotkey to be pressed with the
		/// modifier keys.
		/// </param>
		/// <returns>
		/// True if the hotkey was registered, false if otherwise.
		/// </returns>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RegisterHotKey(
			IntPtr hWnd,
			int id,
			uint fsModifiers,
			uint vk);

		/// <summary>
		/// Unregisters a hotkey registered through the
		/// <see cref="RegisterHotKey(IntPtr, int, uint, uint)"/> function.
		/// </summary>
		/// <param name="hWnd">
		/// The handle to the window that receives notifications of the
		/// triggering of the hotkey to unregister.
		/// </param>
		/// <param name="id">
		/// The handle-unique identifier of the hotkey to unregister.
		/// </param>
		/// <returns>
		/// True if the hotkey was unregistered, false if otherwise.
		/// </returns>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		/// <summary>
		/// A procedure for handling messages received by the window.
		/// </summary>
		/// <param name="handle">
		/// A handle to the window receiving the message.
		/// </param>
		/// <param name="message">
		/// The type of message received.
		/// </param>
		/// <param name="wParam">
		/// Additional information dependent on the value of
		/// <paramref name="message"/>.
		/// </param>
		/// <param name="lParam">
		/// Additional information dependent on the value of
		/// <paramref name="message"/>.
		/// </param>
		/// <returns>
		/// A value which depends on the value of <paramref name="message"/>
		/// and which indicates the result of processing the message. Return
		/// null to defer to the next available window procedure (which is
		/// the default procedure if no other procedure is registered).
		/// </returns>
		public delegate IntPtr WindowProcedure(
			IntPtr handle,
			WindowMessage message,
			UIntPtr wParam,
			IntPtr lParam);

		/// <summary>
		/// Destroys a window.
		/// </summary>
		/// <param name="hWnd">
		/// The handle to the window to destroy.
		/// </param>
		/// <returns>
		/// True if the window was successfully destroyed, false if
		/// otherwise.
		/// </returns>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		/// <summary>
		/// Registers a window class for use in creating a window.
		/// </summary>
		/// <param name="wndClass">
		/// A description of the class to be created.
		/// </param>
		/// <returns>
		/// Zero if the operation does not succeed, or an atom uniquely
		/// identifying the registered class otherwise.
		/// </returns>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern ushort RegisterClass(ref WindowClass wndClass);

		/// <summary>
		/// Unregisters a class registered using
		/// <see cref="RegisterClass(ref WindowClass)"/>.
		/// </summary>
		/// <param name="lpClassName">
		/// A pointer to the name of the class, or an atom returned
		/// by <see cref="RegisterClass(ref WindowClass)"/>.
		/// </param>
		/// <param name="hInstance">
		/// A handle to the instance of the module that created the
		/// class.
		/// </param>
		/// <returns>
		/// True if the class was unregistered, false if otherwise.
		/// </returns>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnregisterClass(IntPtr lpClassName, IntPtr hInstance);
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
		private readonly uint padding;
		private readonly uint padding_;
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
