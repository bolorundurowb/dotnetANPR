# dotnetANPR

[![Build and Test](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)


## About
dotnetANPR is an automatic number plate recognition software, which implements algorithmic and mathematical principles from field of artificial intelligence, machine vision and neural networks.

This is a work in progress. I am converting the java code in javaanpr into cross-platform C# (that means no reliance on System.CDrawing.Common) which would make the library usable by .NET developers. I have finished porting all the non-GUI functionality but I am yet to convert the GUI (will do that soon). I am currently moving into testing to see how well the library does with recognizing characters and to pick out bugs.

Contibutions are very welcome.

This project aims to port [this](https://github.com/oskopek/javaanpr) library from Java to .NET. Thanks to [Ondrej Martinsky](https://sourceforge.net/u/martinsky/profile/) for the original `javaanpr` library.

## Usage

## Testing

The solution includes a unit test project (`dotnetANPR.Tests`) built with the cross-platform [MSTest](https://learn.microsoft.com/dotnet/core/testing/) SDK and using [OmniAssert](https://github.com/bolorundurowb/OmniAssert) for assertions.

```bash
dotnet test --solution ./src/dotnetANPR.slnx
```
