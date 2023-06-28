# IshakBuildTool
This is a Build Tool that has the prupose of Building the development enviroment for my game Engine called IshakEngine.

**IMPORTANT:** The Ishak Engine is a ***single platform*** Engine created for windows, so the Build tool is architechture **JUST for Windows**, at least for now.

**NOTE: Still work in progress**, the basic architechture is planned, but can be changed along the implementation.

This tool will set the Visual Studio Enviroment and will be architechtured for compiling the Engine Project, so when developing a game
we will use custom compile actions like: Multi-threading compilation, re-compiling checks, logs and more.

I have made the architercture in a way that the Engine is divided in Modules. Every module counts with Module build file( Module.cs ) which will define further information about the module like:
dependencies, static libraries, dynamic libraries loading,etc.
At run time all the modules builder files are compiled to a DLL so any info can be extracted and all the enviroment can be set correctly.

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
