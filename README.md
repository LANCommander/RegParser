# RegParserDotNet

RegParserDotNet is a .NET library to parse `.reg` export files from RegEdit in Windows.
## Quick Start

Implementation is easy. Instantiate a new parser and feed it the text contents of your reg file:
```csharp
using RegParserDotNet;

var parser = new RegParser();

var frogger = File.ReadAllText("frogger.reg");

var froggerKeys = parser.Parse(frogger);

Console.WriteLine($"Found {froggerKeys.Count()} keys!");
```