# IshakBuildTool
This is a Build Tool that has the prupose of Building the development enviroment for my game Engine called IshakEngine.
NOTE: Still work in progress, the basic architechture is planned, but can be changed along the implementation.

This tool will set the Visual Studio Enviroment and will be architechtured for compiling the Engine Project, so when developing a game
we will use custom compile actions like: Multi-threading compilation, re-compiling checks, logs and more.

I have made the architercture in a way that the Engine is divided in Modules. Every module counts with Module build file( Module.cs ) which will define further information about the module like:
dependencies, static libraries, dynamic libraries loading,etc.
At run time all the modules builder files are compiled and the info extracted, so all the enviroment can be set correctly.
