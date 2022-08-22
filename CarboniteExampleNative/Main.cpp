/**
 * @file Main.cpp
 * @brief Demonstrates how to unfreeze a Carbonite Image and read the objects contained within it.
 */

// In a project with more than one cpp file, make sure that this is only defined in a single one of them.
#define CARBONITE_IMPLEMENTATION
#include <carbonite/carbonite.h>

#include <cstdio>

#include "ExampleTypes.h"


// The payload format version is used to make sure that the Carbonite Image that is being unfrozen was
// frozen with data model types that match the application doing the unfreezing.
#define PAYLOAD_FORMAT_VERSION (1)


/**
 * @brief Prints the data in the given example model to stdout.
 * 
 * @param model The model to print the data of.
 */
void PrintExampleModel(const ExampleModel* model) {
    printf("    LODs[%d]:\n", (int)model->LODs.Count);
    
    for (size_t lod = 0; lod < model->LODs.Count; lod++) {
        printf("      LOD %d:\n", (int)lod);
        printf("        Sections[%d]:\n", (int)model->LODs[lod].Sections.Count);
        
        for (size_t section = 0; section < model->LODs[lod].Sections.Count; section++) {
            printf("          Section %d:\n", (int)section);
            printf("            Vertices[%d]:\n", (int)model->LODs[lod].Sections[section].Vertices.Count);
            
            for (size_t vertex = 0; vertex < model->LODs[lod].Sections[section].Vertices.Count; vertex++) {
                const ExampleModelVertex& vertexData = model->LODs[lod].Sections[section].Vertices[vertex];
                printf("              Vertex %d: Position = (%f, %f, %f), Normal = (%f, %f, %f), TexCoord = (%f, %f)\n", (int)vertex,
                    vertexData.Position.X, vertexData.Position.Y, vertexData.Position.Z,
                    vertexData.Normal.X, vertexData.Normal.Y, vertexData.Normal.Z,
                    vertexData.TexCoord.X, vertexData.TexCoord.Y);
            }

            printf("            Indices[%d]:\n", (int)model->LODs[lod].Sections[section].Indices.Count);
            
            for (size_t index = 0; index < model->LODs[lod].Sections[section].Indices.Count; index++) {
                printf("              Index %d: %d\n", (int)index, (int)model->LODs[lod].Sections[section].Indices[index]);
            }

            // Because freezing appends an extra zero byte at the end of a string's character bytes, we can directly use the character array
            // as a null-terminated C string.
            printf("            MaterialName: \"%s\"\n", model->LODs[lod].Sections[section].MaterialName.Elements);
        }
    }

    printf("    Bounds: Center = (%f, %f, %f), HalfExtents = (%f, %f, %f)\n",
        model->Bounds.Center.X, model->Bounds.Center.Y, model->Bounds.Center.Z,
        model->Bounds.HalfExtents.X, model->Bounds.HalfExtents.Y, model->Bounds.HalfExtents.Z);
}

int main(int argc, const char** argv) {

    // Print the usage info and exit if no image filename was given
    if (argc != 2) {
        printf("Usage: CarboniteExampleNative <image>");
        return 1;
    }

    // Load the data from the image file into a mutable char array
    FILE* file = fopen(argv[1], "rb");
    if (file == nullptr) {
        printf("ERROR: Could not open image file.");
        return 2;
    }
    fseek(file, 0, SEEK_END);
    long fileLength = ftell(file);
    fseek(file, 0, SEEK_SET);
    char* imageBytes = new char[fileLength];
    fread(imageBytes, 1, fileLength, file);
    fclose(file);

    // Unfreeze the image
    // If the payload format version of the image does not match the version we ask for here, unfreezing will
    // fail and return nullptr.
    const CarboniteImage* image = CarboniteUnfreezeImage(imageBytes, PAYLOAD_FORMAT_VERSION);
    if (image == nullptr) {
        printf("ERROR: Could not unfreeze image.");
        delete[] imageBytes;
        return 3;
    }
    else {
        // Read the data from the image
        // Because the image writer laid out the data matching the native structures, we can cast the root 
        // objects to their native type.
        printf("Root Objects[%d]:\n", (int)image->RootObjectTable.Count);
        for (size_t i = 0; i < image->RootObjectTable.Count; i++) {
            printf("  Model %d:\n", (int)i);
            PrintExampleModel((const ExampleModel*)image->RootObjectTable[i]);
        }

        // The CarboniteImage and objects contained within are all in the buffer passed to CarboniteUnfreezeImage.
        // So, make sure to not free that buffer until you are done accessing the image.
        delete[] imageBytes;
        return 0;
    }
}