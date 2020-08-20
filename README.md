# ICsi
### Currently, ICsi is not quite stable. Use it at your own risk.

ICsi is a new REPL (read-eval-print-loop) designed to run C# code. It supports only .NET Core 3.1 and newer versions.

## Building and running

Requirements:

* .NET Core 3.1 LTS or newer (you can also use the latest preview of .NET 5 if you want)
* Visual Studio 2019 (optional, but it is important for debugging)
* Git (latest version)
* PowerShell Core (on both Windows and Linux)


1. Clone the repository using Git:
```
git clone https://github.com/iurie5100/icsi.git
```

2. Use the build script to:
* Restore:
```
.\build.ps1 -target restore
```

* Build:
```
.\build.ps1 -target build
```

* Clean (if needed):
```
.\build.ps1 -target clean
```
There is also a option to specify the build configuration by using -configuration along with the target.

3. Now open artifacts, then there will be a folder named Debug or Release (depending on the build configuration) which is where the binaries are built.

## Using ICsi
Using ICsi is just as easy as using any other REPL. You just type in some C# code, press ENTER and you're good to go. Use the PageUp and PageDown keys to load previous code.

``` c#
> using static System.Console;

> for (int i = 0; i <= 10; i++)
      WriteLine(i);
1
2
3
4
5
6
7
8
9
10

> 
```

## Contributing
Want to contribute? Fork this repository and send a pull request with your changes.

## License
ICsi is licensed under the MIT License.