/**
 * @file ExampleTypes.h
 * @brief Declares example types for the Carbonite example to demonstrate loading with.
 */

#pragma once

// We need the CarboniteArray and CarboniteString types for arrays and strings
#include <carbonite/carbonite.h>

#include <cstdint>

/**
 * @brief A two-dimensional vector of 32-bit floating point values.
 */
struct Vector2 {
    float X;
    float Y;
};

/**
 * @brief A three-dimensional vector of 32-bit floating point values.
 */
struct Vector3 {
    float X;
    float Y;
    float Z;
};

/**
 * @brief An axis-aligned box defined by its center point and half-extents.
 */
struct AxisAlignedBox {
    Vector3 Center;
    Vector3 HalfExtents;
};

/**
 * @brief A vertex in an example model.
 */
struct ExampleModelVertex {
    Vector3 Position;
    Vector3 Normal;
    Vector2 TexCoord;
};

/**
 * @brief An individual section in an example model, comprised of a single mesh and a material name.
 */
struct ExampleModelSection {
    CarboniteArray<ExampleModelVertex> Vertices;
    CarboniteArray<uint32_t> Indices;
    CarboniteString MaterialName;
};

/**
 * @brief The geometry that should be rendered for a single level of detail of a model.
 */
struct ExampleModelLOD {
    CarboniteArray<ExampleModelSection> Sections;
};

/**
 * @brief A 3D model with geometry for various levels of detail and an axis-aligned bounding box.
 */
struct ExampleModel {
    CarboniteArray<ExampleModelLOD> LODs;
    AxisAlignedBox Bounds;
};