using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WorldReader
{
    public class WorldManager
    {
        private MainWindow parent = null;
        private WorldDatastructur worldDatastructur = null;
        private TileMap tileMap = null;
        private TileVisuals visualHost = null;

        private bool loaded = false;

        public WorldManager(MainWindow parent, WorldDatastructur worldDatastructur, FileStream fileStream)
        {
            this.parent = parent;
            this.worldDatastructur = worldDatastructur;

            this.tileMap = new TileMap(fileStream, worldDatastructur.WidthTiles, worldDatastructur.HeightTiles);
            this.visualHost = new TileVisuals(tileMap);
            parent.TileCanvas.Children.Add(visualHost);

            // Build Window Components
            buildComponents();

            // Fill General Loaded Values
            fillLoadedValues();

            // Load Map Objects
            loadMapObjects();

            loaded = true;
        }

        public void renderMap()
        {
            if (!loaded) return;
            renderCurrentMap();
        }

        public void selectTile(int x, int y)
        {
            if(!loaded) return;

            WorldDatastructur.TileMapGroup.Tile tile = worldDatastructur.tileMapGroups[getCurrentTileMapGroup()].tiles[x + y * worldDatastructur.WidthTiles];

            // Tile collision
            if(tile.PropertyIndex2 == int.MaxValue) parent.PropertyIndex2.SelectedIndex = 0;
            else parent.PropertyIndex2.SelectedIndex = tile.PropertyIndex2 + 1; // Add 1 to account for no value option
            if(tile.collisionTile.LineSegmentIndex3 == int.MaxValue) parent.LineSegementIndex3.SelectedIndex = 0;
            else parent.LineSegementIndex3.SelectedIndex = tile.collisionTile.LineSegmentIndex3 + 1; // Add 1 to account for no value option

            parent.CollisionFlag.Text = tile.collisionTile.CollisionFlag.ToString();
            foreach(WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags tileFlag in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags)))
            {
                CheckBox checkBox = (CheckBox)parent.FindName(tileFlag.ToString());
                if(checkBox != null)
                {
                    if (((WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags) tile.collisionTile.CollisionFlag).HasFlag(tileFlag))
                    {
                        if(tileFlag == WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagNone) // Special Check for FlagNone because HasFlag can not handle it
                        {
                            if(((WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags)tile.collisionTile.CollisionFlag) == WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagNone)
                            {
                                checkBox.IsChecked = true;
                            }
                            else
                            {
                                checkBox.IsChecked = false;
                            }
                        }
                        else
                        {
                            checkBox.IsChecked = true;
                        }
                    }
                    else{
                        checkBox.IsChecked = false;
                    }
                }
            }
            int tileShapeBits = (int)tile.collisionTile.CollisionFlag & (int)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.MaskCollisionShape;
            WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape tileShape = (WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape)tileShapeBits;
            switch (tileShape)
            {
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.SQUARE:
                    parent.CollisionShapeSquare.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.LINE_FILLED:
                    parent.CollisionShapeLineFilled.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.PLATFORM:
                    parent.CollisionShapePlatform.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.INVALID:
                    parent.CollisionShapeInvalid.IsChecked = true;
                    break;
            }

            // Tile appearance
            foreach (KeyValuePair<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer, uint> appearance in tile.appearanceTile.TileAppearanceIndex)
            {
                TextBox textBox = (TextBox)parent.FindName(appearance.Key.ToString());
                if(textBox != null)
                {
                    CheckBox flipHorizontally = (CheckBox)parent.FindName(appearance.Key.ToString() + "FlipHorizontally");
                    CheckBox flipVertically = (CheckBox)parent.FindName(appearance.Key.ToString() + "FlipVertically");

                    if(appearance.Value != uint.MaxValue)
                    {
                        textBox.Text = appearance.Value.ToString();

                        // Flip
                        bool isflippedHorizontally = ((appearance.Value & (uint)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipHorizontally) != 0);
                        bool isflippedVertically = ((appearance.Value & (uint)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipVertically) != 0);
                        flipHorizontally.IsChecked = isflippedHorizontally;
                        flipVertically.IsChecked = isflippedVertically;
                    }
                    else
                    {
                        textBox.Text = string.Empty;
                        flipHorizontally.IsChecked = false;
                        flipVertically.IsChecked = false;
                    }
                }
            }
        }

        private void selectObject(object sender, MouseButtonEventArgs e)
        {
            // Maybe there is some confliction potential here if the objects are overlapping and this is called multiple times
            // However this does not seem to be the case every click results in only one call
            
        }

        private void buildComponents()
        {
            // Add TileMapGroups
            parent.TileMapGroups.Items.Clear();
            bool first = true;
            foreach (string tileMapGroup in worldDatastructur.tileMapGroups.Keys)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"TileMapGroupItem{tileMapGroup}";
                comboBoxItem.Content = tileMapGroup;
                if (first)
                {
                    comboBoxItem.IsSelected = true;
                    first = false;
                }
                parent.TileMapGroups.Items.Add(comboBoxItem);
            }

            // Add Layers
            parent.Layers.Items.Clear();
            foreach (WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer)))
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = $"Layer{renderLayer.ToString()}";
                checkBox.Content = renderLayer.ToString();
                checkBox.Width = parent.Layers.Width;
                checkBox.Height = parent.Layers.Height;
                if(renderLayer.ToString() != "PreFGWater") checkBox.IsChecked = true;
                else checkBox.IsChecked = false;
                parent.Layers.Items.Add(checkBox);
            }

            // Add PropertyIndex2
            if (parent.PropertyIndex2.Items.Count > 1)
            {
                parent.PropertyIndex2.Items.Clear();
            }
            string currentTileMapGroup = parent.TileMapGroups.SelectedValue.ToString();
            foreach (var property in this.worldDatastructur.tileMapGroups[currentTileMapGroup].properties.Select((value, i) => (value, i)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"PropertyIndex2Item{property.i+1}"; // Add 1 to account for no element
                comboBoxItem.Content = property.value.ToString();
                parent.PropertyIndex2.Items.Add(comboBoxItem);
            }

            // Add LineSegmentIndex3
            if (parent.LineSegementIndex3.Items.Count > 1)
            {
                parent.LineSegementIndex3.Items.Clear();
            }
            foreach (var lineSegment in this.worldDatastructur.tileMapGroups[currentTileMapGroup].lineSegments.Select((value, i) => (value, i)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"PropertyIndex2Item{lineSegment.i+1}"; // Add 1 to account for no element
                comboBoxItem.Content = lineSegment.value.ToString();
                parent.LineSegementIndex3.Items.Add(comboBoxItem);
            }

            // Add MapObjectGroupType
            if (parent.MapObjectGroupType.Items.Count > 1)
            {
                parent.MapObjectGroupType.Items.Clear();
            }
            foreach (WorldDatastructur.TileMapGroup.MapObjectGroupType mapObjectGroupType in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectGroupTypeItem{mapObjectGroupType}";
                comboBoxItem.Content = mapObjectGroupType.ToString();
                parent.MapObjectGroupType.Items.Add(comboBoxItem);
            }

            // Add MapObject Route Vertices
            if (parent.MapObjectVertices.Items.Count > 1)
            {
                parent.MapObjectVertices.Items.Clear();
            }
            foreach (WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects)
            {
                string content = mapObject.VerticesToString();
                if (content == "") continue;
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectPropertiesItem{mapObject.mapObjectGroupType}";
                comboBoxItem.Content = content;
                parent.MapObjectVertices.Items.Add(comboBoxItem);
            }


            // Add MapObject Properties
            if (parent.MapObjectProperties.Items.Count > 1)
            {
                parent.MapObjectProperties.Items.Clear();
            }
            foreach (WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectPropertiesItem{mapObject.mapObjectGroupType}";
                comboBoxItem.Content = mapObject.PropertiesToString();
                comboBoxItem.Visibility = Visibility.Collapsed;
                parent.MapObjectProperties.Items.Add(comboBoxItem);
            }

            // Add MapObjectHeaderGroups for Toolbar
            parent.MapObjectHeaderGroups.Items.Clear();
            foreach (WorldDatastructur.TileMapGroup.MapObjectGroupType mapObjectGroupType in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType)))
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = $"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}";
                checkBox.Content = mapObjectGroupType.ToString();
                checkBox.Width = parent.MapObjectHeaderGroups.Width;
                checkBox.Height = parent.MapObjectHeaderGroups.Height;
                // Disable the two biggest group types for better visibility
                if ((new[] { "Room", "Generic" }).Contains(mapObjectGroupType.ToString())) checkBox.IsChecked = false;
                else checkBox.IsChecked = true;
                parent.MapObjectHeaderGroups.Items.Add(checkBox);
                parent.MapObjectHeaderGroups.RegisterName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}", checkBox);
            }

            // Add MapObject Canvas Groups for Rectangle
            foreach (WorldDatastructur.TileMapGroup.MapObjectGroupType mapObjectGroupType in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType)))
            {
                if (parent.FindName($"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}") == null)
                {
                    Canvas canvas = new Canvas();
                    canvas.Name = $"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}";
                    canvas.Visibility = Visibility.Collapsed;
                    CheckBox mapObjectHeaderGroup = (CheckBox)parent.MapObjectHeaderGroups.FindName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}");
                    if((bool)mapObjectHeaderGroup.IsChecked) canvas.Visibility = Visibility.Visible;
                    Canvas.SetZIndex(canvas, 1);
                    parent.TileCanvas.Children.Add(canvas);
                    parent.TileCanvas.RegisterName($"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}", canvas);
                }
            }
        }

        private void fillLoadedValues()
        {
            parent.WorldWidth.Text = worldDatastructur.WidthTiles.ToString();
            parent.WorldHeight.Text = worldDatastructur.HeightTiles.ToString();
            parent.TileSetMapWidth.Text = tileMap.width.ToString();
            parent.TileSetMapHeight.Text = tileMap.height.ToString();
        }

        private void loadMapObjects()
        {
            foreach(WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[getCurrentTileMapGroup()].mapObjects)
            {
                Rectangle displayedObject = new Rectangle();
                displayedObject.Name = $"MapObjectRectangle{mapObject.id.ToString()}";
                displayedObject.Fill = (SolidColorBrush)Application.Current.Resources["ObjectColorBrush"];
                displayedObject.Opacity = 0.3;
                Canvas.SetLeft(displayedObject, mapObject.boundsPixelX);
                Canvas.SetTop(displayedObject, mapObject.boundsPixelY);
                displayedObject.Width = mapObject.boundsPixelWidth;
                displayedObject.Height = mapObject.boundsPixelHeight;
                Canvas groupCanvas = (Canvas)parent.TileCanvas.FindName($"MapObjectGroupsCanvasItems{mapObject.mapObjectGroupType.ToString()}");
                groupCanvas.Children.Add(displayedObject);
                displayedObject.MouseLeftButtonUp += new MouseButtonEventHandler(selectObject);
            }
        }

        private void renderCurrentMap()
        {
            tileMap.unsetAllTiles();

            // Depends on the selected TileMapGroup and RenderLayers in the Toolbar
            string currentTileMap = getCurrentTileMapGroup();
            List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> usedRenderLayers = getSelectedRenderLayers(parent);

            for (int height = 0; height < worldDatastructur.HeightTiles; height++)
            {
                for (int width = 0; width < worldDatastructur.WidthTiles; width++)
                {
                    int tileIndex = width + height * worldDatastructur.WidthTiles;
                    bool overlay = false;
                    foreach (KeyValuePair< WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer, UInt32> appearance in worldDatastructur.tileMapGroups[currentTileMap].tiles[tileIndex].appearanceTile.TileAppearanceIndex)
                    {
                        if (usedRenderLayers.Contains(appearance.Key))
                        {
                            tileMap.setTile(width, height, (int)appearance.Value, overlay);
                            overlay = true;
                        }
                    }
                }
            }
        }

        private string getCurrentTileMapGroup()
        {
            return (string)parent.TileMapGroups.SelectedValue;
        }

        private List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> getSelectedRenderLayers(MainWindow parent)
        {
            List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> tempList = new List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer>();
            foreach(CheckBox element in parent.Layers.Items)
            {
                if ((bool)element.IsChecked)
                {
                    tempList.Add((WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer) Enum.Parse(typeof(WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer), (string)element.Content)); 
                }
            }
            return tempList;
        }

    }
}
