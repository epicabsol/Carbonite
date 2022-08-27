/**
 * @file carbonite.h
 * @brief Carbonite C++ library.
 * @version 1.0
 * 
 * This is a single-header-file library for loading Carbonite Images.
 * 
 * To use it, do this in *one* C++ file:
 * 
 *     #define CARBONITE_IMPLEMENTATION
 *     #include <carbonite/carbonite.h>
 * 
 * Then, you can use the `CarboniteUnfreezeImage` function to load an image,
 * so you can use the loaded objects in its `RootObjectTable`.
 * 
 * When authoring your object types to load with Carbonite, use the CarboniteArray
 * and CarboniteString structures to represent your arrays and strings to ensure
 * that they line up with what the freezing process produced. This also lets you
 * use the provided `carbonite.natvis` visualizer out of the box.
 * 
 */

#pragma once

#include <cstdint>
#include <cstddef>

/**
 * @brief The value that valid CarboniteImage objects have for their CarboniteImage::Magic member.
 * 
 * ('CRBN' little-endian)
 */
#define CARBONITE_IMAGE_MAGIC ('NBRC')

/**
 * @brief The payload format version that specifies that images should be loaded regardless of version mismatches.
 */
#define CARBONITE_ACCEPT_ANY_PAYLOAD_FORMAT_VERSION (0)

// Carbonite assumes that images are being loaded on a 64-bit platform
static_assert(sizeof(void*) == sizeof(uint64_t), "Carbonite Images are built to be loaded on 64-bit platforms.");

/**
 * @brief A fixed-size array of constant values loaded from a Carbonite Image.
 * 
 * @tparam T The type of the elements in the array.
 */
template <typename T>
struct CarboniteArray {

    /**
     * @brief The number of elements in this array.
     */
    size_t Count;

    /**
     * @brief A pointer to the first element in this array.
     * 
     * If the Count of this array is zero, Elements will be `nullptr` (unless this was frozen as a string - see CarboniteString for details).
     */
    const T* Elements;

    /**
     * @brief Returns a reference to the element at the given index.
     * 
     * No bounds checking is done on the given index.
     * 
     * @param index The index of the element to return a reference to.
     * 
     * @return A constant reference to the element.
     */
    inline const T& operator[](size_t index) const {
        return this->Elements[index];
    }
};

/**
 * @brief A fixed-length read-only string of UTF8-encoded text.
 * 
 * The freezing process of CarboniteString differs from that of CarboniteArray in a few ways:
 *  - The Count member is the number of bytes, not the number of code points.
 *  - A null byte is appended after the bytes of the string, although this null byte is not reflected in the Count member.
 *    This means that Elements is always a valid null-terminated C string.
 *  - Because Elements is guaranteed to be a valid null-terminated string, a zero-length CarboniteString's Elements member is not
 *    nullptr - it is instead a valid pointer to a null terminator byte.
 */
typedef CarboniteArray<char> CarboniteString;

/**
 * @brief A Carbonite Image that has been unfrozen.
 * 
 * Access to the root objects in the image can be found in the RootObjectTable array.
 */
struct CarboniteImage {

    /**
     * @brief The magic value identifying a valid Carbonite Image. This must have the value CARBONITE_IMAGE_MAGIC.
     */
    uint32_t Magic;

    /**
     * @brief The payload format version that this image was frozen with.
     */
    uint32_t PayloadFormatVersion;

    /**
     * @brief An array of all the pointers inside the objects in this image.
     */
    CarboniteArray<void*> PointerTable;

    /**
     * @brief An array of all the root objects in this image.
     */
    CarboniteArray<void*> RootObjectTable;
};

/**
 * @brief Unfreezes a Carbonite Image from the given frozen image data.
 * 
 * @param imageData The bytes of the frozen Carbonite Image.
 * @param expectedPayloadFormatVersion The payload format version that the given image must match, or 
 * CARBONITE_ACCEPT_ANY_PAYLOAD_FORMAT_VERSION to accepy any version.
 * 
 * @return A pointer to the unfrozen image, which lives inside the given imageData buffer,
 * or nullptr if the image could not be loaded (due to a payload format version mismatch, incorrect magic value, etc.)
 */
const CarboniteImage* CarboniteUnfreezeImage(char* imageData, uint32_t expectedPayloadFormatVersion);

#ifdef CARBONITE_IMPLEMENTATION

const CarboniteImage* CarboniteUnfreezeImage(char* imageData, uint32_t expectedPayloadFormatVersion) {
    CarboniteImage* header = (CarboniteImage*)imageData;

    // If the magic number in the header is incorrect or the payload format version of the image does not match the expected version, decline to unfreeze the image
    if (header->Magic != CARBONITE_IMAGE_MAGIC
        || (header->PayloadFormatVersion != expectedPayloadFormatVersion && expectedPayloadFormatVersion != CARBONITE_ACCEPT_ANY_PAYLOAD_FORMAT_VERSION)) {
        return nullptr;
    }

    // Convert the relative offsets in the header tables into actual pointers
    header->RootObjectTable.Elements = (void* const*)(imageData + (size_t)header->RootObjectTable.Elements);
    char** rootObjects = (char**)header->RootObjectTable.Elements;
    for (size_t i = 0; i < header->RootObjectTable.Count; i++) {
        rootObjects[i] += (size_t)imageData;
    }
    header->PointerTable.Elements = (void* const*)(imageData + (size_t)header->PointerTable.Elements);
    char** pointers = (char**)header->PointerTable.Elements;
    for (size_t i = 0; i < header->PointerTable.Count; i++) {
        pointers[i] += (size_t)imageData;
    }

    // Convert each of the pointers pointed to by the pointer table from offsets into actual pointers
    for (size_t i = 0; i < header->PointerTable.Count; i++) {
        char** pointer = (char**)header->PointerTable.Elements[i];
        *pointer += (size_t)imageData;
    }

    return header;
}

#endif
