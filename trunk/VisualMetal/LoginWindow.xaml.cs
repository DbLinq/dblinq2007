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
		SqlMetalParameters parameters = new SqlMetalParameters();

		public LoginWindow()
		{
			InitializeComponent();

			DataContext = parameters;
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			parameters.Password = PasswordInput.Password; // can't bind to password for security reasons

			try
			{
				var loader = new LoaderFactory().Load(parameters);
				var database = SqlMetalProgram.LoadSchema(parameters, loader);

				MessageBox.Show("Schema loaded.");
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}
		}
	}
}
