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
			PrefixBox.Value = 24;
			UpdateMaskFromPrefix(24);
		}

		private string GetIpAddress()
		{
			return $"{FirstOctet.Text}.{SecondOctet.Text}.{ThirdOctet.Text}.{FourthOctet.Text}";
		}

		private string GetSubnetMask()
		{
			return $"{MaskFirstOctet.Text}.{MaskSecondOctet.Text}.{MaskThirdOctet.Text}.{MaskFourthOctet.Text}";
		}

		private void Octet_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			if (textBox.Text.Length == textBox.MaxLength && textBox.Text.All(char.IsDigit))
			{
				MoveToNextOctet(textBox);
			}

			IPAddress ip;
			if (IPAddress.TryParse(GetIpAddress(), out ip))
			{
				byte first = ip.GetAddressBytes()[0];
				if (first < 128) PrefixBox.Value = 8;
				else if (first < 192) PrefixBox.Value = 16;
				else if (first < 224) PrefixBox.Value = 24;
			}

			UpdateInfo();
		}

		private void MaskOctet_TextChanged(object sender, TextChangedEventArgs e)
		{
			IPAddress mask;
			if (IPAddress.TryParse(GetSubnetMask(), out mask))
			{
				byte[] bytes = mask.GetAddressBytes();
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

		private void MoveToNextOctet(TextBox currentTextBox)
		{
			if (currentTextBox == FirstOctet) SecondOctet.Focus();
			else if (currentTextBox == SecondOctet) ThirdOctet.Focus();
			else if (currentTextBox == ThirdOctet) FourthOctet.Focus();
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
			byte[] bytes = BitConverter.GetBytes(mask).Reverse().ToArray();
			MaskFirstOctet.Text = bytes[0].ToString();
			MaskSecondOctet.Text = bytes[1].ToString();
			MaskThirdOctet.Text = bytes[2].ToString();
			MaskFourthOctet.Text = bytes[3].ToString();
		}

		private void UpdateInfo()
		{
			try
			{
				string ipAddress = GetIpAddress();
				string subnetMask = GetSubnetMask();

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
			catch
			{
				InfoTextBlock.Text = ""; // очистка при ошибке
			}
		}

		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}