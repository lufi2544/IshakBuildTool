# IshakBuildTool
This is a Build Tool that has the prupose of Building the development enviroment for my game Engine called IshakEngine.

**NOTE: Still work in progress**, the basic architechture is planned, but can be changed along the implementation.

## Motivation
I have opted for C# for 2 main reasons:

  &rarr; **First:** I wanted to learn C#.

  &rarr; **Second:** Wanted to take advantage of the .NET framework that I think is a very powerful library for this kind of projects.

**All the IshakEngine is going to be developed in pure C++**

## Introduction

**IMPORTANT:** The Ishak Engine is a ***single platform*** Engine created for windows, so the Build tool is architechture **JUST for Windows**, at least for now.

This tool will set the Visual Studio Enviroment and will be architechtured for compiling the Engine Project, so when developing a game
we will use custom compile actions like: Multi-threading compilation, re-compiling checks, logs and more.

I have made the architercture in a way that the Engine is divided in Modules. Every module counts with Module build file( Module.cs ) which will define further information about the module like:
dependencies, static libraries, dynamic libraries loading,etc.
At run time all the modules builder files are compiled to a DLL so any builder can be extracted at runtime and the Modules created in consequence.

<img src="/RepoImages/Screenshot 2023-06-28 165418.png" alt="Alt text" title="Module Image">

Here we have an example of one of the Modules. In this case the  build file is the Renderer.Module.cs.



## Getting Started
If you want to start taking a look at the Build Tool Flow, the starting point would be:
'''IshakBuildTool.cs'''
```cpp
IshakBuildToolFramework.Execute(args);
```


<img src="/RepoImages/ProjectFilesHandler.png" alt="Alt text" title="Module Image">

This is where we discover all the modules.
