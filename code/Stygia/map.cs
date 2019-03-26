using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stygia.Entity;
using Stygia;
using libtcod;

namespace Stygia.Space
{
    public enum MapType { None = 0, Cave = 1, Dungeon = 2 };
    public enum TerrainType { HardRock = 0, SoftRock = 1, RoomFloor = 2, CorridorFloor = 3, Elemental = 4, OpenDoor = 5, 
        ClosedDoor = 6, StairsDown = 7, ElementalGate = 8};
    public enum Visiblity { None = 0, Previously = 1, Visible = 2, NotVisible =3};

    [Serializable]
    class MapViewPort
    {
        // Standard Default Constructor
        public MapViewPort()
        {
            topLeft.Set(0, 0);
            topRight.Set(0, 0);
            bottomLeft.Set(0, 0);
            bottomRight.Set(0, 0);
        }

        // Other Constructor
        public MapViewPort(int TopLeftX, int TopLeftY, int TopRightX, int TopRightY,
            int BottomLeftX, int BottomLeftY, int BottomRightX, int BottomRightY)
        {
            topLeft = new Point(TopLeftX, TopLeftY);
            topRight = new Point(TopRightX, TopRightY);
            bottomLeft = new Point(BottomLeftX, BottomLeftY);
            bottomRight = new Point(BottomRightX, BottomRightY);
        }
        public MapViewPort(int CentreX, int CentreY, int Width, int Height)
        {
            topLeft = new Point(CentreX - (Width / 2), CentreY - (Height/2));
            topRight = new Point(CentreX + (Width / 2), CentreY - (Height / 2));
            bottomLeft = new Point(CentreX - (Width / 2), CentreY + (Height / 2));
            bottomRight = new Point(CentreX + (Width / 2), CentreY + (Height / 2));
        }
        public void Add(Point offset)
        {
            topLeft.Add(offset);
            topRight.Add(offset);
            bottomLeft.Add(offset);
            bottomRight.Add(offset);
        }

        // Points
        private Point topLeft;
        private Point topRight;
        private Point bottomLeft;
        private Point bottomRight;

        // Properties
        public Point TopLeft
        {
            get { return topLeft; }
            set { topLeft = value; }
        }
        public Point TopRight
        {
            get { return topRight; }
            set { topRight = value; }
        }
        public Point BottomLeft
        {
            get { return bottomLeft; }
            set { bottomLeft = value; }
        }
        public Point BottomRight
        {
            get { return bottomRight; }
            set { bottomRight = value; }
        }

        public String Value
        {
            get
            {
                StringBuilder buffer = new StringBuilder();
                buffer.Append(TopLeft.Value);
                buffer.Append(":");
                buffer.Append(TopRight.Value);
                buffer.Append(":");
                buffer.Append(BottomLeft.Value);
                buffer.Append(":");
                buffer.Append(BottomRight.Value);
                return buffer.ToString();
            }
            set
            {
                // Parse the value and extract the data
                string[] words = value.Split(':');
                topLeft.Value = words[0];
                topRight.Value = words[1];
                bottomLeft.Value = words[2];
                bottomRight.Value = words[3];
            }
            
        }
    }

    [Serializable]
    class MapCell
    {
        // Standard Default Constructor
        public MapCell()
        {
            terrain = TerrainType.HardRock;
            visible = Visiblity.None;
            visited = false;
            turnCount = 0;
            lightIntensity = 0.0f;
            ambientLight = 0.0f;
            ambientLightColour = new TCODColor();
            ambientLightColour = TCODColor.black;
            item = -1;
            creature = -1;
        }

        // Usual Constructor
        public MapCell(TerrainType terrainType, bool Visible, bool Visited)
        {           
            terrain = terrainType;
            visible = Visible ? Visiblity.Visible : Visiblity.NotVisible;          
            visited = Visited;
            turnCount = 0;
            lightIntensity = 0.0f;
            ambientLight = 0.0f;
            ambientLightColour = new TCODColor();
            ambientLightColour = TCODColor.black;
            item = -1;
            creature = -1;
        }

        public bool PlaceItem(int itemIndex)
        {
            if (item != -1)
            {
                return false;
            }
            else
            {
                item = itemIndex;
                return true;
            }
        }

