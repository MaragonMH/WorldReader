using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WorldReader
{
    public class Randomizer
    {
        public class MapObjectPosition
        {
            public string tileMapGroup, newTileMapGroup;
            public float x, newX;
            public float y, newY;
            public uint id, newId;
            public int normalizedIndex, newNormalizedIndex;
            public string name;
            public int index, newIndex;

            public MapObjectPosition(string tileMapGroup, float x, float y, uint id, int normalizedIndex, string name, int index)
            {
                this.tileMapGroup = tileMapGroup;
                this.newTileMapGroup = tileMapGroup;
                this.x = x;
                this.newX = x;
                this.y = y;
                this.newY = y;
                this.id = id;
                this.newId = id;
                this.normalizedIndex = normalizedIndex;
                this.newNormalizedIndex = normalizedIndex;
                this.name = name;
                this.index = index;
                this.newIndex = index;
            }

            public void setNewNormalizedIndex(int newNormalizedIndex)
            {
                this.newNormalizedIndex = newNormalizedIndex;
            }
            public void setNewTileMapGroup(string newTileMapGroup)
            {
                this.newTileMapGroup = newTileMapGroup;
            }
            public void setNewPosition(float newX, float newY)
            {
                this.newX = newX;
                this.newY = newY;
            }
            public void setNewId(uint newId)
            {
                this.newId = newId;
            }
            public void setNewIndex(int newIndex)
            {
                this.newIndex = newIndex;
            }
        }

        // The following list mentions the object count in the game and corrects it if objects are not loaded by a seperate item or multiple objects.
        private int armCount = 18 + 1; // Game has 18 urns // Dun has two seperate Objects
        private int itemCount = 25 - 1; // Game has 25 items // Flashlight is given from the start 
        private int healthNodeFragmentCount = 25 - 1; // Game has 25 Health Fragments // Rotate Dispenser contains the health node fragment
        private int healthNodeCount = 5;
        private int apocalypseFlaskInBossCount = 9; // 9 Flasks are contained by bosses
        private int apocalypseFlaskCountCombined = 90 - 9; // Game has 90 Flasks // In Bosses
        private int apocalypseFlaskSCount = 56;
        private int apocalypseFlaskMCount = 34 - 9; // Game has 34 large Flasks // In Bosses
        private int notesCount = 37 + 1; // Game has 37 notes // Because first node is doubled
        // First note and Dun are doubled so substract 2 from object count

        private MainWindow parent = null;
        private WorldDatastructur worldDatastructur = null;
        private string seed = string.Empty;
        private List<MapObjectPosition> mapObjectPositions = null;


        public Randomizer(MainWindow parent, WorldDatastructur worldDatastructur)
        {
            this.parent = parent;
            this.worldDatastructur = worldDatastructur;
            this.mapObjectPositions = new List<MapObjectPosition>();

            readMapObjects();
            buildRandomize();
        }

        public void startRandomizer(string seed)
        {
            this.seed = seed;
            
            applySeed();
            writeMapObjects();
        }

        public void configureRandomizer(int selectorIndex, int designatorIndex)
        {
            // Disable new selection but enable old selection.
            int oldDesignatorIndex = mapObjectPositions[selectorIndex].newNormalizedIndex;
            ((ComboBoxItem)parent.RandomizeItemDesignator.Items[oldDesignatorIndex + 1]).IsEnabled = true;
            if(designatorIndex > 0) ((ComboBoxItem)parent.RandomizeItemDesignator.Items[designatorIndex]).IsEnabled = false;
            setNewObjectPosition(selectorIndex, designatorIndex - 1);
            
        }

        public int getConfigureRandomizer(int selectorIndex)
        {
            return mapObjectPositions[selectorIndex].newNormalizedIndex;
        }

        private void setNewObjectPosition(int oldIndex, int newIndex)
        {
            mapObjectPositions[oldIndex].setNewNormalizedIndex(newIndex);
            if(newIndex >= 0)
            {
                mapObjectPositions[oldIndex].setNewTileMapGroup(mapObjectPositions[newIndex].tileMapGroup);
                mapObjectPositions[oldIndex].setNewPosition(mapObjectPositions[newIndex].x, mapObjectPositions[newIndex].y);
                mapObjectPositions[oldIndex].setNewId(mapObjectPositions[newIndex].id);
                mapObjectPositions[oldIndex].setNewIndex(mapObjectPositions[newIndex].index);
            }
        }

        private void readMapObjects()
        {
            int normalizedIndex = 0;
            foreach(KeyValuePair<string, WorldDatastructur.TileMapGroup> tileMapGroup in worldDatastructur.tileMapGroups)
            {
                foreach(WorldDatastructur.TileMapGroup.MapObject mapObject in tileMapGroup.Value.mapObjects)
                {
                    if(mapObject.mapObjectGroupType == WorldDatastructur.TileMapGroup.MapObjectGroupType.Item || mapObject.mapObjectGroupType == WorldDatastructur.TileMapGroup.MapObjectGroupType.ApocalypseUrn)
                    {
                        uint id = mapObject.id;
                        int index = tileMapGroup.Value.mapObjects.IndexOf(mapObject);
                        string name = mapObject.name == "ApocalypseNode" ? mapObject.valuePairs["ItemName"] : mapObject.name; 
                        mapObjectPositions.Add(new MapObjectPosition(tileMapGroup.Key, mapObject.boundsPixelX, mapObject.boundsPixelY, id, normalizedIndex++, name, index));
                    }
                }
            }
        }

        private void buildRandomize()
        {
            parent.RandomizeItemSelector.Items.Clear();
            removeAllButFirst(parent.RandomizeItemDesignator);
            foreach(MapObjectPosition mapObjectPosition in mapObjectPositions)
            {
                ComboBoxItem comboBox1 = new ComboBoxItem();
                comboBox1.Name = $"RandomizeItemSelectorItem{mapObjectPosition.normalizedIndex}";
                comboBox1.Content = mapObjectPosition.name;
                parent.RandomizeItemSelector.Items.Add(comboBox1);

                ComboBoxItem comboBox2 = new ComboBoxItem();
                comboBox2.Name = $"RandomizeItemDesignatorItem{mapObjectPosition.normalizedIndex}";
                comboBox2.Content = mapObjectPosition.name;
                comboBox2.IsEnabled = false;
                parent.RandomizeItemDesignator.Items.Add(comboBox2);
            }
        }

        private void applySeed()
        {
            if (this.seed == "") return;
            Random rand = new Random(this.seed.GetHashCode());
            // These entries need to be randomized
            List<MapObjectPosition> newMapObjectPositions = mapObjectPositions.Where(o => o.normalizedIndex == o.newNormalizedIndex).ToList();
            List<MapObjectPosition> randomizedMapObjectPositions = newMapObjectPositions.OrderBy(_ => rand.Next()).ToList(); 
            foreach(var mapObjectPosition in randomizedMapObjectPositions.Select((value, i) => new { i, value }))
            {
                mapObjectPosition.value.newNormalizedIndex = newMapObjectPositions[mapObjectPosition.i].normalizedIndex;
                mapObjectPosition.value.newId = newMapObjectPositions[mapObjectPosition.i].id;
                mapObjectPosition.value.newX = newMapObjectPositions[mapObjectPosition.i].x;
                mapObjectPosition.value.newY = newMapObjectPositions[mapObjectPosition.i].y;
                mapObjectPosition.value.newTileMapGroup = newMapObjectPositions[mapObjectPosition.i].tileMapGroup;
                mapObjectPosition.value.newIndex = newMapObjectPositions[mapObjectPosition.i].index;
            }
        }

        private void writeMapObjects()
        {
            // Backup original values, only reference needs to be backed up because the values are not tinkered with.
            Dictionary<string, Dictionary<int, WorldDatastructur.TileMapGroup.MapObject>> backup = new Dictionary<string, Dictionary<int, WorldDatastructur.TileMapGroup.MapObject>>();
            foreach(var mapObjectPosition in mapObjectPositions)
            {
                if (!backup.ContainsKey(mapObjectPosition.tileMapGroup)) backup.Add(mapObjectPosition.tileMapGroup, new Dictionary<int, WorldDatastructur.TileMapGroup.MapObject>());
                backup[mapObjectPosition.tileMapGroup][mapObjectPosition.index] = worldDatastructur.tileMapGroups[mapObjectPosition.tileMapGroup].mapObjects[mapObjectPosition.index];
            }

            // Change id, position, tileMapGroup
            foreach(var mapObjectPosition in mapObjectPositions)
            {
                WorldDatastructur.TileMapGroup.MapObject mapObject = backup[mapObjectPosition.tileMapGroup][mapObjectPosition.index];
                mapObject.boundsPixelX = mapObjectPosition.newX;
                mapObject.boundsPixelY = mapObjectPosition.newY;
                mapObject.id = mapObjectPosition.newId;
                worldDatastructur.tileMapGroups[mapObjectPosition.newTileMapGroup].mapObjects[mapObjectPosition.newIndex] = mapObject;
            }
        }

        // Logic
        private bool logicCheck()
        {
            return true;
        }
        private bool isIndexAccesible(int normalizedIndex)
        {
            return true;
        }

        // Helper
        private void removeAllButFirst(ComboBox comboBox)
        {
            if (comboBox.Items.Count > 1)
            {
                for (int i = 1; i < comboBox.Items.Count; i++)
                {
                    comboBox.Items.RemoveAt(1);
                }
            }
        }
    }
}
