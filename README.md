# dotNETANPR

[![Build and Test](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

## About

dotnetANPR is an automatic number plate recognition library that implements algorithmic and mathematical principles from the fields of artificial intelligence, machine vision, and neural networks.

This project is a C# port of the [JavaANPR](https://github.com/oskopek/javaanpr) library. Thanks to [Ondrej Martinsky](https://sourceforge.net/u/martinsky/profile/) for the original `javaanpr` library.

The library is cross-platform and uses [SkiaSharp](https://github.com/mono/SkiaSharp) for image processing, so it has no dependency on `System.Drawing.Common`.

## Build

The solution is located under the `src/` folder.

```bash
dotnet build ./src/dotnetANPR.slnx --configuration Release
```

## Test

The solution includes an xUnit test project (`dotnetANPR.Tests`).

```bash
dotnet test ./src/dotnetANPR.slnx --configuration Release
```

## Usage

### As a library

```csharp
using DotNetANPR;

var plateText = ANPR.Recognize("path/to/car.jpg");
Console.WriteLine(plateText ?? "No plate recognized");
```

You can also recognize from a `Stream` or an `SKBitmap`:

```csharp
using var stream = File.OpenRead("path/to/car.jpg");
var plateText = ANPR.Recognize(stream);
```

### CLI

```bash
dotnet run --project ./src/dotnetANPR.CLI/dotnetANPR.CLI.csproj -- -recognize -i path/to/car.jpg
```

Other CLI options:

```text
-help                          Display this help
-recognize -i <snapshot>       Recognize a single snapshot
-newconfig -o <file>           Generate default config file (JSON)
-newnetwork -i <learndir> -o <file>
                               Train neural network and save
-newalphabet -i <srcdir> -o <dstdir>
                               Normalize alphabet images
```

## Configuration

Configuration is stored in `Resources/config.json` and embedded into the library assembly at build time. You can export the default configuration:

```bash
dotnet run --project ./src/dotnetANPR.CLI/dotnetANPR.CLI.csproj -- -newconfig -o config.json
```

Edit the exported JSON and load it at runtime with `AnprConfig.Load(path)` before calling `ANPR.Recognize`.

## Project structure

| Project | Target | Description |
|---|---|---|
| `dotnetANPR` | `net6.0` | Core ANPR library |
| `dotnetANPR.CLI` | `net10.0` | Command-line interface |
| `dotnetANPR.Tests` | `net10.0` | xUnit test suite |

## Contributing

Contributions are very welcome. Please ensure the solution builds and all tests pass before opening a pull request.

## License

This project is licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for details.
