using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace IPCalculatorWPF
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			IpAddressControl.SetZeroValues();
			PrefixBox.Value = 8;
			UpdateMaskFromPrefix(8);
		}

		private void IpAddress_OctetChanged(object sender, TextChangedEventArgs e)
		{
			if (IPAddress.TryParse(IpAddressControl.Address, out var ip))
			{
				byte first = ip.GetAddressBytes()[0];
				if (first < 128) PrefixBox.Value = 8;
				else if (first < 192) PrefixBox.Value = 16;
				else if (first < 224) PrefixBox.Value = 24;
			}
			UpdateInfo();
		}

		private void SubnetMask_OctetChanged(object sender, TextChangedEventArgs e)
		{
			if (IPAddress.TryParse(SubnetMaskControl.Address, out var mask))
			{
				var bytes = mask.GetAddressBytes();
				uint m = BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
				int prefix = 0;
				while ((m & 0x80000000) != 0)
				{
					prefix++;
					m <<= 1;
				}
				PrefixBox.Value = prefix;
			}
			UpdateInfo();
		}

		private void PrefixBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (PrefixBox.Value.HasValue)
			{
				UpdateMaskFromPrefix(PrefixBox.Value.Value);
			}
			UpdateInfo();
		}

		private void UpdateMaskFromPrefix(int prefix)
		{
			uint mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
			var bytes = BitConverter.GetBytes(mask).Reverse().ToArray();
			SubnetMaskControl.Address = string.Join(".", bytes);
		}

		private void UpdateInfo()
		{
			try
			{
				string ipAddress = IpAddressControl.Address;
				string subnetMask = SubnetMaskControl.Address;

				IPAddress ip;
				if (!IPAddress.TryParse(ipAddress, out ip))
					throw new Exception("Некорректный IP-адрес.");
				IPAddress mask;
				if (!IPAddress.TryParse(subnetMask, out mask))
					throw new Exception("Некорректная маска.");

				byte[] ipBytes = ip.GetAddressBytes();
				byte[] maskBytes = mask.GetAddressBytes();
				byte[] networkBytes = new byte[4];

				for (int i = 0; i < 4; i++)
					networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);

				byte[] broadcastBytes = new byte[4];
				for (int i = 0; i < 4; i++)
					broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);

				uint totalIps = (uint)Math.Pow(2, 32 - (PrefixBox.Value ?? 24));
				uint usableIps = totalIps > 2 ? totalIps - 2 : 0;

				InfoTextBlock.Text =
					$"Адрес сети:\t\t\t{new IPAddress(networkBytes)}\n" +
					$"Широковещательный адрес:\t{new IPAddress(broadcastBytes)}\n" +
					$"Количество IP-адресов:\t\t{totalIps}\n" +
					$"Количество узлов:\t\t{usableIps}";
			}
			catch (Exception ex)
			{
				InfoTextBlock.Text = $"Ошибка: {ex.Message}";
			}
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}