        // The terrain type
        private TerrainType terrain;

        // If the terrain is visible
        private Visiblity visible;

        // Whither it has been visited by the character
        private bool visited;

        // Turn count until the tile event callback is called
        private int turnCount = 0;

        // Light intensity for lighting
        private float lightIntensity = 0.0f;

        // Ambient lighting
        private float ambientLight = 0.0f;
        private TCODColor ambientLightColour;

        // Item pointer
        private int item;

        // Creature pointer
        private int creature;
     
        // Properties
        public TerrainType Terrain
        {
            get { return terrain; }
            set { terrain = value; }
        }
        public Visiblity Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        public bool Visited
        {
            get { return visited; }
            set { visited = value; }
        }
        public int TurnCount
        {
            get { return turnCount; }
            set { turnCount = value; }
        }
        public float LightIntensity
        {
            get { return lightIntensity; }
            set { lightIntensity = value; }
        }
        public float AmbientLight
        {
            get { return ambientLight; }
            set { ambientLight = value; }
        }
        public TCODColor AmbientLightColour
        {
            get { return ambientLightColour; }
            set { ambientLightColour = value; }
        }
        public bool Walkable
        {
            get
            {
                if (terrain == TerrainType.CorridorFloor ||
                  terrain == TerrainType.OpenDoor ||
                  terrain == TerrainType.StairsDown ||
                   terrain == TerrainType.ElementalGate ||
                  terrain == TerrainType.RoomFloor)
                { return true; }
                else { return false; }
            }
        }
        public int Item
        {
            get { return item; }
            set { item = value; }
        }
        public int Creature
        {
            get { return creature; }
            set { creature = value; }
        }


        public string Value
        {
            get
            {
                float h, s, v;

                StringBuilder buffer = new StringBuilder();
                buffer.Append(terrain.ToString());
                buffer.Append(":");
                buffer.Append(visible.ToString());
                buffer.Append(":");
                buffer.Append(visited.ToString());
                buffer.Append(":");
                buffer.Append(turnCount.ToString());
                buffer.Append(":");
                buffer.Append(lightIntensity.ToString());
                buffer.Append(":");
                buffer.Append(ambientLight.ToString());
                buffer.Append(":");
                ambientLightColour.getHSV(out h, out s, out v);
                buffer.Append(h.ToString());
                buffer.Append(":");
                buffer.Append(s.ToString());
                buffer.Append(":");
                buffer.Append(v.ToString());
                buffer.Append(":");
                buffer.Append(item.ToString());
                buffer.Append(":");
                buffer.Append(creature.ToString());
                return buffer.ToString();
            }
            set 
            {
                // Parse the value and extract the data
                int index = 0;             
                string[] words = value.Split(':');
                terrain = (TerrainType)Enum.Parse(typeof(TerrainType), words[index++]);
                visible = (Visiblity)Enum.Parse(typeof(Visiblity), words[index++]);
                visited = Convert.ToBoolean(words[index++]);
                turnCount = Convert.ToInt32(words[index++]);
                lightIntensity = (float)Convert.ToDouble(words[index++]);
                ambientLight = (float)Convert.ToDouble(words[index++]);
                ambientLightColour.setHSV((float)Convert.ToDouble(words[index++]),
                                            (float)Convert.ToDouble(words[index++]),
                                            (float)Convert.ToDouble(words[index++]));
                item = Convert.ToInt32(words[index++]);
                creature = Convert.ToInt32(words[index++]);
            }
        }       
    }

