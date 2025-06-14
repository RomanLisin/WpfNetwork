using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace IPcalc
{
	public class WinApiIpAddressHost : HwndHost
	{
		private const string WC_IPADDRESS = "SysIPAddress32";
		private IntPtr hwndControl;

		protected override HandleRef BuildWindowCore(HandleRef hwndParent)
		{
			InitCommonControls();

			hwndControl = CreateWindowEx(
				0,
				WC_IPADDRESS,
				"",
				WS_CHILD | WS_VISIBLE,
				0, 0, 140, 25,
				hwndParent.Handle,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero);

			return new HandleRef(this, hwndControl);
		}

		protected override void DestroyWindowCore(HandleRef hwnd)
		{
			DestroyWindow(hwnd.Handle);
		}

		public string GetIPAddress()
		{
			SendMessage(hwndControl, IPM_GETADDRESS, IntPtr.Zero, out uint address);
			byte[] ip = new byte[]
			{
				(byte)((address >> 24) & 0xFF),
				(byte)((address >> 16) & 0xFF),
				(byte)((address >> 8) & 0xFF),
				(byte)(address & 0xFF)
			};
			return string.Join(".", ip);
		}

		public void SetIPAddress(string ip)
		{
			var parts = ip.Split('.');

			if (parts.Length == 4 &&
				byte.TryParse(parts[0], out byte a) &&
				byte.TryParse(parts[1], out byte b) &&
				byte.TryParse(parts[2], out byte c) &&
				byte.TryParse(parts[3], out byte d))
			{
				uint addr = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | d;
				SendMessage(hwndControl, IPM_SETADDRESS, IntPtr.Zero, (IntPtr)addr);
			}
		}

		#region WinAPI imports

		private const int WS_CHILD = 0x40000000;
		private const int WS_VISIBLE = 0x10000000;

		private const int IPM_GETADDRESS = 0x0400 + 102;
		private const int IPM_SETADDRESS = 0x0400 + 101;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr CreateWindowEx(
			int dwExStyle,
			string lpClassName,
			string lpWindowName,
			int dwStyle,
			int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent,
			IntPtr hMenu,
			IntPtr hInstance,
			IntPtr lpParam);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("comctl32.dll", SetLastError = true)]
		static extern void InitCommonControls();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, out uint lParam);

		#endregion
	}
}
