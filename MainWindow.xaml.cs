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
using System.IO;
using Microsoft.Win32;

namespace WorldReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private WorldDatastructur worldDatastructur = null;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "AV2 World File (*.xnb)|*.xnb|AV2 TileMap File(*.png)|*.png|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				using (BinaryReader binaryReader = new BinaryReader(File.Open(dlg.FileName, FileMode.Open, FileAccess.Read)))
                {
					this.worldDatastructur = new WorldDatastructur(binaryReader);
				}
			}
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Axiom Verge 2 World File (*.xnb)|*.xnb|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(dlg.FileName, FileMode.Create, FileAccess.Write)))
				{
					this.worldDatastructur.Write(binaryWriter);
					this.worldDatastructur = null;
				}
			}
		}
	}
}
