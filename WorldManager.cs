using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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

            // Initial Render
            renderCurrentMap();

            loaded = true;
        }

        public void renderMap()
        {
            if (!loaded) return;
            renderCurrentMap();
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
                checkBox.IsChecked = true;
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
                comboBoxItem.Name = $"PropertyIndex2Item{property.i}";
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
                comboBoxItem.Name = $"PropertyIndex2Item{lineSegment.i}";
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
        }

        private void renderCurrentMap()
        {
            tileMap.unsetAllTiles();

            // Depends on the selected TileMapGroup and RenderLayers in the Toolbar
            string currentTileMap = (string)parent.TileMapGroups.SelectedValue;
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
