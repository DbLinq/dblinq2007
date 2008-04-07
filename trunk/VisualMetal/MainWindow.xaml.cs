﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DbLinq.Vendor;
using DbLinq.Schema.Dbml;
using DbMetal;

namespace VisualMetal
{
	public partial class MainWindow : Window
	{
		public DbMetalProgram Program = new DbMetalProgram();
		public DbMetalParameters Parameters = new DbMetalParameters();
		public ISchemaLoader Loader;
		public Database Database;

		public MainWindow()
		{
			InitializeComponent();

			try
			{
				if (!String.IsNullOrEmpty(Properties.Settings.Default.Params))
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Settings.Default.Params)))
						Parameters = (DbMetalParameters)XamlReader.Load(stream);
			}
			catch { } // throw away any errors from parsing parameters
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (!Properties.Settings.Default.SavePassword)
				Parameters.Password = null; // clear password for security.
			Properties.Settings.Default.Params = XamlWriter.Save(Parameters);
			Properties.Settings.Default.Save();
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			new LoginWindow(this).ShowDialog();
		}

		public bool LoadSchema()
		{
			try
			{
				Loader = new LoaderFactory().Load(Parameters);
				Database = Program.LoadSchema(Parameters, Loader);

				TableList.ItemsSource = Database.Table;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
				return false;
			}
			return true;
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				ColumnList.ItemsSource = null;
			else
			{
				DbLinq.Schema.Dbml.Table selected = (DbLinq.Schema.Dbml.Table)e.AddedItems[0];
				ColumnList.ItemsSource = selected.Type.Items;
			}
		}

		private void GenerateCSharp_Click(object sender, RoutedEventArgs e)
		{
			if (Database == null)
			{
				MessageBox.Show("No database schema loaded.");
				return;
			}

			var dialog = new System.Windows.Forms.SaveFileDialog();
			dialog.Filter = "C# source files (*.cs)|*.cs|All files (*.*)|*.*";
			dialog.FileName = Parameters.Database;
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
				Program.GenerateCSharp(Parameters, Database, Loader, dialog.FileName);
		}

		private void SaveDbml_Click(object sender, RoutedEventArgs e)
		{
			if (Database == null)
			{
				MessageBox.Show("No database schema loaded.");
				return;
			}

			var dialog = new System.Windows.Forms.SaveFileDialog();
			dialog.Filter = "Database markup files (*.dbml)|*.dbml|All files (*.*)|*.*";
			dialog.FileName = Parameters.Database;
			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
				using (Stream dbmlFile = File.OpenWrite(dialog.FileName))
					DbLinq.Schema.DbmlSerializer.Write(dbmlFile, Database);
		}
	}
}