    [Serializable]
    class Map
    {       
        // Default Constructor
        public Map()
        {
            Cells = new MapCell[300, 200];
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    Cells[x, y].Terrain = TerrainType.HardRock;
                    Cells[x, y].Visible = Visiblity.NotVisible;
                    Cells[x, y].Visited = false;
                }
            }
            lightSources = new List<LightSource>();          
        }

        public Map(MapType type, int Level)
        {
            Cells  = new MapCell[300, 200];
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    Cells[x, y] = new MapCell();
                    if (!(InBounds(x, y))) { Cells[x, y].Terrain = TerrainType.HardRock; }
                    else { Cells[x, y].Terrain = TerrainType.SoftRock; }
                    Cells[x, y].Visible = Visiblity.NotVisible;
                    Cells[x, y].Visited = false;
                }
            }

            // Setup the light sources
            lightSources = new List<LightSource>();

            if (type == MapType.Cave)
            {
                // Build a cave-type level
                int cellsDug;
                int randomCellToDig;
                int surroundingWalls;
                List<Point> digger = new List<Point>();

                // Initialise the level
                random = new TCODRandom(TCODRandomType.MersenneTwister);
                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        Cells[x, y] = new MapCell();           
                    }
                }
    
                do
                {
                    // Clear the digger queue
                    digger.Clear();

                    // Set up the default terrain for the level - soft, diggable rock
                    // surrounded by hard rock
                    for (int x = 0; x < 300; x++)
                    {
                        for (int y = 0; y < 200; y++)
                        {                         
                            if (InBounds(x, y))
                                { Cells[x, y].Terrain = TerrainType.SoftRock; }
                            else 
                                { Cells[x, y].Terrain = TerrainType.HardRock; }
                            Cells[x, y].Visible = Visiblity.NotVisible;
                            Cells[x, y].Visited = false;
                        }
                    }
                    
                    // Keep a count of the cells dug out
                    cellsDug = 0;

                    // Start in the middle and dig out a cell
                    Point diggerPoint = new Point(150, 100);
                    this.Cells[diggerPoint.X, diggerPoint.Y].Terrain = TerrainType.CorridorFloor;

                    // Add the cell dug out as a seed point
                    digger.Add(diggerPoint);
                    
                    // Note we've dug out a cell
                    cellsDug++;
                    
                    // Whilst we have seed cells to dig
                    while (digger.Count != 0)
                    {
                        // Get a random cell from the list of seed cells                      
                        randomCellToDig = random.getInt(0, digger.Count - 1);

                        // Use it to dig out more cells
                        diggerPoint = digger[randomCellToDig];

                        // Delete the cell from the list of seed cells now we've used it
                        digger.RemoveAt(randomCellToDig);

                        // Check if the cell is inbounds
                        if (InBounds(diggerPoint.X, diggerPoint.Y))
                        {
                            // Get the number of walls (unexcavated cells) around this square
                            surroundingWalls = GetAdjacentTerrainCount(diggerPoint.X, diggerPoint.Y, 
                                TerrainType.SoftRock);
                           
                            // Only continue to excavate if we have more than a certain number
                            // of walls surrounding this cell
                            if (surroundingWalls > 5)
                            {
                                // Dig out two cells next to an excavated cell
                                int CellDigCount = 0;

                                // Find two cells if possible
                                while (CellDigCount < 2)
                                {
                                    // Find an adjacent cell                              
                                    int newx = diggerPoint.X + random.getInt(-1, 1);
                                    int newy = diggerPoint.Y + random.getInt(-1, 1);

                                    // Check if the cell is inbounds
                                    if (InBounds(newx, newy))
                                    {
                                        // Check if the cell is undug
                                        if (this.Cells[newx, newy].Terrain == TerrainType.SoftRock)
                                        {
                                            // Add the cell to the list of seed cells
                                            Point newPoint = new Point(newx, newy);
                                            digger.Add(newPoint);

                                            // Dig the cell out
                                            this.Cells[newx, newy].Terrain = TerrainType.CorridorFloor;

                                            // Keep a track of the number of cells we've dug out
                                            CellDigCount++;
                                            cellsDug++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                } // Keep going until 500 cells have been dug out
                while (cellsDug < 500);

                // Now do some tidyup and remove most isolated unexcavated cells 
                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        if (InBounds(x, y))
                        {
                            // If we have an undug isolated cell
                            if (GetAdjacentTerrainCount(x, y, TerrainType.CorridorFloor) > 5)
                            {
                                // dig out isolated cells
                                this.Cells[x, y].Terrain = TerrainType.CorridorFloor;
                            }
                        }
                    }
                }

                // Now do some tidyup and remove most isolated unexcavated cells 
                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        if (InBounds(x, y))
                        {
                            // If we have an undug isolated cell
                            if (GetAdjacentTerrainCount(x, y, TerrainType.CorridorFloor) > 5)
                            {
                                // dig out isolated cells
                                this.Cells[x, y].Terrain = TerrainType.CorridorFloor;
                            }
                        }
                    }
                }

                // Add a stairs in a random place to all except the bottom level
                if (Level < 10)
                {
                    Point stairPos = new Point(GetRandomTerrain(TerrainType.CorridorFloor, random));
                    this.Cells[stairPos.X, stairPos.Y].Terrain = TerrainType.StairsDown;
                }
                else
                {
                    // Bottom level, add the elemental gate!
                    Point gatePos = new Point(GetRandomTerrain(TerrainType.CorridorFloor, random));
                    this.Cells[gatePos.X, gatePos.Y].Terrain = TerrainType.ElementalGate;
                }

                for (int i = 0; i < Level; i++)
                {
                    // Add random light sources to represent a fire, i.e. a safe spot on the level
                    Point lspos = new Point(GetRandomTerrain(TerrainType.CorridorFloor, random));
                    LightSource ls = new LightSource(lspos, 9, TCODColor.flame, 0.7f, this);
                    lightSources.Add(ls);
                }

            }
        }

        public void BuildFOV(TCODMap viewmap, int xpos, int ypos)
        {
            float Distance; 
            Point currentLoc = new Point(0, 0);
            Point playerLoc = new Point(xpos, ypos);
            float intensity = 0.0f;
            viewmap.clear(false, false);
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    viewmap.setProperties(x, y, this.Cells[x, y].Walkable, this.Cells[x, y].Walkable);
                }
            }
            viewmap.computeFov(xpos, ypos, 9, true, TCODFOVTypes.ShadowFov);

            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    if (this.Cells[x, y].Visible == Visiblity.Visible) { this.Cells[x, y].Visible = Visiblity.Previously; }     
                    if (viewmap.isInFov(x, y)) { this.Cells[x, y].Visible = Visiblity.Visible; }
                    this.Cells[x,y].LightIntensity = 0.0f;
                }
            }

            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    if (this.Cells[x, y].Visible == Visiblity.Visible)
                    {
                        currentLoc.Set(x, y);
                        Distance = (float) currentLoc.Dist(playerLoc);
                        intensity = 0.5f -  Distance/12;
                        if (intensity < 0.0f) { intensity = 0.0f; }
                        this.Cells[x, y].LightIntensity = intensity; 
                    }

                    if (this.Cells[x, y].Visible == Visiblity.Visible) { this.Cells[x, y].Visible = Visiblity.Previously; }
                    if (viewmap.isInFov(x, y)) { this.Cells[x, y].Visible = Visiblity.Visible; }
                }
            }
        }
  
        public bool InBounds(int x, int y)
        {
            if (x < 11 || x > 189) { return false; }
            else if (y < 11 || y > 189) { return false; }
            else { return true; }
        }

        public Point GetRandomTerrain(TerrainType terrainType, TCODRandom random)
        {
            int x = 0;
            int y = 0;
            bool Found = false;
            do
            {
                x = random.getInt(1, 299);
                y = random.getInt(1, 199);
                if (this.Cells[x, y].Terrain == terrainType) { Found = true; }
            }
            while (!Found);

            return new Point(x, y);

        }

        private int GetAdjacentTerrainCount(int x, int y, TerrainType terrain)
        {
            int surroundingTerrainCount = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (!((dx == 0) && (dy == 0)))
                    {
                        if (this.Cells[x + dx, y + dy].Terrain == terrain) 
                            { surroundingTerrainCount++; }
                    }
                }
            }
            return surroundingTerrainCount;
        }

        public string Value
        {
            get 
            {
                StringBuilder result = new StringBuilder(string.Empty);

                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        result.Append(this.Cells[x, y].Value);
                        result.Append("=");
                    }
                }

                return result.ToString();
            }
            set
            {
                // Parse the value and extract the data
                string[] words = value.Split('=');
                int index = 0;

                for (int x = 0; x < 300; x++)
                {
                    for (int y = 0; y < 200; y++)
                    {
                        Cells[x, y].Value = words[index];
                        index++;
                    }
                }
            }

        }

        // Indexer
        public MapCell this[int indexx, int indexy]
        {
            get { return Cells[indexx, indexy]; }
            set { Cells[indexx, indexy] = value; }
        }

        // Cell Structure
        public MapCell[,] Cells;

        private TCODRandom random;
        public List<LightSource> lightSources;           

        public string LightValue
        {
            get 
            {
                StringBuilder result = new StringBuilder(string.Empty);

                foreach (LightSource ls in lightSources)
                {
                    result.Append(ls.Value);
                    result.Append("=");              
                }
                return result.ToString();
            }
        }
    }

    [Serializable]
    class LightSource
    {
        public LightSource(Point Position, int Radius, TCODColor Colour, float Intensity, Map mapData)
        {
            float Distance;
            Point currentLoc = new Point(0, 0);
            float workingIntensity = 0.0f;

            position = new Point(Position);
            radius = Radius;
            colour = new TCODColor();
            colour = Colour;
            intensity = Intensity;
            int mapSize = 1 + (Radius * 2);

            lightMap = new TCODMap(300, 200);
            lightMap.clear(false, false);

            //for (int dx = Position.X - Radius; dx <= Position.X + Radius; dx++)
            for (int dx = 0; dx < 300; dx++)
            {
                //for (int dy = Position.Y - Radius; dy <= Position.Y + Radius; dy++)
                for (int dy = 0; dy < 200; dy++)
                {
                    lightMap.setProperties(dx, dy, mapData.Cells[dx, dy].Walkable, mapData.Cells[dx, dy].Walkable);
                }
            }
            lightMap.computeFov(Position.X, Position.Y, Radius, true, TCODFOVTypes.ShadowFov);

            //for (int dx = Position.X - Radius; dx < Position.X + Radius; dx++)
            for (int dx = 0; dx < 300; dx++)
            {
                //for (int dy = Position.Y - Radius; dy <= Position.Y + Radius; dy++)
                for (int dy = 0; dy < 200; dy++)
                {
                    if (lightMap.isInFov(dx, dy)) 
                    {
                        currentLoc.Set(dx, dy);
                        Distance = (float)currentLoc.Dist(Position);
                        workingIntensity = Intensity - Distance / 12;
                        if (workingIntensity < 0.0f) { workingIntensity = 0.0f; }
                        mapData.Cells[dx, dy].AmbientLight = workingIntensity;
                        mapData.Cells[dx, dy].AmbientLightColour = TCODColor.Interpolate(TCODColor.black, Colour, workingIntensity);
                    }
                }
            }
  
        }
        public LightSource()
        {
            position = new Point(0, 0);
            radius = 0;
            colour = new TCODColor(0, 0, 0);         
            intensity = 0;
            lightMap = new TCODMap(300, 200);
            lightMap.clear(false, false);
        }

        private Point position;
        private int radius;
        private TCODColor colour;
        private float intensity;

        public Point Position
        {
            get { return position; }
            set { position = value; }
        }
        public int Radius
        {
            get { return radius; }
            set { radius = value; }
        }
        public TCODColor Colour
        {
            get { return colour; }
            set { colour = value; }
        }
        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        public string Value
        {
            get 
            {
                float h, s, v;

                StringBuilder buffer = new StringBuilder();
                buffer.Append(position.Value);
                buffer.Append(":");
                buffer.Append(radius.ToString());
                buffer.Append(":");
                colour.getHSV(out h, out s, out v);
                buffer.Append(h.ToString());
                buffer.Append(":");
                buffer.Append(s.ToString());
                buffer.Append(":");
                buffer.Append(v.ToString());
                buffer.Append(":");
                buffer.Append(intensity.ToString());
               
                return buffer.ToString();      
            }
            set
            {
                // Parse the value and extract the data
                int index = 0;
                string[] words = value.Split(':');
                position.Value = words[index++];
                radius = Convert.ToInt32(words[index++]);
                colour.setHSV((float)Convert.ToDouble(words[index++]),
                                            (float)Convert.ToDouble(words[index++]),
                                            (float)Convert.ToDouble(words[index++]));
                intensity = (float)Convert.ToDouble(words[index++]);
            }
        }

        private TCODMap lightMap;
      
    }
}
