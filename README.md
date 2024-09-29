# W3G.NET

A replay parser tool for Warcraft III.

The tool is based on [w3gjs](https://github.com/PBug90/w3gjs)

[![NuGet downloads](https://img.shields.io/nuget/dt/W3GNET.svg)](https://www.nuget.org/packages/W3GNET/)
[![NuGet version](https://img.shields.io/nuget/v/W3GNET.svg)](https://www.nuget.org/packages/W3GNET/)

## Usage

### Example 1
``` cs
using W3GNET;

var w3replay = new W3GReplay(false);
using (var stream = new FileStream("C:\\path\\to\\replay.w3g", FileMode.Open))
{
    await w3replay.Parse(stream);
}
```

### Example 2
``` cs
using W3GNET;

byte[] replayData;
var w3replay = new W3GReplay(false);
using (var stream = new MemoryStream(replayData))
{
    await w3replay.Parse(stream);
}
```


## Dependencies

[![dotnet standard 2.1](https://img.shields.io/badge/.NET%20standard-v2.1-brightgreen.svg)](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)