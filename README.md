## CPSC411
Code for compiler construction assignments

#### Usage - Linux

1. Install mono - a linux implementation of the .net framework
  1. It should be sufficient to invoke `sudo apt-get install mono-complete`
  2. If, however there are problems encountered, plese consult the [mono installation instructions](http://www.mono-project.com/docs/getting-started/install/linux/)
1. Place any sample files in the code directory
2. Invoke the code via `mono SimpleCompiler.exe [Optional file name]`
  1. for uspecified filename, a random sample in the root directory will be parsed
  2. for a specified filename, the file of the same name in the SampleFiles director will be parsed.

#### Usage - Windows
These usage instructions assume a working copy of visual studio is installed.

1. Open SimpleCompiler.sln in the root directory
2. Build code through by compiling the solution in the visual studio solution explorer
3. Place any sample files in the code directory
4. Invoke the program in the command line via `SimpleCompiler.exe [Optional file name]`
  1. for uspecified filename, a random sample in the root directory will be parsed
  2. for a specified filename, the file of the same name in the SampleFiles director will be parsed.
