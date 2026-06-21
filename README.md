# dotnetANPR

[![Build and Test](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/dotnetANPR/actions/workflows/build-and-test.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

## About

dotnetANPR is an automatic number plate recognition library implementing algorithmic and mathematical principles from artificial intelligence, machine vision, and neural networks. It is a cross-platform .NET port of [javaanpr](https://github.com/oskopek/javaanpr) by Ondrej Martinsky.

This is a work in progress. All non-GUI functionality has been ported; testing and bug-fixing are ongoing. Contributions are very welcome.

## Usage

```csharp
using dotnetANPR;

// Recognise from a file path
string? plate = ANPR.Recognize("car.jpg");

// Dump intermediate processing stages for diagnostics
string? plate = ANPR.Recognize("car.jpg", dumpDir: "./stages");

// Recognise from a Stream
using var stream = File.OpenRead("car.jpg");
string? plate = ANPR.Recognize(stream);
```

## Testing

```bash
dotnet test --solution ./src/dotnetANPR.slnx
```

---

## Configuration Reference

The recognition pipeline is controlled by `Resources/config.xml`. Each parameter is listed below with its purpose and the effect of adjusting it.

### Photo Preprocessing

| Parameter | Default | Description | Effect of Increasing |
|---|---|---|---|
| `photo_adaptivethresholdingradius` | `7` | Radius for adaptive thresholding. `0` uses plain global thresholding; `N >= 1` uses a neighbourhood of radius `N`. | Larger radius produces smoother thresholding at the cost of losing fine detail. Lower values (or 0) preserve edges but are more sensitive to noise. |

### Skew Detection

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `intelligence_skewdetection` | `0` | Enables skew detection and de-skewing correction via Hough transform. `0` = disabled, `1` = enabled. | Enabling adds computational cost but corrects rotated plates, improving character recognition. |

### Plate Candidate Search

| Parameter | Default | Description | Effect of Increasing |
|---|---|---|---|
| `intelligence_numberOfBands` | `3` | Number of horizontal bands (candidate plate rows) extracted from the car image. | More bands catch more candidates but increase processing time and false positives. |
| `intelligence_numberOfPlates` | `3` | Number of plate candidates extracted from each band. | More candidates per band may find plates in unusual positions but increases processing time. |
| `intelligence_numberOfChars` | `20` | Maximum number of character peaks extracted from a plate's horizontal histogram. | Higher values allow more characters to be segmented but risk splitting characters or picking up noise. |

### Plate Heuristics

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `intelligence_minimumChars` | `5` | Minimum number of detected characters for a valid plate. | Lower values accept shorter plates (e.g. motorcycles); higher values reject partial detections. |
| `intelligence_maximumChars` | `15` | Maximum number of detected characters for a valid plate. | Lower values reject over-segmented plates; higher values accept longer plate formats. |
| `intelligence_maxCharWidthDispersion` | `0.5` | Maximum allowed variation in character widths relative to the average. | Higher values tolerate irregular character widths; lower values enforce uniform character widths. |
| `intelligence_minPlateWidthHeightRatio` | `0.5` | Minimum width-to-height ratio for a valid plate. | Higher values reject square-ish regions; lower values accept narrower plates. |
| `intelligence_maxPlateWidthHeightRatio` | `15.0` | Maximum width-to-height ratio for a valid plate. | Higher values accept very wide plates; lower values reject overly elongated regions. |

### Character Heuristics

| Parameter | Default | Description | Effect of Increasing |
|---|---|---|---|
| `intelligence_minCharWidthHeightRatio` | `0.1` | Minimum width-to-height ratio for a valid character. | Higher values reject very narrow characters (like '1' or 'I'); lower values accept them. |
| `intelligence_maxCharWidthHeightRatio` | `0.92` | Maximum width-to-height ratio for a valid character. | Higher values accept wider characters (like 'W' or 'M'); lower values reject them. |
| `intelligence_maxBrightnessCostDispersion` | `0.161` | Maximum allowed brightness deviation of a character from the plate average. | Higher values accept characters with more varied lighting; lower values enforce uniform brightness. |
| `intelligence_maxContrastCostDispersion` | `0.1` | Maximum allowed contrast deviation of a character from the plate average. | Higher values tolerate more varied background/text contrast; lower values enforce uniformity. |
| `intelligence_maxHueCostDispersion` | `0.145` | Maximum allowed hue deviation of a character from the plate average. | Higher values accept characters with different colour tones; lower values require consistent colouring. |
| `intelligence_maxSaturationCostDispersion` | `0.24` | Maximum allowed saturation deviation of a character from the plate average. | Higher values tolerate varied colour intensity; lower values enforce uniform saturation. |
| `intelligence_maxHeightCostDispersion` | `0.2` | Maximum allowed height deviation of a character from the plate average. | Higher values accept characters of differing heights; lower values require uniform height. |
| `intelligence_maxSimilarityCostDispersion` | `100.0` | Maximum classification cost for a recognised character to be accepted. | Higher values accept less confident matches (more false positives); lower values require stronger matches. |

### Character Normalisation & Feature Extraction

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `char_normalizeddimensions_x` | `8` | Width (in pixels) to which characters are downsampled before recognition. | Larger values preserve more detail but increase processing time and memory. Must match the learned alphabet dimensions. |
| `char_normalizeddimensions_y` | `13` | Height (in pixels) to which characters are downsampled before recognition. | Same as above — must match the alphabet used for training. |
| `char_learnAlphabetPath` | `Resources/alphabets/alphabet_8x13` | Path to the directory containing alphabet training images. | Point to a custom alphabet for different fonts or character styles. Images must match the normalised dimensions. |
| `char_resizeMethod` | `1` | Downsampling method. `0` = linear (better for edge detection), `1` = weighted average (better for pixel mapping). | Use `0` with edge-based feature extraction; use `1` with map-based feature extraction. |
| `char_featuresExtractionMethod` | `0` | Feature extraction method. `0` = direct pixel mapping (good for blurred characters), `1` = edge features (good for skewed characters). | Use `0` for clean/blurred characters; use `1` for skewed or deformed characters. |

### Classification Method

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `intelligence_classification_method` | `0` | Classification algorithm. `0` = KNN Euclidean distance pattern matching, `1` = feed-forward neural network. | Pattern matching is faster and requires no training. Neural network is more accurate but requires a trained network file. |

### Neural Network Parameters

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `char_neuralNetworkPath` | `Resources/neuralnetworks/network_avgres_813_map.xml` | Path to the pre-trained neural network XML file. Dimensions must match the feature extraction method and normalised character size. | Only used when `intelligence_classification_method` is `1`. |
| `neural_topology` | `20` | Number of neurons in the hidden (middle) layer. | More neurons increase the network's capacity and accuracy but also increase training time and risk overfitting. |
| `neural_maxk` | `8000` | Maximum number of training iterations (epochs). | Higher values allow more thorough training but may overfit. Lower values may stop before convergence. |
| `neural_eps` | `0.07` | Convergence threshold (epsilon). Training stops when the total gradient falls below this value. | Lower values produce more precise training; higher values stop earlier (faster but less accurate). |
| `neural_lambda` | `0.05` | Learning rate (lambda). Controls how much weights are adjusted per iteration. | Higher values converge faster but may overshoot; lower values converge more stably but slowly. |
| `neural_micro` | `0.5` | Momentum factor (micro). Persistence ratio of previous weight changes. | Higher values smooth the gradient descent and help escape local minima; lower values respond faster to new gradients. |

### Syntax Analysis

| Parameter | Default | Description | Effect of Changing |
|---|---|---|---|
| `intelligence_syntaxanalysis` | `2` | Syntax correction mode. `0` = no correction, `1` = correct only if character count matches a known format, `2` = correct regardless, eliminating redundant characters. | Mode `2` is most aggressive (may over-correct); mode `1` is safer. Mode `0` returns raw recognition. |
| `intelligence_syntaxDescriptionFile` | `Resources/syntax.xml` | Path to the XML file defining known plate format templates. | Point to a custom file to add plate formats for different countries. See syntax reference below. |

### Graph Peak Analysis

These parameters control histogram peak detection used for finding plate bands, plates, and characters. They should generally not need adjustment unless the source images have unusual characteristics.

| Parameter | Default | Description | Effect of Increasing |
|---|---|---|---|
| `carsnapshot_graphrankfilter` | `9` | Rank filter applied to the car snapshot vertical histogram. | Higher values produce smoother histograms, reducing noise but potentially merging nearby peaks. |
| `carsnapshot_distributormargins` | `25` | Margins for the probability distribution applied to car snapshot peaks. | Larger margins spread probability more broadly, favouring wider bands. |
| `carsnapshotgraph_peakDiffMultiplicationConstant` | `0.1` | Multiplier for peak difference calculation in the car snapshot graph. | Higher values require larger gaps between peaks to be considered separate. |
| `carsnapshotgraph_peakfootconstant` | `0.55` | Fraction of peak height used to determine the foot (base) of a peak. | Higher values result in narrower peak boundaries; lower values widen them. |
| `bandgraph_peakDiffMultiplicationConstant` | `0.2` | Multiplier for peak difference calculation in band graphs. | Same as above but for plate detection within bands. |
| `bandgraph_peakfootconstant` | `0.55` | Peak foot constant for band graphs (plate detection). | Same effect as `carsnapshotgraph_peakfootconstant` but for plate-finding. |
| `plategraph_peakfootconstant` | `0.7` | Peak foot constant for character segmentation within plates. | Higher values produce narrower character segments; lower values widen them. |
| `plategraph_rel_minpeaksize` | `0.86` | Relative minimum peak size as a fraction of the maximum peak. | Higher values ignore smaller peaks (fewer character candidates); lower values detect more candidates. |
| `platehorizontalgraph_detectionType` | `1` | Horizontal edge detection method. `0` = magnitude derivative, `1` = Sobel edge detection. | Edge detection (`1`) is more robust for finding plate boundaries. |
| `platehorizontalgraph_peakfootconstant` | `0.05` | Peak foot constant for the plate's horizontal edge graph (left/right bounds). | Higher values produce tighter left/right cropping of the plate. |
| `plateverticalgraph_peakfootconstant` | `0.42` | Peak foot constant for the plate's vertical edge graph (top/bottom bounds). | Higher values produce tighter top/bottom cropping of the plate. |

---

## Syntax Configuration Reference

`Resources/syntax.xml` defines known licence plate format templates per country. Each `<type>` is a named template (e.g. `germany`). Each `<char>` element defines the set of allowed characters at that position. During syntax analysis, the parser tries to match recognised text against these templates and corrects mismatches when possible.

### Format Templates

| Template Name | Positions | Pattern | Description |
|---|---|---|---|
| `germany` | 6 | `LLL DDD` | Letters: a–z, 0, o; Digits: 0–9. German-style plate (e.g. `ABC 123`). |
| `slovensko_nova` | 7 | `LL DD LL` | Letters: a–z, 0, o; Digits: 0–9, o. New Slovak plate (e.g. `AA 123 AB`). |
| `ceskoslovenska_novsia` | 7 | `LLL DDDD` | Letters: a–z, 0, o; Digits: 0–9, o. Newer Czechoslovak plate (e.g. `AAA 1234`). |
| `ceskoslovenska_starsia` | 6 | `LL DDDD` | Letters: a–z, 0, o; Digits: 0–9, o. Older Czechoslovak plate (e.g. `AB 1234`). |
| `ceska_nova` | 7 | `D L DDDDD` | First: 0–9, o; Second: c,b,k,h,l,t,m,e,p,a,s,u,j,z; Digits: 0–9, o. New Czech plate (e.g. `1A2 3456`). |

### Character Set Notation

- `a-z` — alphabetic characters (uppercase or lowercase)
- `0-9` — numeric digits
- `o` — the letter O, often included in digit positions because O and 0 are visually similar and frequently confused by OCR
- `0` — the digit zero, often included in letter positions for the same reason

### Adding a New Format

To add a custom plate format, add a `<type>` element inside `<structure>`:

```xml
<type name="uk">
    <char content="abcdefghijklmnopqrstuvwxyz"/>
    <char content="0123456789"/>
    <char content="0123456789"/>
    <char content="abcdefghijklmnopqrstuvwxyz"/>
    <char content="abcdefghijklmnopqrstuvwxyz"/>
    <char content="abcdefghijklmnopqrstuvwxyz"/>
</type>
```

Each `<char>` element represents one position in the plate. The `content` attribute lists all characters allowed at that position.

---

## API Documentation

XML doc comments are provided on all public types and members. The main entry point is the static `ANPR` class:

- `ANPR.Recognize(string imagePath, string? dumpDir)` — Recognise a plate from a file path.
- `ANPR.Recognize(Stream imageStream, string? dumpDir)` — Recognise a plate from a stream.
- `ANPR.Recognize(SKBitmap image, string? dumpDir)` — Recognise a plate from a SkiaSharp bitmap.
- `ANPR.ExportDefaultConfig(string outputFilePath)` — Export default configuration to XML.
- `ANPR.TrainNetworkAndExport(string outputFilePath)` — Train a neural network and export it.
- `ANPR.NormalizeAlphabets(string sourceDir, string dstDir)` — Normalise alphabet character images.

## License

GPL v3 — see [LICENSE](LICENSE).
