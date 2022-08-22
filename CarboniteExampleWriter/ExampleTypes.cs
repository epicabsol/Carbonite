using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbonite;

namespace CarboniteExampleWriter
{
    // Example data structures to be frozen into a Carbonite Image for use with CarboniteExampleNative.

    [GenerateFreezable]
    public partial struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [GenerateFreezable]
    public partial struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    [GenerateFreezable]
    public partial struct AxisAlignedBox
    {
        public Vector3 Center;
        public Vector3 HalfExtents;
    }

    [GenerateFreezable]
    public partial struct ExampleModelVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
    }

    [GenerateFreezable]
    public partial struct ExampleModelSection
    {
        public ExampleModelVertex[] Vertices;
        public uint[] Indices;
        public string MaterialName;
    }

    [GenerateFreezable]
    public partial struct ExampleModelLOD
    {
        public ExampleModelSection[] Sections;
    }

    [GenerateFreezable]
    public partial struct ExampleModel
    {
        public ExampleModelLOD[] LODs;
        public AxisAlignedBox Bounds;
    }
}
