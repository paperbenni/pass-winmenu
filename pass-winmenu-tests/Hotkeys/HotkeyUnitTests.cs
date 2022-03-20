using System;
using System.Windows.Input;
using PassWinmenu.Hotkeys;
using PassWinmenuTests.Utilities;
using Xunit;

namespace PassWinmenuTests.Hotkeys
{
	/// <summary>
	/// Unit tests for the <see cref="Hotkey"/> class.
	/// </summary>
		public class HotkeyUnitTests
	{
		private const ModifierKeys Modifiers = ModifierKeys.Control;
		private const Key          KeyCode   = Key.A;
		private const bool         Repeats   = false;

		private Hotkey                    _hotkey;
		private DummyHotkeyRegistrar      _registrar;

		public HotkeyUnitTests()
		{
			// Save initial value so we can reset to it between tests
			_registrar = new DummyHotkeyRegistrar();

			_hotkey = Hotkey.With(_registrar)
			                .Register(Modifiers, KeyCode, Repeats);
		}

		[Fact]
		public void DefaultRegistrar_FailsOnNull()
		{
			Assert.Throws<ArgumentNullException>(
				() => Hotkey.DefaultRegistrar = null
				);
		}

		[Fact]
		public void Register_ReturnValue_ModifiersKeyRepeats()
		{
			var b = Hotkey.Register(Modifiers, KeyCode, Repeats);

			Assert.Equal(Modifiers, b.ModifierKeys);
			Assert.Equal(KeyCode,   b.Key);
			Assert.Equal(Repeats,   b.Repeats);
		}
		[Fact]
		public void Register_ReturnValue_KeyRepeats()
		{
			var b = Hotkey.Register(KeyCode, Repeats);

			Assert.Equal(ModifierKeys.None, b.ModifierKeys);
			Assert.Equal(KeyCode, b.Key);
			Assert.Equal(Repeats, b.Repeats);
		}

		[Fact]
		public void Register_DefaultRegistration()
		{
			Hotkey.DefaultRegistrar = _registrar;

			var hkCount = _registrar.Hotkeys.Count;

			Hotkey.Register(KeyCode);

			Assert.Equal(hkCount + 1, _registrar.Hotkeys.Count);
		}
		[Fact]
		public void Register_ExplicitRegistration()
		{
			Hotkey.DefaultRegistrar = HotkeyRegistrars.Windows;

			var hkCount = _registrar.Hotkeys.Count;

			Hotkey hk = Hotkey.With(_registrar).Register(KeyCode);

			Assert.Equal(hkCount + 1, _registrar.Hotkeys.Count);
		}

		[Fact]
		public void Triggered_WhenEnabled()
		{
			_hotkey.Enabled = true;

			Assert.True(_hotkey.Enabled);

			bool fired = false;
			_hotkey.Triggered += (s, e) => fired = true;

			_registrar.Hotkeys[(Modifiers, KeyCode)](_registrar, null);

			Assert.True(fired);
		}
		[Fact]
		public void Triggered_WhenNotEnabled()
		{
			_hotkey.Enabled = false;

			Assert.False(_hotkey.Enabled);

			bool fired = false;
			_hotkey.Triggered += (s, e) => fired = true;

			_registrar.Hotkeys[(Modifiers, KeyCode)](_registrar, null);

			Assert.False(fired);
		}

		[Fact]
		public void Dispose_UnregistersWithRegistrar()
		{
			bool callsDispose = false;

			_registrar.Disposal += (s, combo) =>
			{
				Assert.Equal(Modifiers, combo.Item1);
				Assert.Equal(KeyCode, combo.Item2);

				callsDispose = true;
			};

			_hotkey.Dispose();

			Assert.True(callsDispose);
		}
		[Fact]
		public void Dispose_CanMakeMultipleCalls()
		{
			_hotkey.Dispose();
			_hotkey.Dispose();
			_hotkey.Dispose();
		}

		[Fact]
		public void _Properties_Initialised()
		{
			Assert.True(_hotkey.Enabled);
			Assert.Equal(Modifiers, _hotkey.ModifierKeys);
			Assert.Equal(KeyCode, _hotkey.Key);
			Assert.Equal(Repeats, _hotkey.Repeats);
		}

	}
}
