using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorldReader
{
    public class WorldDatastructur
    {
        // File Header
        private byte[] fileHeader = null;

        // General File Information
        private string WorldName;
        public Int32 WidthTiles;
        public Int32 HeightTiles;
        private Int32 TileSetWidth;
        private Int32 TileSetHeight;

        public Dictionary<string, TileMapGroup> tileMapGroups = null;

        public class TileMapGroup
        {

            public List<Tile> tiles = null;
            public List<MapObject> mapObjects = null;
            // Non visible Information
            public List<Properties> properties = null;
            public List<LineSegment> lineSegments = null;

            public enum MapObjectGroupType
            {
                ApocalypseUrn,
                Boss,
                BreachPortal,
                Damageable,
                DamageTrigger,
                ExteriorDoor,
                ForceField,
                Generic,
                GlitchableTile,
                GenericRegion,
                Item,
                Lattice,
                NPC,
                ParticleObject,
                Room,
                RoomAction,
                RoomTransition,
                PasscodeAction,
                SavePoint,
                SecretWorld,
                SecretWorldEntrance,
                SecretWorldItem,
                TileNPC,
                TriggerRegion,
                UdugDoor
            }

            public TileMapGroup()
            {
                this.tiles = new List<Tile>();
                this.mapObjects = new List<MapObject>();
                this.properties = new List<Properties>();
                this.lineSegments = new List<LineSegment>();
            }

            ~TileMapGroup()
            {
                this.tiles.Clear();
                this.tiles = null;
                this.mapObjects.Clear();
                this.mapObjects = null;
                this.properties.Clear();
                this.properties = null;
                this.lineSegments.Clear();
                this.lineSegments = null;
            }

            public class Tile
            {
                public Int32 PropertyIndex2 = Int32.MaxValue;

                public bool IsPropertyIndex2Set
                {
                    get { return PropertyIndex2 != Int32.MaxValue; }
                    private set { }
                }

                public CollisionTile collisionTile = null;

                public AppearanceTile appearanceTile = null;

                public class CollisionTile
                {
                    [Flags]
                    public enum TileFlags : uint
                    {
                        FlagFlipHorizontally = 0x80000000u,
                        FlagFlipVertically = 0x40000000u,
                        MaskFlip = 0xC0000000u,
                        MaskDamageFlags = 0x7000000u,
                        FlagGlitchable = 0x4000000u,
                        FlagTriggerable = 0x2000000u,
                        FlagDamageable = 0x1000000u,
                        FlagUnclimbable = 0x2000u,
                        FlagDamageableFringe = 0x1000u,
                        FlagCastsShadow = 0x800u,
                        FlagHandlesDamageAsNPC = 0x400u,
                        FlagLedge = 0x200u,
                        FlagStair = 0x100u,
                        FlagChainDestructs = 0x80u,
                        FlagField = 0x40u,
                        FlagSurface = 0x20u,
                        FlagAlternateRender = 0x10u,
                        FlagCollision = 0x8u,
                        FlagHidden = 0x4u,
                        MaskCollisionShape = 0x3u,
                        FlagEmptyTile = uint.MaxValue,
                        FlagNone = 0x0u
                    }

                    public enum TileCollisionShape
                    {
                        Square,
                        LineFilled,
                        Platform,
                        Invalid
                    }

                    public UInt32 CollisionFlag;
                    public Int32 LineSegmentIndex3 = Int32.MaxValue; // uninitialized 

                    public bool IsLineSegmentIndex3Set { 
                        get { return LineSegmentIndex3 != Int32.MaxValue; }
                        private set { }
                    }
                }

                public class AppearanceTile
                {
                    public enum RenderLayer {
                        PrePreFG = 1,
                        PreFG = 2,
                        PreFGWater = 3,
                        FG = 4,
                        BGProps = 8,
                        BG = 9,
                        BGWater = 10
                    }
                    public Dictionary<RenderLayer, UInt32> TileAppearanceIndex = null;

                    public AppearanceTile()
                    {
                        this.TileAppearanceIndex = new Dictionary<RenderLayer, UInt32>();
                    }

                    ~AppearanceTile()
                    {
                        this.TileAppearanceIndex.Clear();
                        this.TileAppearanceIndex = null;
                    }
                }

                public Tile()
                {
                    this.collisionTile = new CollisionTile();
                    this.appearanceTile = new AppearanceTile();
                }

                ~Tile()
                {
                    this.collisionTile = null;
                    this.appearanceTile = null;
                }
            }

            public class MapObject
            {
                public string mapObjectGroupName;
                public MapObjectGroupType mapObjectGroupType;
                public string name;
                public UInt32 id;
                public Single boundsPixelX;
                public Single boundsPixelY;
                public Single boundsPixelWidth;
                public Single boundsPixelHeight;
                public bool flipH;
                public bool flipV;
                public Single rotation;
                public List<Tuple<Single,Single>> vertices = null;
                public Dictionary<string, string> valuePairs = null;

                public MapObject()
                {
                    this.vertices = new List<Tuple<Single, Single>>();
                    this.valuePairs = new Dictionary<string, string>();
                }

                ~MapObject()
                {
                    this.vertices.Clear();
                    this.vertices = null;
                    this.valuePairs.Clear();
                    this.valuePairs = null;
                }

                public string PropertiesToString()
                {
                    string result = $"Type: {mapObjectGroupType.ToString()}";
                    foreach (KeyValuePair<string, string> pair in this.valuePairs)
                    {
                        result += $"\nK: {pair.Key}, V: {pair.Value}";
                    }
                    return result;
                }

                public void PropertiesFromString(string propertiesText)
                {
                    valuePairs.Clear();
                    if (propertiesText == "None") return;
                    string[] propertyTextList = propertiesText.Split("\n");
                    foreach (string line in propertyTextList)
                    {
                        if (propertyTextList.First() == line)
                        {
                            this.mapObjectGroupType = (MapObjectGroupType) Enum.Parse(typeof(MapObjectGroupType), line.Replace("Type: ", ""));
                        }
                        else
                        {
                            var search = Regex.Match(line, @"K: (.*), V: (.*)").Groups;
                            valuePairs.Add(search[1].Value, search[2].Value);
                        }
                    }
                }

                public string VerticesToString()
                {
                    string result = $"";
                    bool first = true;
                    foreach (Tuple<Single, Single> vertex in this.vertices)
                    {
                        result += first ? "" : "\n";
                        result += $"X: {vertex.Item1}, Y: {vertex.Item2}";
                        first = false;
                    }
                    return result;
                }

                public void VerticesFromString(string verticesText)
                {
                    vertices.Clear();
                    if (verticesText == "None") return;
                    string[] verticesTextList = verticesText.Split("\n");
                    foreach (string line in verticesTextList)
                    {
                        GroupCollection search = Regex.Match(line, @"X: (.*),Y: (.*)").Groups;
                        vertices.Add(new Tuple<Single, Single>(Single.Parse(search[1].Value), Single.Parse(search[2].Value)));
                    }
                }
            }

            public class Properties
            {
                
                public MapObjectGroupType mapObjectGroupType;
                public Dictionary<string, string> valuePairs = null;
                public Properties()
                {
                    this.valuePairs = new Dictionary<string, string>();
                }

                ~Properties()
                {
                    this.valuePairs.Clear();
                    this.valuePairs = null;
                }

                public override string ToString()
                {
                    string result = $"Type: {mapObjectGroupType.ToString()}\n";
                    foreach(KeyValuePair<string, string> pair in this.valuePairs)
                    {
                        result += $"K: {pair.Key}, V: {pair.Value}\n";
                    }
                    return result;
                }
            }

            public class LineSegment
            {
                public Tuple<Single, Single> point1 = null;
                public Tuple<Single, Single> point2 = null;
                public LineSegment(Single p1x, Single p1y, Single p2x, Single p2y)
                {
                    this.point1 = new Tuple<Single, Single>(p1x, p1y);
                    this.point2 = new Tuple<Single, Single>(p2x, p2y);
                }

                ~LineSegment()
                {
                    this.point1 = null;
                    this.point2 = null;
                }

                public override string ToString()
                {
                    string result = $"X1: {point1.Item1}, Y1: {point1.Item2}\nX2: {point2.Item1}, Y2: {point2.Item2}";
                    return result;
                }
            }

        }


        public WorldDatastructur(BinaryReader reader)
        {
            this.tileMapGroups = new Dictionary<string, TileMapGroup>();

            // FileHeader
            fileHeader = reader.ReadBytes(0x5C);
            reader.ReadInt32(); // File Length

            // Header
            WorldName = reader.ReadString();
            WidthTiles = reader.ReadInt32();
            HeightTiles = reader.ReadInt32();
            TileSetWidth = reader.ReadInt32();
            TileSetHeight = reader.ReadInt32();
            Int32 numberOfNext2 = reader.ReadInt32();

            // TileMapGroups
            for (int i = 0; i < numberOfNext2; i++) {

                // Header
                string tileMapGroupName = reader.ReadString();
                tileMapGroups.Add(tileMapGroupName, new TileMapGroup());
                for (int j = 0; j < 4; j++)
                {
                    reader.ReadInt32(); // Same as global Header
                }

                // Tile Collision Flag
                for (int j = 0; j < WidthTiles * HeightTiles; j++)
                {
                    TileMapGroup.Tile tempTile = new TileMapGroup.Tile();
                    tempTile.collisionTile.CollisionFlag = reader.ReadUInt32();
                    
                    tileMapGroups[tileMapGroupName].tiles.Add(tempTile);
                    
                }

                // Tile Appearance
                Int32 numberOfNext3 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext3; j++)
                {
                    UInt32 renderLayer = reader.ReadUInt32();
                    reader.ReadUInt32(); // RenderType always 1/alpha
                    Int32 numberOfNext = reader.ReadInt32();

                    // Render Tile
                    for (int k = 0; k < numberOfNext; k++)
                    {
                        tileMapGroups[tileMapGroupName].tiles[k].appearanceTile.TileAppearanceIndex.Add((TileMapGroup.Tile.AppearanceTile.RenderLayer)renderLayer, reader.ReadUInt32());
                    }
                }

                // MapObjectGroup
                Int32 numberOfNext4 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext4; j++)
                {
                    string mapObjectGroupName = reader.ReadString();
                    Int32 numberOfNext = reader.ReadInt32();

                    // MapObject
                    for (int k = 0; k < numberOfNext; k++)
                    {
                        TileMapGroup.MapObject tempMapObject = new TileMapGroup.MapObject();
                        tempMapObject.mapObjectGroupName = mapObjectGroupName;
                        tempMapObject.name = reader.ReadString();
                        tempMapObject.id = reader.ReadUInt32();
                        tempMapObject.boundsPixelX = reader.ReadSingle();
                        tempMapObject.boundsPixelY = reader.ReadSingle();
                        tempMapObject.boundsPixelWidth = reader.ReadSingle();
                        tempMapObject.boundsPixelHeight = reader.ReadSingle();
                        tempMapObject.flipH = reader.ReadBoolean();
                        tempMapObject.flipV = reader.ReadBoolean();
                        tempMapObject.rotation = reader.ReadSingle();
                        Int32 numberOfNextVertices = reader.ReadInt32();

                        // Vertices for enemy pathfinding
                        for (int l = 0; l < numberOfNextVertices; l++)
                        {
                            tempMapObject.vertices.Add(new Tuple<Single, Single>(reader.ReadSingle(), reader.ReadSingle()));
                        }

                        // MapObjectProperties
                        tempMapObject.mapObjectGroupType = (TileMapGroup.MapObjectGroupType)reader.ReadUInt32();
                        Int32 numberOfNextProperties = reader.ReadInt32();
                        for (int l = 0; l < numberOfNextProperties; l++)
                        {
                            tempMapObject.valuePairs.Add(reader.ReadString(), reader.ReadString());
                        }

                        tileMapGroups[tileMapGroupName].mapObjects.Add(tempMapObject);
                    }
                }

                // PropertiesList
                Int32 numberOfNext5 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext5; j++)
                {
                    // Properties
                    TileMapGroup.Properties tempProperties = new TileMapGroup.Properties();
                    tempProperties.mapObjectGroupType = (TileMapGroup.MapObjectGroupType)reader.ReadUInt32();
                    Int32 numberOfNextProperties = reader.ReadInt32();

                    // Add Key-Values Pairs
                    for (int k = 0; k < numberOfNextProperties; k++)
                    {
                        tempProperties.valuePairs.Add(reader.ReadString(), reader.ReadString());
                    }

                    tileMapGroups[tileMapGroupName].properties.Add(tempProperties);
                }

                // TileProperties
                Int32 numberOfNext6 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext6; j++)
                {
                    tileMapGroups[tileMapGroupName].tiles[reader.ReadInt32()].PropertyIndex2 = reader.ReadInt32();
                }

                // LineSegmentList
                Int32 numberOfNext7 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext7; j++)
                {
                    tileMapGroups[tileMapGroupName].lineSegments.Add(new TileMapGroup.LineSegment(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }

                // CollisionTileLineSegment
                Int32 numberOfNext9 = reader.ReadInt32();
                for (int j = 0; j < numberOfNext9; j++)
                {
                    tileMapGroups[tileMapGroupName].tiles[reader.ReadInt32()].collisionTile.LineSegmentIndex3 = reader.ReadInt32();
                }
            }

            Debug.WriteLine("WorldDatastructur read");
        }

        ~WorldDatastructur()
        {
            this.fileHeader = null;
            this.tileMapGroups.Clear();
            this.tileMapGroups = null;
            Debug.WriteLine("WorldDatastructur disposed");
        }

        public void Write(BinaryWriter binaryWriter)
        {
            // Constants
            const UInt32 RENDER_TYPE_ALPHA = 1;

            // FileHeader
            binaryWriter.Write(fileHeader);

            // Header
            binaryWriter.Write(0); // Length of File // Placeholder inserted after all other
            binaryWriter.Write(WorldName);
            binaryWriter.Write(WidthTiles);
            binaryWriter.Write(HeightTiles);
            binaryWriter.Write(TileSetWidth);
            binaryWriter.Write(TileSetHeight);
            Int32 numberOfNext2 = tileMapGroups.Count();
            binaryWriter.Write(numberOfNext2);

            // TileMapGroups
            foreach (KeyValuePair<string, TileMapGroup> tileMapGroup in tileMapGroups)
            {

                // Header
                binaryWriter.Write(tileMapGroup.Key);
                binaryWriter.Write(WidthTiles);
                binaryWriter.Write(HeightTiles);
                binaryWriter.Write(TileSetWidth);
                binaryWriter.Write(TileSetHeight);

                // Tile Collision Flag
                for (int i = 0; i < WidthTiles * HeightTiles; i++)
                {
                    binaryWriter.Write(tileMapGroup.Value.tiles[i].collisionTile.CollisionFlag);
                }

                // Tile Appearance
                HashSet<TileMapGroup.Tile.AppearanceTile.RenderLayer> renderLayers = calculateTileAppearanceLength(tileMapGroup.Value);
                Int32 numberOfNext3 = renderLayers.Count();
                binaryWriter.Write(numberOfNext3);
                foreach (TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in renderLayers)
                {
                    binaryWriter.Write((UInt32)renderLayer);
                    binaryWriter.Write(RENDER_TYPE_ALPHA);
                    Int32 numberOfNext = WidthTiles * HeightTiles;
                    binaryWriter.Write(numberOfNext);
                    
                    for (int i = 0; i < numberOfNext; i++)
                    {
                        if (!tileMapGroup.Value.tiles[i].appearanceTile.TileAppearanceIndex.ContainsKey(renderLayer))
                        {
                            binaryWriter.Write(uint.MaxValue);
                        }
                        else
                        {
                            UInt32 tileAppearanceIndex = tileMapGroup.Value.tiles[i].appearanceTile.TileAppearanceIndex[renderLayer];
                            binaryWriter.Write(tileAppearanceIndex);
                        }
                    }
                }

                // MapObjectGroupList
                Dictionary<string, Int32> mapObjectLengthDict = calculateMapObjectGroupLength(tileMapGroup.Value);
                Int32 numberOfNext4 = mapObjectLengthDict.Count();
                binaryWriter.Write(numberOfNext4);
                foreach(KeyValuePair<string, Int32> mapObjectGroupLength in mapObjectLengthDict)
                {

                    // MapObjectGroup
                    binaryWriter.Write(mapObjectGroupLength.Key);
                    Int32 numberOfNext = mapObjectGroupLength.Value;
                    binaryWriter.Write(numberOfNext);

                    foreach(TileMapGroup.MapObject mapObject in tileMapGroup.Value.mapObjects)
                    {

                        // MapObject
                        if (mapObject.mapObjectGroupName != mapObjectGroupLength.Key) continue;
                        binaryWriter.Write(mapObject.name);
                        binaryWriter.Write(mapObject.id);
                        binaryWriter.Write(mapObject.boundsPixelX);
                        binaryWriter.Write(mapObject.boundsPixelY);
                        binaryWriter.Write(mapObject.boundsPixelWidth);
                        binaryWriter.Write(mapObject.boundsPixelHeight);
                        binaryWriter.Write(mapObject.flipH);
                        binaryWriter.Write(mapObject.flipV);
                        binaryWriter.Write(mapObject.rotation);

                        // Vertices
                        Int32 numberOfVertices = mapObject.vertices.Count();
                        binaryWriter.Write(numberOfVertices);
                        foreach(Tuple<Single, Single> vertex in mapObject.vertices)
                        {
                            binaryWriter.Write(vertex.Item1);
                            binaryWriter.Write(vertex.Item2);
                        }

                        // Properties
                        binaryWriter.Write((UInt32)mapObject.mapObjectGroupType);
                        Int32 numberOfValuePairs = mapObject.valuePairs.Count();
                        binaryWriter.Write(numberOfValuePairs);
                        foreach(KeyValuePair<string, string> valuePair in mapObject.valuePairs)
                        {
                            binaryWriter.Write(valuePair.Key);
                            binaryWriter.Write(valuePair.Value);
                        }

                    }

                }

                // PropertiesList
                Int32 numberOfNext5 = tileMapGroup.Value.properties.Count();
                binaryWriter.Write(numberOfNext5);
                foreach (TileMapGroup.Properties property in tileMapGroup.Value.properties)
                {
                    binaryWriter.Write((UInt32)(property.mapObjectGroupType));
                    binaryWriter.Write(property.valuePairs.Count());

                    // Key-Value Pairs
                    foreach (KeyValuePair<string, string> valuePair in property.valuePairs)
                    {
                        binaryWriter.Write(valuePair.Key);
                        binaryWriter.Write(valuePair.Value);
                    }

                }

                // TileProperties
                Int32 numberOfNext6 = calculateTilePropertiesLength(tileMapGroup.Value);
                binaryWriter.Write(numberOfNext6);
                for(int i = 0; i < WidthTiles * HeightTiles; i++)
                {
                    if (!tileMapGroup.Value.tiles[i].IsPropertyIndex2Set) continue;
                    binaryWriter.Write(i);
                    binaryWriter.Write(tileMapGroup.Value.tiles[i].PropertyIndex2);
                }

                // LineSegmentList
                Int32 numberOfNext7 = tileMapGroup.Value.lineSegments.Count();
                binaryWriter.Write(numberOfNext7);
                foreach(TileMapGroup.LineSegment lineSegment in tileMapGroup.Value.lineSegments)
                {
                    binaryWriter.Write(lineSegment.point1.Item1);
                    binaryWriter.Write(lineSegment.point1.Item2);
                    binaryWriter.Write(lineSegment.point2.Item1);
                    binaryWriter.Write(lineSegment.point2.Item2);
                }

                // CollisionTileLineSegment
                Int32 numberOfNext9 = calculateCollisionTileLineSegmentLength(tileMapGroup.Value);
                binaryWriter.Write(numberOfNext9);
                for (int i = 0; i < WidthTiles * HeightTiles; i++)
                {
                    if (!tileMapGroup.Value.tiles[i].collisionTile.IsLineSegmentIndex3Set) continue;
                    binaryWriter.Write(i);
                    binaryWriter.Write(tileMapGroup.Value.tiles[i].collisionTile.LineSegmentIndex3);
                }

            }

            // Set fileLength postpone
            Int32 fileLength = (Int32)binaryWriter.BaseStream.Length;
            binaryWriter.Seek(0x5C, SeekOrigin.Begin);
            binaryWriter.Write(fileLength);

            Debug.WriteLine("WorldDatastructur written");

        }



        // HELPER
        private HashSet<TileMapGroup.Tile.AppearanceTile.RenderLayer> calculateTileAppearanceLength(TileMapGroup tileMapGroup)
        {
            HashSet<TileMapGroup.Tile.AppearanceTile.RenderLayer> tempSet = new HashSet<TileMapGroup.Tile.AppearanceTile.RenderLayer>();
            foreach(TileMapGroup.Tile tile in tileMapGroup.tiles)
            {
                foreach(TileMapGroup.Tile.AppearanceTile.RenderLayer renderLayer in tile.appearanceTile.TileAppearanceIndex.Keys)
                {
                    tempSet.Add(renderLayer);
                }
            }
            return tempSet;
        }

        private Dictionary<string, Int32> calculateMapObjectGroupLength(TileMapGroup tileMapGroup)
        {
            Dictionary<string, Int32> tempDict = new Dictionary<string, int>();
            foreach(TileMapGroup.MapObject mapObject in tileMapGroup.mapObjects)
            {
                if (tempDict.ContainsKey(mapObject.mapObjectGroupName))
                {
                    tempDict[mapObject.mapObjectGroupName]++;
                }
                else
                {
                    tempDict.Add(mapObject.mapObjectGroupName, 1);
                }
            }
            return tempDict;
        }

        private Int32 calculateTilePropertiesLength(TileMapGroup tileMapGroup)
        {
            Int32 length = 0;
            foreach(TileMapGroup.Tile tile in tileMapGroup.tiles)
            {
                if (tile.IsPropertyIndex2Set) length++;
            }
            return length;
        }

        private Int32 calculateCollisionTileLineSegmentLength(TileMapGroup tileMapGroup)
        {
            Int32 length = 0;
            foreach (TileMapGroup.Tile tile in tileMapGroup.tiles)
            {
                if (tile.collisionTile.IsLineSegmentIndex3Set) length++;
            }
            return length;
        }
    }
}
