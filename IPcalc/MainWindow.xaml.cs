// MainWindow.xaml.cs
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Runtime.InteropServices;
using IPcalc;
using System.Windows.Controls;

namespace IPCalculatorWPF
{
    public partial class MainWindow : Window
    {
         private WinApiIpAddressHost ipControl;
        public MainWindow()
		{
            InitializeComponent();

			ipControl = new WinApiIpAddressHost();
			WindowsFormsHost host = new WindowsFormsHost();
			host.Child = ipControl;
			IpHostPlaceholder.Child = host;

			PrefixBox.Value = 24;
            UpdateMaskFromPrefix(24);
        }

        private void OnIpAddressChanged(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(IpTextBox.Text, out var ip))
            {
                byte first = ip.GetAddressBytes()[0];
                if (first < 128) PrefixBox.Value = 8;
                else if (first < 192) PrefixBox.Value = 16;
                else if (first < 224) PrefixBox.Value = 24;
            }
        }

        private void PrefixBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PrefixBox.Value.HasValue)
                UpdateMaskFromPrefix(PrefixBox.Value.Value);
        }

        private void MaskTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IPAddress.TryParse(MaskTextBox.Text, out var mask))
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
        }

        private void UpdateMaskFromPrefix(int prefix)
        {
            uint mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
            var bytes = BitConverter.GetBytes(mask).Reverse().ToArray();
            MaskTextBox.Text = string.Join(".", bytes);
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IPAddress.TryParse(IpTextBox.Text, out var ip))
                    throw new Exception("Некорректный IP-адрес.");

                if (!IPAddress.TryParse(MaskTextBox.Text, out var mask))
                    throw new Exception("Некорректная маска.");

                var ipBytes = ip.GetAddressBytes();
                var maskBytes = mask.GetAddressBytes();
                var networkBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);

                var broadcastBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                    broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);

                uint totalIps = (uint)Math.Pow(2, 32 - (PrefixBox.Value ?? 24));
                uint usableIps = totalIps > 2 ? totalIps - 2 : 0;

                InfoTextBlock.Text =
                    $"Адрес сети:\t\t{new IPAddress(networkBytes)}\n" +
                    $"Широковещательный адрес:\t{new IPAddress(broadcastBytes)}\n" +
                    $"Количество IP-адресов:\t{totalIps}\n" +
                    $"Количество узлов:\t\t{usableIps}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
