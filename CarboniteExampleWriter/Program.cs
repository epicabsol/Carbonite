using System.IO;

namespace CarboniteExampleWriter
{
    internal class Program
    {
        // Where to write the example output image.
        // There's no point standardizing on a file extension - it's expected that Carbonite
        // Images will end up being compressed and/or packaged together, and the frozen objects
        // are only useable by applications designed for those specific images.
        const string OutputFilename = "ExampleCarboniteImage.img";

        // This is version 1 of our types we are freezing.
        // We change this each time a breaking change to the types we are freezing is made,
        // so that the resulting image will not be loaded by native applications expecting a
        // different version of the frozen types.
        const int PayloadFormatVersion = 1;

        static void Main(string[] args)
        {
            // Initialize dummy example data to freeze.
            // This isn't sensible model data - it's just arbitrary gibberish.
            ExampleModel exampleModel = new ExampleModel()
            {
                LODs = new ExampleModelLOD[]
                {
                    new ExampleModelLOD()
                    {
                        Sections = new ExampleModelSection[]
                        {
                            new ExampleModelSection()
                            {
                                Vertices = new ExampleModelVertex[]
                                {
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.5f, 1.4f, 2.4f),
                                        Normal = new Vector3(0.5f, 0.2f, -0.5f),
                                        TexCoord = new Vector2(0.0f, 1.0f),
                                    },
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.3f, 3.5f, 0.85f),
                                        Normal = new Vector3(-0.6f, 0.2f, 0.0f),
                                        TexCoord = new Vector2(1.0f, 0.333f),
                                    },
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.25f, 0.31f, 0.821f),
                                        Normal = new Vector3(0.0f, 1.0f, 0.0f),
                                        TexCoord = new Vector2(0.5f, 1.0f),
                                    },
                                },
                                Indices = new uint[]
                                {
                                    0, 1, 2,
                                },
                                MaterialName = "Material1",
                            },
                        },
                    },
                    new ExampleModelLOD()
                    {
                        Sections = new ExampleModelSection[]
                        {
                            new ExampleModelSection()
                            {
                                Vertices = new ExampleModelVertex[]
                                {
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.52f, 0.81f, 0.17f),
                                        Normal = new Vector3(0.0f, 0.0f, -1.0f),
                                        TexCoord = new Vector2(1.0f, 0.33f),
                                    },
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.91f, 0.74f, -0.83f),
                                        Normal = new Vector3(0.05f, 0.81f, 1.2f),
                                        TexCoord = new Vector2(0.05f, 0.83f),
                                    },
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.19f, 0.83f, 0.2389f),
                                        Normal = new Vector3(0.28f, 0.38f, 0.81f),
                                        TexCoord = new Vector2(0.5f, 0.7f),
                                    },
                                    new ExampleModelVertex()
                                    {
                                        Position = new Vector3(0.82f, 0.27f, 0.16f),
                                        Normal = new Vector3(0.0f, -1.0f, 0.0f),
                                        TexCoord = new Vector2(0.8f, 0.224f),
                                    },
                                },
                                Indices = new uint[]
                                {
                                    0, 1, 2,
                                    1, 2, 3,
                                },
                                MaterialName = "Material McMaterialface",
                            },
                        },
                    },
                },
                Bounds = new AxisAlignedBox()
                {
                    Center = new Vector3(0.5f, 0.5f, 0.5f),
                    HalfExtents = new Vector3(2.5f, 1.5f, 3.0f),
                },
            };

            // Open a file stream and create a Carbonite Image writer.
            // Note that the image is only really written when the writer is disposed.
            using (FileStream stream = new FileStream(Program.OutputFilename, FileMode.Create))
            using (Carbonite.CarboniteImageWriter imageWriter = new Carbonite.CarboniteImageWriter(stream, Program.PayloadFormatVersion))
            {
                // Write a single root object to the image.
                // Root objects are accessible by index when reading the image, making
                // this root object 0.
                imageWriter.WriteRootObject(exampleModel);
            }
        }
    }
}