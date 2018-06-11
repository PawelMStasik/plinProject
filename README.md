# p-lin Project
A simple project for testing image interpolation and an implementation of my own pixel-art scaling methods.

## Goal and Purpose

The p-lin Project serves two main purposes:
* it showcases an implementation of several new pixel-art scaling methods,
* it can be used for prototyping new scaling methods.

The main project is `plin`. It serves as a command-line application and as a DLL. New functions can be added by either modifying its code or in-run with functions provided by the library part.

The second project is `plin_showcase`, which presents how the application can be used as a library.

## Scaling methods

There are two classic scaling methods provided by `plin`:
* linear interpolation,
* nearest neighbour.

Three new scaling techniques were introduced in this project:
* proximity-based coefficient correction (PBCC),
* p-lin (a 1D simplification of PBCC),
* transition area restriction (TAR).

Personally I'd advise p-lin with TAR set to 2 for scaling pixel art.

## Requirements

The applications require .NET 4.5 (or compatible Mono version) to work.

The project files require at least Visual Studio 2012.

## Example of Use

`plin.exe` -
  Simply calling the application without any parameters brings out its help and list of commands.

`plin.exe image_path width height output_file` -
  This will perform a p-lin interpolation on a bitmap at `image_path`. The resulting `width` x `height` image will be saved as `output_file`.

`plin.exe image_path scale output_file` -
  This is the similar case as above, but instead of providing desired dimensions we set the scale factor.

`plin.exe image_path scale output_file -lin` -
  Performs linear interpolation.

`plin.exe image_path scale output_file -nn` -
  Performs nearest neighbour.

`plin.exe image_path scale output_file -plin` -
  Performs p-lin interpolation (this is the same as calling out without stating a mathod).

`plin.exe image_path scale output_file -lin -pbcc` -
  Performs linear interpolation with PBCC.

`plin.exe image_path scale output_file -lin -tar:2` -
  Performs linear interpolation with TAR set to 2.

## Reference

If you use this project in in scientific purposes, I'd appreciate if you could refer to the following article:
* P. M. Stasik and J. Balcerek, "Improvements in upscaling of pixel art," 2017 Signal Processing: Algorithms, Architectures, Arrangements, and Applications (SPA), Poznan, 2017, pp. 371-376.
