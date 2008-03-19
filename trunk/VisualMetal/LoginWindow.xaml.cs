using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using SqlMetal;

namespace VisualMetal
{
	public partial class LoginWindow
	{
		MainWindow main;

		public LoginWindow(MainWindow window)
		{
			main = window;

			InitializeComponent();

			DataContext = main.Parameters;
			SavePasswordCheckBox.DataContext = Properties.Settings.Default;

			PasswordInput.Password = main.Parameters.Password; // can't bind to password for security reasons
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			main.Parameters.Password = PasswordInput.Password; // can't bind to password for security reasons
            //main.Parameters.Provider = Convert.ToString(comboProvider.SelectedValue);

			if (main.LoadSchema())
				Close();
		}

       
	}
}
