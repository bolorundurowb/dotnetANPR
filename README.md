# dotNETANPR

[![Build Status](https://travis-ci.org/bolorundurowb/dotnetANPR.svg?branch=develop)](https://travis-ci.org/bolorundurowb/dotnetANPR)


## About
dotNETANPR is an automatic number plate recognition software, which implements algorithmic and mathematical principles from field of artificial intelligence, machine vision and neural networks.

This is a work in progress. I am converting the java code in javaanpr into C# which would make the library usable by .NET developers. I have finished porting all the non-GUI functionality but I am yet to convert the GUI (will do that soon). I am currently moving into testing to see how well the library does with recognizing characters and to pick out bugs.

Contibutions are very welcome.

This project aims to port [this](https://github.com/oskopek/javaanpr) library from Java to .NET. Big ups to [Ondrej Martinsky](https://sourceforge.net/u/martinsky/profile/) for the original `javaanpr` library.

## Usage
Usage : `mono dotNETANPR.exe [-options]`

### Options:

**-help**

Displays this help

**-gui**

Run GUI viewer (default choice)

**-recognize -i &lt;snapshot&gt;**

Recognize single snapshot

**-recognize -i &lt;snapshot&gt; -o &lt;dstdir&gt;**

Recognize single snapshot and save report html into specified directory

**-newconfig -o &lt;file&gt;**

Generate default configuration file

**-newnetwork -o &lt;file&gt;**

Train neural network according to specified feature extraction method and learning parameters (in config. file) and saves it into output file

**-newalphabet -i &lt;srcdir&gt; -o &lt;dstdir&gt;**

Normalize all images in &lt;srcdir&gt; and save it to &lt;dstdir&gt;
