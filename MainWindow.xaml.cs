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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace WorldReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private WorldDatastructur worldDatastructur = null;
		private WorldManager worldManager = null;
		public bool disableEventHandler = false;

		public class MenuItem
		{
			public MenuItem()
			{
				this.Items = new ObservableCollection<MenuItem>();
			}

			public string Title { get; set; }

			public ObservableCollection<MenuItem> Items { get; set; }
		}

		public MainWindow()
		{
			InitializeComponent();
		}

        ~MainWindow()
        {
			this.worldDatastructur = null;
			this.worldManager = null;
        }

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				OpenFileDialog dlg0 = new OpenFileDialog();
				dlg0.Filter = "AV2 World File (*.xnb)|*.xnb";
				if (dlg0.ShowDialog() == true && dlg0.FileName.EndsWith(".xnb"))
				{
					using (BinaryReader binaryReader = new BinaryReader(File.Open(dlg0.FileName, FileMode.Open, FileAccess.Read)))
					{
						this.worldDatastructur = new WorldDatastructur(binaryReader);
					}


					OpenFileDialog dlg1 = new OpenFileDialog();
					dlg1.Filter = "AV2 TileMap File(*.png)|*.png";
					if (dlg1.ShowDialog() == true && dlg1.FileName.EndsWith(".png"))
					{
						// Remove old data
						this.worldManager?.Clean();

						FileStream fileStream = new FileStream(dlg1.FileName, FileMode.Open, FileAccess.Read);
						this.worldManager = new WorldManager(this, worldDatastructur, fileStream);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error while reading file: " + ex.Message, "File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			if(this.worldManager != null)
            {
				this.worldManager.renderMap();
            }
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			try
            {
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.Filter = "Axiom Verge 2 World File (*.xnb)|*.xnb|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{
					using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(dlg.FileName, FileMode.Create, FileAccess.Write)))
					{
						this.worldDatastructur.Write(binaryWriter);
					}
				}
            }
			catch (Exception ex)
			{
				MessageBox.Show("Error while writing file: " + ex.Message, "File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

		private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void ZoomToFit(object sender, RoutedEventArgs e)
		{
			var st = (ScaleTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is ScaleTransform);
			var tt = (TranslateTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is TranslateTransform);

			st.CenterX = 0;
			st.CenterY = 0;

			tt.X = 0;
			tt.Y = 0;
		}

		private void Edit(object sender, RoutedEventArgs e)
        {
			worldManager?.showObjectGroups();
        }

        // ZOOM CANVAS
        private Double zoomMax = 2;
        private Double zoomMin = 0.1;
        private Double zoomSpeed = 0.0005;
        private Double zoom = 1;
		private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
		{

			var st = (ScaleTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is ScaleTransform);
			var tt = (TranslateTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is TranslateTransform);

			double oldZoom = zoom;
			zoom += zoomSpeed * e.Delta; // Ajust zooming speed (e.Delta = Mouse spin value )
			if (zoom < zoomMin) { zoom = zoomMin; } // Limit Min Scale
			if (zoom > zoomMax) { zoom = zoomMax; } // Limit Max Scale

            Point relative = e.GetPosition(TileCanvas);
			Point absolute = e.GetPosition(ScrollContainer);

			// MessageBox.Show($"absolute: x: {absolute.X}, y: {absolute.Y} \n current: x:{st.CenterX}, y: {st.CenterY}");

            st.ScaleX = zoom;
            st.ScaleY = zoom;

            st.CenterX = TileCanvas.ActualWidth / 2;
            st.CenterY = TileCanvas.ActualHeight / 2;

        }

		// PAN CANVAS
		Point last;
		void CanvasMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			TileCanvas.ReleaseMouseCapture();
		}

		void CanvasMouseMove(object sender, MouseEventArgs e)
		{
			// ZOOM
			/*
			var st = (ScaleTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is ScaleTransform);
			var tt = (TranslateTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is TranslateTransform);

			Point absolute = Mouse.GetPosition(ScrollContainer);

			st.CenterX = absolute.X;
			st.CenterY = absolute.Y;
			*/

			// PAN
			if (!TileCanvas.IsMouseCaptured)
				return;
			var t = (TranslateTransform)((TransformGroup)TileCanvas.RenderTransform).Children.First(tr => tr is TranslateTransform);
			Vector v = last - e.GetPosition(TileCanvas);
			t.X -= v.X;
			t.Y -= v.Y;
			last = e.GetPosition(TileCanvas);
		}
		void CanvasMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			last = e.GetPosition(TileCanvas);
			TileCanvas.CaptureMouse();
		}

		// SELECTION CANVAS
		Point selection;
		void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.worldManager != null)
			{
				Point relative = e.GetPosition(TileCanvas);
				if (relative.X < 0) selection.X = 0;
				else if(relative.X > 16 * worldDatastructur.WidthTiles - 1) selection.X = worldDatastructur.WidthTiles - 1;
				else selection.X = (int)relative.X / 16;
				if(relative.Y < 0) selection.Y = 0;
				else if (relative.Y > 16 * worldDatastructur.HeightTiles - 1) selection.Y = worldDatastructur.HeightTiles - 1;
				else selection.Y = (int)relative.Y / 16;
				Canvas.SetLeft(this.CanvasSelection, selection.X * 16);
				Canvas.SetTop(this.CanvasSelection, selection.Y * 16);
				this.CanvasSelection.Visibility = Visibility.Visible;
				this.worldManager.selectTile((int)selection.X, (int)selection.Y);
			}
		}

		// PROPERTIES MENU
		private void UpdateCollisionFlag(object sender, RoutedEventArgs e)
		{
			if (disableEventHandler) return;
			WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags flag = (WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags)Enum.Parse(typeof(WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags), ((CheckBox)sender).Name);
			uint collision = uint.Parse(this.CollisionFlag.Text);
			if ((bool)((CheckBox)sender).IsChecked) this.CollisionFlag.Text = (collision | (uint)flag).ToString();
			else this.CollisionFlag.Text = (collision & ~((uint)flag)).ToString();
		}
		private void UpdateAppearanceFlip(object sender, RoutedEventArgs e)
        {
			if (disableEventHandler) return;
			WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags flag = ((CheckBox)sender).Name.EndsWith("Horizontally") ? WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipHorizontally : WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipVertically;
			string layerName = ((CheckBox)sender).Name.Replace(flag.ToString(), "");
			TextBox textBox = (TextBox)this.FindName(layerName);
			uint appearance = uint.Parse(textBox.Text == "" ? "0" : textBox.Text);
			if ((bool)((CheckBox)sender).IsChecked) textBox.Text = (appearance | (uint)flag).ToString();
			else textBox.Text = (appearance & ~((uint)flag)).ToString();
			if (uint.Parse(textBox.Text) == 0) textBox.Text = "";
		}
		private void UpdateCollisionShape(object sender, RoutedEventArgs e)
        {
			if (disableEventHandler) return;
			WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape flag = (WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape)Enum.Parse(typeof(WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape), ((RadioButton)sender).Name.Replace("CollisionShape", ""));
			uint collision = uint.Parse(this.CollisionFlag.Text);
			this.CollisionFlag.Text = ( (collision & ~((uint)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.MaskCollisionShape)) | (uint)flag).ToString();
		}
		private void UpdateTile(object sender, RoutedEventArgs e)
        {
			if (disableEventHandler) return;
			this.worldManager?.writeTile((int)selection.X, (int)selection.Y);
		}
		private void UpdateMapObject(object sender, RoutedEventArgs e)
        {
			if (disableEventHandler) return;
			this.worldManager?.writeCurrentObject();
        }
		private void UpdateMapProperties(object sender, RoutedEventArgs e)
        {
			ComboBox comboBox = ((ComboBox)sender);
			string str = comboBox.SelectedValue?.ToString();
			if (str == null) return;
			foreach(ComboBoxItem comboBoxItem in this.MapObjectProperties.Items)
            {
				if (comboBoxItem.Name == $"MapObjectPropertiesItem{str}")
                {
					comboBoxItem.Visibility = Visibility.Visible;
                }
				else
                {
					comboBoxItem.Visibility = Visibility.Collapsed;
                }
            }
        }
	}
}
