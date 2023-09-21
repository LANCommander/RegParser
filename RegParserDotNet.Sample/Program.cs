using RegParserDotNet;

var parser = new RegParser();

var frogger = File.ReadAllText("frogger.reg");

var froggerKeys = parser.Parse(frogger);

Console.WriteLine($"Found {froggerKeys.Count()} keys!");