# ICsi
### Currently, ICsi is not quite stable. Use it at your own risk.

ICsi is a new REPL (read-eval-print-loop) designed to run C# code. It supports only .NET Core 3.1 and newer versions.

## Building and running

Requirements:

* .NET Core 3.1 LTS or newer (you can also use the latest preview of .NET 5 if you want)
* Visual Studio 2019 (optional)
* Git


1. Clone the repository using Git:
```
git clone https://github.com/iurie5100/icsi.git
```

2. Use .NET Core CLI to restore and build the project.
```
dotnet restore
dotnet build ICsi.sln
```

3. Run ICsi using:
```
dotnet run --project .\src\icsi.csproj
```

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

## License
ICsi is licensed under the MIT License.