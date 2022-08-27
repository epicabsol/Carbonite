Carbonite
=========

![Carbonite NuGet Package](https://img.shields.io/nuget/v/Carbonite?label=Carbonite&style=flat-square) ![CarboniteFreezableCodeGen NuGet Package](https://img.shields.io/nuget/v/CarboniteFreezableCodeGen?label=CarboniteFreezableCodeGen&style=flat-square)

Carbonite is a C# library, C++ single-header library, and binary file format for storing data models that can be loaded and used from native code as quickly as possible.
The intended audience for Carbonite is games and other applications where high data loading performance is needed.

Carbonite works by taking instances of C# classes and structures and writing out their data into a contiguous block, called a Carbonite Image, that matches how your native code expects it to be laid out. This process is called **freezing** and is carried out ahead-of-time using the Carbonite C# library. Once the frozen bytes have been shipped to the end user, your application **unfreezes** the data using the Carbonite C++ header, which transforms the relative offsets in the data into valid pointers, resulting in data models in memory that can be directly consumed by your native application.

In summary:

C# Data Model => **freezing** (C# library) => Frozen Carbonite Image (frozen data) => **unfreezing** (C++ library) => Unfrozen Image (Native data objects)


Getting Started
---------------

To create frozen Carbonite Images with C#, add the [Carbonite](https://www.nuget.org/packages/Carbonite/) and [CarboniteFreezableCodeGen](https://www.nuget.org/packages/CarboniteFreezableCodeGen/) NuGet packages to your .NET 6+ project. You will need to enable preview features for your project by adding `<EnablePreviewFeatures>True</EnablePreviewFeatures>` to a `<PropertyGroup>` in your project file. You can also add the `PrivateAssets="all"` and `ExcludeAssets="compile; runtime"` attributes to the `<PackageReference>` element for `Carbonite.FreezableCodeGen`, as you do not need its assembly in your build output nor in referencing projects. You are now ready to freeze some images!

To load frozen Carbonite Images with C++, you will probably want to use the `carbonite.h` single-header library (in the `/Include/carbonite/` directory of this repo). Like other single-header libraries, make sure to define the `CARBONITE_IMPLEMENTATION` macro in exactly one of your source files before `carbonite.h` is included. Personally, for non-trivial projects I like adding an individual `.cpp` file (with only those two lines) for single-header libraries like this one.

If you are using Visual Studio or Visual Studio Code, you can load the provided `carbonite.natvis` for enhanced visualization of `CarboniteArray` and `CarboniteString` variables. You are now ready to unfreeze some images!


Freezing
--------

The freezing process is handled by the Carbonite C# library. To create a frozen Carbonite Image, create a `CarboniteImageWriter` and use the `WriteRootObject` method to freeze your data objects to a `Stream`. (Note that the destination `Stream` must be seekable as well as writable.)

Although you could implement the `IFreezable<TSelf>` interface manually in each of your freezable classes and structures, Carbonite provides a code generator (in the `Carbonite.FreezableCodeGen` assembly) that will do this automatically, serializing all the declared fields of the type contiguously in the declared order.

Here is an example of freezing an object to a Carbonite Image (for more detail see the `CarboniteExampleWriter` project):

```csharp
using (FileStream stream = new FileStream(filename, FileMode.Create))
using (CarboniteImageWriter imageWriter = new CarboniteImageWriter(stream, payloadFormatVersion))
{
    imageWriter.WriteRootObject(objectToSerialize);
}
```


Unfreezing
----------

The unfreezing process is handled by the Carbonite C++ library (although it is by design not very complicated, so you could handle it yourself if you wanted to).

Unfreezing is accomplished by calling the `CarboniteUnfreezeImage(...)` function, which verifies that the given payload format version matches the version in the image, fixes up the relative offsets in the image, and then returns a `const CarboniteImage*` so you can access its root objects in `RootObjectTable`. Carbonite does not store any type information in an image - your native code is expected to know what the types of the objects in the image are.

Remember that this `CarboniteImage` pointer, as well as all the objects inside it, are a view of the memory that was passed to `CarboniteUnfreezeImage`, so make sure not to deallocate that memory before you are done accessing the image.

Here is an example of unfreezing a Carbonite Image and iterating through its root objects (for more detail see the `CarboniteExampleNative` project):

```cpp
// Unfreeze the bytes we loaded from disk
const CarboniteImage* image = CarboniteUnfreezeImage(imageBytes, MY_PAYLOAD_FORMAT_VERSION);

if (image != nullptr) {
    for (size_t i = 0; i < image->RootObjectTable.Count; i++) {
        const ExampleModel* model = (const ExampleModel*)image->RootObjectTable[i];
        // Do something with `model`
    }
}

// Now that we are done accessing the image, we can free the memory
delete[] imageBytes;
```


Data Structure Authoring
------------------------

There are a number of things to keep in mind when authoring data structures for use with Carbonite:
 - C# Arrays and `string`s are frozen as a `size_t` length and a pointer to the array elements/characters. `carbonite.h` provides the corresponding `CarboniteArray<T>` and `CarboniteString` structures to be used in your native types, and you can find more information on how to use them in `carbonite.h` itself.
 - Like how C# does things, C# `struct`s are serialized inline, and `class`es are serialized as pointers to a some other part of the image.
 
    In other words, if you create a type `struct MyThing` in C#, in C++ it must be used as a value in containing types (structures, classes, and arrays) (e.g. `MyThing Thing;` and `CarboniteArray<MyThing>`).
    
    If you instead create a type `class MyThing` in C#, in C++ it must be used as a pointer in containing types (e.g. `MyThing* Thing;` and `CarboniteArray<MyThing*>`).
 - We all love `const` correctness, right? Unfrozen images are not meant to be modified, so it makes sense to mark all pointers as `const` in your native data structures.
 - If you want your C# type to automatically implement the `IFreezable<TSelf>` interface with all required members generated for you, mark your type as `partial` and apply the `[GenerateFreezable]` attribute to it.

    The source generator will serialize each member variable back-to-back in declaration order when freezing that type. (Variables are written - not properties - to allow avoiding redundant `struct` copies, by using `in` parameters). If you would rather your types expose properties rather than `public` variables, you can make your variables `private` - they will still get frozen just fine, and then you can implement `public` properties to wrap them.
 - The Carbonite C# source generator for freezable types does not yet understand C++ padding and alignment, meaning that you need to manually ensure that there are no bytes in the C++ structure that are implicitly inserted due to alignment and padding rules.

   Having to manually ensure C# and native structure layouts are matching is not ideal, and options are being explored for moving to a single source of truth that would automatically handle layout parity in a future version of Carbonite.
