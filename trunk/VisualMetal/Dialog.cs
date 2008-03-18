using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace VisualMetal
{
	public class Dialog : Window
	{
		public Dialog()
		{
            Background = SystemColors.ControlBrush;

            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.ToolWindow;
            SizeToContent = SizeToContent.WidthAndHeight;

            // these can't be set via a style as they are not dependency props
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Note: Setting the owner causes the WPF designer to not be able to instantiate.
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                var windows = Application.Current.Windows;
                Owner = windows[windows.Count - 2];
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Resources.MergedDictionaries.Add(Application.Current.Resources);
        }
	}
}
