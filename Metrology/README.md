﻿# Sample Metrology Project

A project to build _metrology (units of measurement)_ class library for use in C#
[dotnet](https://dotnet.microsoft.com/en-us/) applications (such as
[CALINE3](https://github.com/mangh/CALINE3.CS/tree/main/CALINE3)). 


## Install project template

- To create such a project you need the
[Mangh.Metrology.CSUnits](https://www.nuget.org/packages/Mangh.Metrology.CSUnits)
template ([NuGet](https://www.nuget.org/) project template package), which can be installed using
the following [dotnet](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet) command:

  ```sh
  dotnet new install Mangh.Metrology.CSUnits
  ```

## Create project from the template

- Create a project from the template installed above
(labeled with the _short name_ `unitsofmeasurement` in the command below):

  ```sh
  dotnet new csunits -n Metrology -o Metrology -ns Metrology
  ```
  This will create a project of name (-n) `Metrology`, in the folder of name (-o) `Metrology`,
  for units of measurement in namespace (-ns) `Metrology`:
  ```sh
  ~/CALINE3.CS/Metrology$ ll
  
  total 36
  drwxr-xr-x 6 marek marek 4096 Jun 19 10:02 .
  drwxr-xr-x 5 marek marek 4096 Jun 12 10:28 ..
  drwxr-xr-x 2 marek marek 4096 Jun 12 09:38 Core
  -rw-r--r-- 1 marek marek  430 Jun 12 09:52 Directory.Build.targets
  -rw-r--r-- 1 marek marek 2901 Jun 12 09:51 Math.cs
  -rw-r--r-- 1 marek marek 2141 Jun 12 09:54 Metrology.csproj
  drwxr-xr-x 2 marek marek 4096 Jun 12 09:38 Parsers
  drwxr-xr-x 2 marek marek 4096 Jun 12 09:38 RuntimeLoader
  drwxr-xr-x 2 marek marek 4096 Jun 12 10:03 Templates
  
  ~/CALINE3.CS/Metrology$
  ```

- Under Visual Studio IDE, you can attach the project to a
[Visual Studio solution](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022).

## Customize the project

- Edit `Templates/definitions.txt` file to specify units of measurement required in your application(s).

- You can (optionally) edit the `Templates/*.xslt` templates to customize the structures to be generated in C#:
    - `unit.xslt`: template for a single _unit_ (partial struct),
    - `scale.xslt`: template for a single _scale_ (partial struct),
    - `catalog.xslt`: template for a _Catalog_ class (catalog of all defined units and scales),
    - `aliases.xslt`: template for _Aliases.inc_ file (that may be used to import defined unit and scale types to dependent projects),
    - `report.xslt`: template for _generator_report.txt_ file (a summary of generated units and scales).

- You can (optionally) create extensions for the generated measurement units to extend their functionality beyond what is generated by default:
units/scales are _partial structures_, so each one can be extended with additional file(s) that provide additional sections (functionality) of the structure.

- It may be necessary to edit the `./Math.cs` file to adapt mathematical operations for use with units of measurement
(or, perhaps even better, move this file directly to a dependent application and make the necessary adjustments there),

- You may also want to import defined unit types (by means of the [global using](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive) statements) to dependent projects.
To do this, edit the `Directory.Build.targets` file so that it automatically exports the generated `Templates/Aliases.inc`
file to dependent projects, whenever the unit library project (`Metrology`) is built.
The advantage of this facility is that you can easily switch between (dimensionally safe) solution that use units od measure and the (faster) solution that use plain numbers only.
The switch is performed via `DIMENSIONAL_ANALYSIS` symbol that can be defined (for solution with units) or undefined (for solution with numbers instead of units) before compiling the dependent project.
