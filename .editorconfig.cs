// Purpose of this file is to suppress
// the warnings that are suppressed with
// the editorconfig for the vscode csharp plugin,
// which seems to ignore the editorconfig
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "Stylecop", 
    checkId: "SA1633: File should have header", 
    Justification = "code is not copyrighted.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "Stylecop", 
    checkId: "SA1600: Elements should be documented",
    Justification = "code may be self explainatory. documentations is still encouraged.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "Stylecop", 
    checkId: "SA1028: Code should not contain trailing whitespace", 
    Justification = "usually removed by code format.")]

// this suppresses a warning at build-time which is not shown anywhere in code.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "Stylecop", 
    checkId: "SA0001: XML comment analysis is disabled due to project configuration", 
    Justification = "Documentation requirements are not given.")]