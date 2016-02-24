CPSC 411 Assignment 2

Usage:
	Compile with mono, ideally using monodevelop as an ide
	Run by invoking SimpleCompiler.exe
		Can pass file name as a parameter to parse a specific file or
		Can pass no parameter to randomly pick a file in the folder

	Program outputs 2 files - StackCode.txt: the compiled code
	And a tree representation file called raphviz.dot

Visualizing the tree:
	The tree visualization is done with the graphviz library.
	Installing graphviz can be done via abt-get on linux
	Invoking graphviz is done via the command:
	    dot -Tpng graphviz.dot -o AST.png
	This will generate AST.png which is an image representing the AST generated
	by the code
