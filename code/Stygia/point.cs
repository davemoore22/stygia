using System;
using System.Linq;
using System.Linq.Expressions;

namespace Stygia.Space
{
    // List of cardinal directions useful in offset and coordinate calculations
    enum CardinalDirection { None = -1, N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7 };
    
    // Point Class to handle Map Functions
    [Serializable]
    class Point
    {
        // Default Constructor
        public Point()
        {
            // Set the internal values
            x = 0;
            y = 0;
        }

        // Standard Constructor
        public Point(int X, int Y)
        {
            // Set the internal values
            x = (byte)X;
            y = (byte)Y;
        }

        // Limit the values of the point to an additional supplied arbitrary 
        // bounds - useful when working with calculated values
        public Point(int X, int Y, int LimitSmall, int LimitBig)
        {
            // Set the internal values
            x = (byte)X;
            y = (byte)Y;

            // Limit the x value if necessary
            if (x > LimitBig) { x = (byte)LimitBig; }
            else if (x < LimitSmall) { x = (byte)LimitSmall; }

            // Limit the y value if necessary
            if (y > LimitBig) { y = (byte)LimitBig; }
            else if (y < LimitSmall) { y = (byte)LimitSmall; }
        }

        public String Value
        {
            get
            {
                return X.ToString() + "/" + Y.ToString();
            }
            set
            {
                string[] words = value.Split('/');
                x = Convert.ToByte(words[0]);
                y = Convert.ToByte(words[1]);
            }
        }

        // Copy Constructor - we need to use this because unlike C++ we cannot 
        // overload the assignment operator
        public Point(Point P2)
        {
            // If we have a valid reference
            if ((object)P2 != null)
            {
                // Set the internal values
                x = P2.X;
                y = P2.Y;
            }           
        }

        // Overload the equality operator
        public static bool operator ==(Point P1, Point P2)
        {
            // If we have a valid reference
            if ((object)P1 == null) { return false; }
            if ((object)P2 == null) { return false; }

            // Check for full equality on both values
            return (P1.x == P2.x && P1.y == P2.y);
        }

        // Overload the non-equality operator
        public static bool operator !=(Point P1, Point P2)
        {
            // If we have a valid reference
            if ((object)P1 == null) { return false; }
            if ((object)P2 == null) { return false; }

            // Check for inequality on either values
            return (P1.x != P2.x || P1.y != P2.y);
        }

        // Overload the equals operator
        public override bool Equals(System.Object P2)
        {
            // If we have a valid reference
            if ((object)P2 == null) { return false; }

            // Check we can cast the incoming object to a Point
            Point p = P2 as Point;
            if ((System.Object)p == null) { return false; }

            // Check for full equality on both values
            return (x == p.x && y == p.y);
        }

        // Provide a custom GetHashCode function (needed when the Equals operator is
        // overridden 
        public override int GetHashCode()
        {
            // Use XOR
            return x ^ y;
        }

        // Provide an equivalent of an assignment operator
        public void Set(Point P2)
        {
            // If we have a valid reference
            if ((object)P2 != null)
            {
                // Set the internal values
                x = P2.X;
                y = P2.Y;
            }
        }

        // Provide the equivalent of an add operator
        public void Add(Point P2)
        {
            if ((object)P2 != null)
            {
                // Set the internal values
                x = (byte) (x + P2.X);
                y = (byte) (y + P2.Y);
            }
        }

        // Provide the equivalent of a subtract operator
        public void Subtract(Point P2)
        {
            if ((object)P2 != null)
            {
                // Set the internal values
                x = (byte)(x - P2.X);
                y = (byte)(y - P2.Y);
            }
        }

        // Provide another equivalent of an assignment operator
        public void Set(int X, int Y)
        {  
            // Set the internal values
            x = (byte)X;
            y = (byte)Y;
        }

        // Return the euclidean distance between two points
        public int Dist(Point P2)
        {
            // If we have a valid reference
            if ((object)P2 == null) { return -1; }

            // Return the distance (as an int, rounded down)
            return (int)Math.Sqrt((x - P2.x) * (x - P2.x) + 
                (y - P2.y) * (y - P2.y));
        }

        // Return the difference between two points
        public void Offset(Point P1, Point P2)
        {
            // Set the default values
            X = 0;
            Y = 0;

            // If we have a valid reference
            if ((object)P1 == null) { return; }
            if ((object)P2 == null) { return; }

            // Get the offsets
            X = (byte)(P1.x - P2.x);
            Y = (byte)(P1.y - P2.y);
        }

        // Return the difference between two points optionally limiting the
        // values returned
        public void Offset(Point P1, Point P2, int Min, int Max)
        {
            // Set the default values
            X = 0;
            Y = 0;

            // If we have a valid reference
            if ((object)P1 == null) { return; }
            if ((object)P2 == null) { return; }

            // Get the offsets
            X = (byte)(P1.x - P2.x);
            Y = (byte)(P1.y - P2.y);
     
            // Limit the x value if necessary
            if (X > Max) { X = (byte)Max; }
            else if (X < Min) { X = (byte)Min; }

            // Limit the y value if necessary
            if (Y > Max) { Y = (byte)Max; }
            else if (Y < Min) { Y = (byte)Min; }
        }

        // Get the direction of one point from another as an enum
        public CardinalDirection Direction(Point P1, Point P2)
        {
            // If we have a valid reference
            if ((object)P1 == null) { return CardinalDirection.None; }
            if ((object)P2 == null) { return CardinalDirection.None; }

            // Set up an offset array to convert the offsets of the two points
            // into a direction
            Point[] Directions = new Point[8];
            Directions[(int)CardinalDirection.N].Set(0, -1);
            Directions[(int)CardinalDirection.NE].Set(1, -1);
            Directions[(int)CardinalDirection.E].Set(1, 0);
            Directions[(int)CardinalDirection.SE].Set(1, 1);
            Directions[(int)CardinalDirection.S].Set(0, 1);
            Directions[(int)CardinalDirection.SW].Set(-1, 1);
            Directions[(int)CardinalDirection.W].Set(-1, 0);
            Directions[(int)CardinalDirection.NW].Set(-1, -1);
           
            // Get the offset from one point to another
            Point P = new Point();
            P.Offset(P1, P2, -1, 1);

            // Find the matching direction
            int Index = 0;
            foreach (Point Item in Directions)
            {
                if (Item == P) { return (CardinalDirection)Index; }
                else { Index++; }
            }

            // Return the null value just in case
            return CardinalDirection.None;
        }

        // Check if two points are adjacent to each other
        public bool Adjacent(Point P1, Point P2)
        {
            // Test if the points are 1 square apart
            return (this.Dist(P2) == 1);
        }

        // Private data members
        private byte x;
        private byte y;

        // Publically accessible properties
        public byte X
        {
            get { return x; }
            set { x = value; }
        }       
        public byte Y
        {
            get { return y; }
            set { y = value; }
        }


    }

}
