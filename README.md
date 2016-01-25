## CPSC411
Code for compiler construction assignments

### Linux
As we can't assume that an IDE designed for c# development is present, a list of the important files is provided:

```
./SimpleCompiler.cs
Contains the main of the program, handles reading in files, feeding text to the lexer, and outputs the resulting token string.

./Lexer/Lexer.cs
Contains the main business logic of the first assignment and handles translating strings to tokens.

./Lexer/Token.cs
./Lexer/IToken.cs
Contains the inerface and implementation of tokens. Currently rather basic as no real use for tokens has been encoutnered yet.
```

#### Usage

1. Install mono - a linux implementation of the .net framework
  1. It should be sufficient to invoke `sudo apt-get install mono-complete`
  2. If, however there are problems encountered, plese consult the [mono installation instructions](http://www.mono-project.com/docs/getting-started/install/linux/)
2. Compile the code by invoking `mcs SimpleCompiler.cs`
3. Place any sample files in the code directory
4. Invoke the code via `mono SimpleCompiler.exe [Optional file name]`
  1. for uspecified filename, a random sample in the root directory will be parsed
  2. for a specified filename, the file of the same name in the SampleFiles director will be parsed.

### Windows

```
This readme assumes the presence of Visual Studio of at least 2012 as an IDE.
The solution file contains a single project, with the main being contained in SimpleCompiler.cs
The Lexer folder contains the code for the lexer and the classes it uses.
```

#### Usage
These usage instructions assume a working copy of visual studio is installed.

1. Open SimpleCompiler.sln in the root directory
2. Build code through by compiling the solution in the visual studio solution explorer
3. Place any sample files in the code directory
4. Invoke the program in the command line via `SimpleCompiler.exe [Optional file name]`
  1. for uspecified filename, a random sample in the root directory will be parsed
  2. for a specified filename, the file of the same name in the SampleFiles director will be parsed.
