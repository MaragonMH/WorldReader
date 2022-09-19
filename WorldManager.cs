using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        private MouseButtonEventHandler objectSelectionChanged;
        private RoutedEventHandler mapVisibilityChanged;
        private RoutedEventHandler objectVisibilityChanged;
        private SelectionChangedEventHandler tileMapObjectGroupChanged;


        public WorldManager(MainWindow parent, WorldDatastructur worldDatastructur, FileStream fileStream)
        {
            this.parent = parent;
            this.worldDatastructur = worldDatastructur;

            this.tileMap = new TileMap(fileStream, worldDatastructur.WidthTiles, worldDatastructur.HeightTiles);
            this.visualHost = new TileVisuals(tileMap);
            parent.TileCanvas.Children.Add(visualHost);

            objectSelectionChanged = new MouseButtonEventHandler(selectObject);
            mapVisibilityChanged = new RoutedEventHandler(changeMapVisibility);
            objectVisibilityChanged = new RoutedEventHandler(changeObjectVisibility);
            tileMapObjectGroupChanged = new SelectionChangedEventHandler(changeTileMapGroupVisibility);

            parent.disableEventHandler = true;

            // Build Window Components
            buildComponents();

            // Fill General Loaded Values
            fillLoadedValues();

            loaded = true;
        }

        public void renderMap()
        {
            if (!loaded) return;
            buildGroupSpecificComponents();
            tileMap.unsetAllTiles();
            renderCurrentMap();
            parent.disableEventHandler = false;
        }

        public void Clean()
        {
            parent.TileMapGroups.SelectionChanged -= tileMapObjectGroupChanged;
            parent.Layers.SubmenuClosed -= mapVisibilityChanged;
            foreach(CheckBox checkbox in parent.MapObjectHeaderGroups.Items)
            {
                checkbox.Checked-= objectVisibilityChanged;
                checkbox.Unchecked -= objectVisibilityChanged;
            }
            foreach (Canvas canvas in parent.TileCanvas.Children.OfType<Canvas>())
            {
                foreach (Rectangle rectangle in canvas.Children)
                {
                    rectangle.MouseLeftButtonUp -= objectSelectionChanged;
                }
            }
        }

        public void selectTile(int x, int y)
        {
            if(!loaded) return;
            parent.disableEventHandler = true;
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
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.Square:
                    parent.CollisionShapeSquare.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.LineFilled:
                    parent.CollisionShapeLineFilled.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.Platform:
                    parent.CollisionShapePlatform.IsChecked = true;
                    break;
                case WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileCollisionShape.Invalid:
                    parent.CollisionShapeInvalid.IsChecked = true;
                    break;
            }

            // Tile appearance
            // Clear all boxes
            foreach (WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer)))
            {
                TextBox textBox = (TextBox)parent.FindName(renderLayer.ToString());
                textBox.Text = "";
            }
            foreach (KeyValuePair<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer, uint> appearance in tile.appearanceTile.TileAppearanceIndex)
            {
                TextBox textBox = (TextBox)parent.FindName(appearance.Key.ToString());
                if(textBox != null)
                {
                    CheckBox flipHorizontally = (CheckBox)parent.FindName(appearance.Key.ToString() + "FlagFlipHorizontally");
                    CheckBox flipVertically = (CheckBox)parent.FindName(appearance.Key.ToString() + "FlagFlipVertically");

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

            parent.disableEventHandler = false;
        }

        // Attached to every mapObject Rectangle
        private void selectObject(object sender, MouseButtonEventArgs e)
        {
            if (!loaded) return;
            // This may lead to crashes if you open the object menu
            buildGroupSpecificComponents();

            parent.disableEventHandler = true;

            // Maybe there is some confliction potential here if the objects are overlapping and this is called multiple times
            // However this does not seem to be the case every click results in only one call
            int id = int.Parse(((Rectangle)sender).Name.Replace("MapObjectRectangle", ""));
            WorldDatastructur.TileMapGroup.MapObject mapObject = worldDatastructur.tileMapGroups[getCurrentTileMapGroup()].mapObjects.FirstOrDefault(o => o.id == id);

            // Display ObjectGroupName
            ComboBoxItem comboBoxItem3 = (ComboBoxItem)parent.MapObjectGroupName.FindName($"MapObjectGroupNameItem{mapObject.mapObjectGroupName}");
            if (comboBoxItem3 != null) comboBoxItem3.IsSelected = true;
            else parent.MapObjectGroupName.SelectedIndex = 0;

            // Display ObjectGroup
            ComboBoxItem comboBoxItem = (ComboBoxItem)parent.MapObjectGroupType.FindName($"MapObjectGroupTypeItem{mapObject.mapObjectGroupType}");
            if (comboBoxItem != null) comboBoxItem.IsSelected = true;
            else parent.MapObjectGroupType.SelectedIndex = 0;

            // Display properties
            bool isPropertySet = false;
            foreach (ComboBoxItem comboBoxItem2 in parent.MapObjectProperties.Items)
            {
                if (comboBoxItem2.Name.StartsWith($"MapObjectPropertiesItem{mapObject.mapObjectGroupType}")) comboBoxItem2.Visibility = Visibility.Visible;
                else comboBoxItem2.Visibility = Visibility.Collapsed;
                if ((string)comboBoxItem2.Content == mapObject.PropertiesToString())
                {
                    comboBoxItem2.IsSelected = true;
                    isPropertySet = true;
                }
            }
            if (!isPropertySet) parent.MapObjectProperties.SelectedIndex = 0;

            // Display vertices
            bool isVerticesSet = false;
            foreach (ComboBoxItem comboBoxItem2 in parent.MapObjectVertices.Items)
            {
                if ((string)comboBoxItem2.Content == mapObject.VerticesToString())
                {
                    comboBoxItem2.IsSelected = true;
                    isVerticesSet = true;
                }
            }
            if (!isVerticesSet) parent.MapObjectVertices.SelectedIndex = 0;

            parent.MapObjectName.Text = mapObject.name;
            parent.MapObjectId.Text = id.ToString();
            parent.MapObjectBoundsPixelX.Text = mapObject.boundsPixelX.ToString();
            parent.MapObjectBoundsPixelY.Text = mapObject.boundsPixelY.ToString();
            parent.MapObjectWidth.Text = mapObject.boundsPixelWidth.ToString();
            parent.MapObjectHeight.Text = mapObject.boundsPixelHeight.ToString();
            parent.MapObjectFlipHorizontally.IsChecked = mapObject.flipH;
            parent.MapObjectFlipVertically.IsChecked = mapObject.flipV;
            parent.MapObjectRotation.Text = mapObject.rotation.ToString();

            parent.disableEventHandler = false;
        }

        public void writeTile(int x, int y)
        {
            if (!loaded) return;
            parent.disableEventHandler = true;
            // Get Tile
            string currentTileMapGroup = getCurrentTileMapGroup();
            WorldDatastructur.TileMapGroup.Tile tile = worldDatastructur.tileMapGroups[currentTileMapGroup].tiles[y * worldDatastructur.WidthTiles + x];

            tile.PropertyIndex2 = parent.PropertyIndex2.SelectedIndex == 0 ? int.MaxValue : parent.PropertyIndex2.SelectedIndex - 1;

            // Write Collision Tile
            tile.collisionTile.LineSegmentIndex3 = parent.LineSegementIndex3.SelectedIndex == 0 ? int.MaxValue : parent.LineSegementIndex3.SelectedIndex - 1;
            tile.collisionTile.CollisionFlag = normalizeTextUInt(parent.CollisionFlag.Text);

            // Write Appearance Tile
            foreach (WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer)))
            {
                TextBox textBox = (TextBox)parent.FindName(renderLayer.ToString());
                if (textBox.Text != "") tile.appearanceTile.TileAppearanceIndex[renderLayer] = normalizeTextUInt(textBox.Text);
                else tile.appearanceTile.TileAppearanceIndex.Remove(renderLayer);
            }
            renderTile(x, y, currentTileMapGroup, getSelectedRenderLayers());

            parent.disableEventHandler = false;
        }

        public void writeCurrentObject()
        {
            if (!loaded) return;
            parent.disableEventHandler = true;
            // Get current Object
            int id = (int)normalizeTextUInt(parent.MapObjectId.Text);
            WorldDatastructur.TileMapGroup.MapObject mapObject = worldDatastructur.tileMapGroups[getCurrentTileMapGroup()].mapObjects.FirstOrDefault(o => o.id == id);

            // Write ObjectGroupName
            mapObject.mapObjectGroupName = parent.MapObjectGroupName.SelectedValue.ToString();

            // Write ObjectGroup -- Done in properties as well
            mapObject.mapObjectGroupType = (WorldDatastructur.TileMapGroup.MapObjectGroupType)Enum.Parse(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType), parent.MapObjectGroupType.SelectedValue.ToString());

            // Write properties
            mapObject.PropertiesFromString(parent.MapObjectProperties.Text?.ToString());

            // Write vertices
            mapObject.VerticesFromString(parent.MapObjectVertices.SelectedValue.ToString());

            mapObject.name = parent.MapObjectName.Text;
            mapObject.boundsPixelX = normalizeTextFloat(parent.MapObjectBoundsPixelX.Text);
            mapObject.boundsPixelY = normalizeTextFloat(parent.MapObjectBoundsPixelY.Text);
            mapObject.boundsPixelWidth = normalizeTextFloat(parent.MapObjectWidth.Text);
            mapObject.boundsPixelHeight = normalizeTextFloat(parent.MapObjectHeight.Text);
            mapObject.flipH = (bool)parent.MapObjectFlipHorizontally.IsChecked;
            mapObject.flipV = (bool)parent.MapObjectFlipVertically.IsChecked;
            mapObject.rotation = normalizeTextFloat(parent.MapObjectRotation.Text);

            // Change CurrentObject Visuals as well
            Rectangle displayedObject = (Rectangle)parent.TileCanvas.FindName($"MapObjectRectangle{id.ToString()}");
            Canvas.SetLeft(displayedObject, mapObject.boundsPixelX);
            Canvas.SetTop(displayedObject, mapObject.boundsPixelY);
            displayedObject.Width = mapObject.boundsPixelWidth;
            displayedObject.Height = mapObject.boundsPixelHeight;
            ((RotateTransform)displayedObject.LayoutTransform).Angle = mapObject.rotation;

            parent.disableEventHandler = false;
		}

        // Attached to mapObjectHeaderGroupsCheckBoxes
        private void changeObjectVisibility(object sender, RoutedEventArgs e)
        {
            showObjectGroups();
        }

        // Attached to and layers
        private void changeMapVisibility(object sender, RoutedEventArgs e)
        {
            renderMap();
        }

        // Attached to tileMapGroups
        private void changeTileMapGroupVisibility(object sender, SelectionChangedEventArgs e)
        {
            renderMap();
        }

        public void showObjectGroups()
        {
            foreach(CheckBox checkBox in parent.MapObjectHeaderGroups.Items)
            {
                Canvas canvas = (Canvas)parent.TileCanvas.FindName($"MapObjectGroupsCanvasItems{((string)checkBox.Content).ToString()}");
                if((bool)checkBox.IsChecked) canvas.Visibility = Visibility.Visible;
                else canvas.Visibility = Visibility.Collapsed;
            }
        }


        // Build these if you load a file
        private void buildComponents()
        {
            buildTileMapGroups();
            buildLayers();
            buildMapObjectGroupType();
            buildMapObjectHeaderGroups();
            buildMapObjectCanvasGroups();
        }

        // Build these if you swap Layer or TileMapGroup or load a file (but then after buildComponents())
        private void buildGroupSpecificComponents()
        {
            parent.disableEventHandler = true;
            string currentTileMapGroup = getCurrentTileMapGroup();
            buildGroupSpecificPropertyIndex2(currentTileMapGroup);
            buildGroupSpecificLineSegmentIndex3(currentTileMapGroup);
            buildGroupSpecificMapObjectGroupName(currentTileMapGroup);
            buildGroupSpecificMapObjectProperties(currentTileMapGroup);
            buildGroupSpecificMapObjectVertices(currentTileMapGroup);
            buildGroupSpecificMapObjectVisuals(currentTileMapGroup);
            parent.disableEventHandler = false;
        }

        // BUILD COMPONENTS
        private void buildTileMapGroups()
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
            parent.TileMapGroups.SelectionChanged += tileMapObjectGroupChanged;
        }

        private void buildLayers()
        {
            // Add Layers
            parent.Layers.Items.Clear();
            foreach (WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer)))
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = $"Layer{renderLayer.ToString()}";
                checkBox.Content = renderLayer.ToString();
                checkBox.Width = parent.Layers.Width;
                checkBox.Height = parent.Layers.Height;
                if (renderLayer.ToString() != "PreFGWater") checkBox.IsChecked = true;
                else checkBox.IsChecked = false;
                parent.Layers.Items.Add(checkBox);
            }
            parent.Layers.SubmenuClosed += mapVisibilityChanged;
        }

        private void buildGroupSpecificPropertyIndex2(string currentTileMapGroup)
        {
            // Add PropertyIndex2
            removeAllButFirst(parent.PropertyIndex2);
            foreach (var property in this.worldDatastructur.tileMapGroups[currentTileMapGroup].properties.Select((value, i) => (value, i)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"PropertyIndex2Item{property.i + 1}"; // Add 1 to account for no element
                comboBoxItem.Content = property.value.ToString();
                parent.PropertyIndex2.Items.Add(comboBoxItem);
            }
        }

        private void buildGroupSpecificLineSegmentIndex3(string currentTileMapGroup)
        {
            // Add LineSegmentIndex3
            removeAllButFirst(parent.LineSegementIndex3);
            foreach (var lineSegment in this.worldDatastructur.tileMapGroups[currentTileMapGroup].lineSegments.Select((value, i) => (value, i)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"PropertyIndex2Item{lineSegment.i + 1}"; // Add 1 to account for no element
                comboBoxItem.Content = lineSegment.value.ToString();
                parent.LineSegementIndex3.Items.Add(comboBoxItem);
            }
        }

        private void buildMapObjectGroupType()
        {             
            // Add MapObjectGroupType
            removeAllButFirst(parent.MapObjectGroupType);
            foreach (WorldDatastructur.TileMapGroup.MapObjectGroupType mapObjectGroupType in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectGroupTypeItem{mapObjectGroupType}";
                comboBoxItem.Content = mapObjectGroupType.ToString();
                parent.MapObjectGroupType.Items.Add(comboBoxItem);
                if (parent.MapObjectGroupType.FindName($"MapObjectGroupTypeItem{mapObjectGroupType}") != null) parent.MapObjectGroupType.UnregisterName($"MapObjectGroupTypeItem{mapObjectGroupType}");
                parent.MapObjectGroupType.RegisterName($"MapObjectGroupTypeItem{mapObjectGroupType}", comboBoxItem);
            }
        }

        private void buildGroupSpecificMapObjectGroupName(string currentTileMapGroup)
        {
            // Add MapObject Properties
            removeAllButFirst(parent.MapObjectGroupName);
            foreach (WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects.GroupBy(o => o.mapObjectGroupName).Select(group => group.First()))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectGroupNameItem{mapObject.mapObjectGroupName}";
                comboBoxItem.Content = mapObject.mapObjectGroupName;
                parent.MapObjectGroupName.Items.Add(comboBoxItem);
                if (parent.MapObjectGroupName.FindName($"MapObjectGroupNameItem{mapObject.mapObjectGroupName}") != null) parent.MapObjectGroupName.UnregisterName($"MapObjectGroupNameItem{mapObject.mapObjectGroupName}");
                parent.MapObjectGroupName.RegisterName($"MapObjectGroupNameItem{mapObject.mapObjectGroupName}", comboBoxItem);
            }
        }

        private void buildGroupSpecificMapObjectProperties(string currentTileMapGroup)
        {
            // Add MapObject Properties
            removeAllButFirst(parent.MapObjectProperties);
            foreach (WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectPropertiesItem{mapObject.mapObjectGroupType.ToString()}";
                comboBoxItem.Content = mapObject.PropertiesToString();
                comboBoxItem.Visibility = Visibility.Collapsed;
                parent.MapObjectProperties.Items.Add(comboBoxItem);
            }
        } 

        private void buildGroupSpecificMapObjectVertices(string currentTileMapGroup)
        {
            // Add MapObject Route Vertices
            removeAllButFirst(parent.MapObjectVertices);
            foreach (WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects)
            {
                string content = mapObject.VerticesToString();
                if (content == "") continue;
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Name = $"MapObjectPropertiesItem{mapObject.mapObjectGroupType}";
                comboBoxItem.Content = content;
                parent.MapObjectVertices.Items.Add(comboBoxItem);
            }
        }

        private void buildMapObjectHeaderGroups()
        {
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
                checkBox.Checked += new RoutedEventHandler(changeObjectVisibility);
                checkBox.Unchecked += new RoutedEventHandler(changeObjectVisibility);
                parent.MapObjectHeaderGroups.Items.Add(checkBox);
                if (parent.MapObjectHeaderGroups.FindName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}") != null) parent.MapObjectHeaderGroups.UnregisterName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}");
                parent.MapObjectHeaderGroups.RegisterName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}", checkBox);
            }
        }

        private void buildMapObjectCanvasGroups()
        {
            // Add MapObject Canvas Groups for Rectangle

            foreach (WorldDatastructur.TileMapGroup.MapObjectGroupType mapObjectGroupType in Enum.GetValues(typeof(WorldDatastructur.TileMapGroup.MapObjectGroupType)))
            {
                if (parent.FindName($"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}") == null)
                {
                    Canvas canvas = new Canvas();
                    canvas.Name = $"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}";
                    canvas.Visibility = Visibility.Collapsed;
                    CheckBox mapObjectHeaderGroup = (CheckBox)parent.MapObjectHeaderGroups.FindName($"MapObjectHeaderGroupsItems{mapObjectGroupType.ToString()}");
                    if ((bool)mapObjectHeaderGroup.IsChecked) canvas.Visibility = Visibility.Visible;
                    Canvas.SetZIndex(canvas, 1);
                    parent.TileCanvas.Children.Add(canvas);
                    if(parent.TileCanvas.FindName($"MapObjectGroupsCanvasItems{ mapObjectGroupType.ToString()}") != null) parent.TileCanvas.UnregisterName($"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}");
                    parent.TileCanvas.RegisterName($"MapObjectGroupsCanvasItems{mapObjectGroupType.ToString()}", canvas);
                }
            }
        }

        private void buildGroupSpecificMapObjectVisuals(string currentTileMapGroup)
        {
            cleanMapObjects();
            foreach(WorldDatastructur.TileMapGroup.MapObject mapObject in worldDatastructur.tileMapGroups[currentTileMapGroup].mapObjects)
            {
                Rectangle displayedObject = new Rectangle();
                displayedObject.Name = $"MapObjectRectangle{mapObject.id.ToString()}";
                displayedObject.Fill = (SolidColorBrush)Application.Current.Resources["ObjectColorBrush"];
                displayedObject.Opacity = 0.3;
                Canvas.SetLeft(displayedObject, mapObject.boundsPixelX);
                Canvas.SetTop(displayedObject, mapObject.boundsPixelY);
                displayedObject.Width = mapObject.boundsPixelWidth;
                displayedObject.Height = mapObject.boundsPixelHeight;
                displayedObject.MinWidth = 3; // Set Min Width to avoid accidental object invisibility
                displayedObject.MinHeight = 3; // Set Min Height to avoid accidental object invisibility
                displayedObject.LayoutTransform = new RotateTransform(mapObject.rotation);
                Canvas groupCanvas = (Canvas)parent.TileCanvas.FindName($"MapObjectGroupsCanvasItems{mapObject.mapObjectGroupType.ToString()}");
                groupCanvas.Children.Add(displayedObject);
                if(parent.TileCanvas.FindName($"MapObjectRectangle{mapObject.id.ToString()}") != null) parent.TileCanvas.UnregisterName($"MapObjectRectangle{mapObject.id.ToString()}");
                parent.TileCanvas.RegisterName($"MapObjectRectangle{mapObject.id.ToString()}", displayedObject);
                displayedObject.MouseLeftButtonUp += objectSelectionChanged;
            }
        }

        private void cleanMapObjects()
        {
            foreach(Canvas canvas in parent.TileCanvas.Children.OfType<Canvas>())
            {
                canvas.Children.Clear();
            }
        }

        // LOAD
        private void fillLoadedValues()
        {
            parent.WorldWidth.Text = worldDatastructur.WidthTiles.ToString();
            parent.WorldHeight.Text = worldDatastructur.HeightTiles.ToString();
            parent.TileSetMapWidth.Text = tileMap.width.ToString();
            parent.TileSetMapHeight.Text = tileMap.height.ToString();
        }

        // RENDER
        private void renderCurrentMap()
        {
            // Depends on the selected TileMapGroup and RenderLayers in the Toolbar
            string currentTileMap = getCurrentTileMapGroup();
            List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> usedRenderLayers = getSelectedRenderLayers();

            for (int height = 0; height < worldDatastructur.HeightTiles; height++)
            {
                for (int width = 0; width < worldDatastructur.WidthTiles; width++)
                {
                    renderTile(width, height, currentTileMap, usedRenderLayers);
                }
            }
        }
        private void renderTile(int width, int height, string currentTileMap, List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> usedRenderLayers)
        {
            int tileIndex = width + height * worldDatastructur.WidthTiles;
            bool overlay = false;
            foreach (KeyValuePair<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer, UInt32> appearance in worldDatastructur.tileMapGroups[currentTileMap].tiles[tileIndex].appearanceTile.TileAppearanceIndex)
            {
                if (usedRenderLayers.Contains(appearance.Key))
                {
                    tileMap.setTile(width, height, (int)appearance.Value, overlay);
                    overlay = true;
                }
            }
        }

        // UTIL
        public void printMap(string filename)
        {
            tileMap.SaveWorldImage(filename);
        }

        // HELPER
        private string getCurrentTileMapGroup()
        {
            return (string)parent.TileMapGroups.SelectedValue;
        }

        private void removeAllButFirst(ComboBox comboBox)
        {
            if (comboBox.Items.Count > 1)
            {
                for(int i = 1; i < comboBox.Items.Count; i++)
                {
                    comboBox.Items.RemoveAt(1);
                }
            }
        }

        private uint normalizeTextUInt(string str)
        {
            if (uint.TryParse(str, out uint value)) return value;
            return 0;
        }

        private float normalizeTextFloat(string str)
        {
            if(float.TryParse(str, out float value)) return value;
            return 0;
        }

        private List<WorldDatastructur.TileMapGroup.Tile.AppearanceTile.RenderLayer> getSelectedRenderLayers()
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
