using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IPCalculatorWPF.View
{
	public partial class IpAddressInput : UserControl
	{
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(IpAddressInput), new PropertyMetadata(""));

		public string Header
		{
			get => (string)GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		public string Address
		{
			get => $"{FirstOctet.Text}.{SecondOctet.Text}.{ThirdOctet.Text}.{FourthOctet.Text}";
			set
			{
				var parts = value?.Split('.');
				if (parts != null && parts.Length == 4)
				{
					FirstOctet.Text = parts[0];
					SecondOctet.Text = parts[1];
					ThirdOctet.Text = parts[2];
					FourthOctet.Text = parts[3];
				}
			}
		}

		public event TextChangedEventHandler OctetChanged;

		public IpAddressInput()
		{
			InitializeComponent();

			FirstOctet.TextChanged += OnOctetTextChanged;
			SecondOctet.TextChanged += OnOctetTextChanged;
			ThirdOctet.TextChanged += OnOctetTextChanged;
			FourthOctet.TextChanged += OnOctetTextChanged;
		}

		public void SetZeroValues()
		{
			FirstOctet.Text = "0";
			SecondOctet.Text = "0";
			ThirdOctet.Text = "0";
			FourthOctet.Text = "0";
		}
		private void OnOctetTextChanged(object sender, TextChangedEventArgs e)
		{
			OctetChanged?.Invoke(this, e);
		}
	}
}