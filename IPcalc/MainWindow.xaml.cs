using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;  // fo using IPAddress

namespace IPcalc
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		private void IpChanged(object sender, RoutedEventArgs e)
		{
			if (IPAddress.TryParse(IpTextBox.Text, out var ip))
			{
				byte first = ip.GetAddressBytes()[0];
				if (first < 128) Spin.Value = 8;
				else if (first < 192) Spin.Value = 16;
				else if (first < 224) Spin.Value = 24;
			}
		}
		private void IpAddressChanged(object sender, RoutedEventArgs e)
		{
		
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{

        }
		private void OnCancelClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}
    }
}
