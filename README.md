# GoFlag

GoFlag is a C# port of the [flag package](https://golang.org/pkg/flag/) in golang. It provides a simple interface for parsing command-line flags. It implements the same api footprint by using generics, which adds a slightly more elegant use of static typing by using an implicit operator. The result is a easy-to-use libary that should get your console app up-and-running in no time.

It's a very elegant design and I wanted to port it for two reasons:

1. I could use it in C#
2. I could share a beauty of a golang pkg to the C# community and possibly get some cross-polination.

If you have any questions or have any feature suggestions, please fire away. The footprint will always remain the same, but that doesn't mean we can't make some improvements and additions.

[![Goflag logo](https://github.com/mikeruhl/goflag/blob/main/goflag-icon.png)

[![NuGet version](https://badge.fury.io/nu/Goflag.svg)](https://badge.fury.io/nu/Goflag)

## Getting started

To make things simple, all use can flow through a single static class `Flags`. It makes your command-line parsing life a simple breeze. Let's take a look at what it cna do.

To start, I have a program that I want to output a phrase a certain number of times. So I need two arguments, the first, `message`, is the phrase I want the program to output. The second, `t` is the number of times I want it repeated.

```C#
static void Main(string[] args)
{
    var message = Flag.String("message", "Hello, World!", "A message to display in the console.");
    var t = Flag.Int("t", 1, "The number of times to display a message");
    Flag.Parse();
    for (var i = 0; i < t; i++)
    {
        Console.WriteLine(message);
    }
}
```

Let's walk through a few scenarios while using this program, RepeaterExample.csproj (can be found in examples).

If I just run the following:
`.\RepeaterExample.exe`
I'm going to get the default values of both flags. Therefore, I will get the following output:

```
Hello, World!
```

Well, that's fun, isn't it? But what if you want to throw an error when the value isn't passed? That's up to you. You can handle that once you parse it. I will show you later how. But for now, let's keep at this example.

I can pass one or both arguments now to get some personalized behavior from my application.

```
.\RepeaterExample.exe -message "You are my density!" -t 5
```

That will give us, you guessed it, the message printed 5 times:

```
You are my density!
You are my density!
You are my density!
You are my density!
You are my density!
```

We don't have to stop there though, Flags also supports setting arguments with equals signs. It's equally ok to do this:

```
.\RepeaterExample.exe -message="You are my density!" -t=5
```

That will output the same as above:

```
You are my density!
You are my density!
You are my density!
You are my density!
You are my density!
```

Out of the box, Flags provides you with some common value types that are also found in Go: int, long, uint, ulong, string, duration (TimeSpan to us C# folk), float, decimal, and bool.

## The uniqueness of bool

Bool is special in this library because it also doubles as a switch. You don't have to pass the value of the bool in the command line.

Let's take this example:

```C#
static void Main(string[] args)
{
    // Create a new FlagSet so we can Parse our custom arguments below.
    var fs = Flag.NewFlagSet("BooleanExample", ErrorHandling.ExitOnError);
    var switchFlag = Flag.Bool("insert", false, "use the flag without a value as a bool as a switch");
    var equalsFlag = Flag.Bool("equals", false, "use equals(=) sign format as well");
    var intFlag = Flag.Bool("inty", false, "use 0 or 1 to indicate bool value");
    args = new[] { "-insert", "-equals=true ", "-inty", "1" };
    fs.Parse(args);
    Console.WriteLine($"-insert value: {switchFlag}");
    Console.WriteLine($"-equalsFlag value: {equalsFlag}");
    Console.WriteLine($"-intFlag value: {intFlag}");
}
```

When you want to parse altered arguments like in the above example, we need to first create a new FlagSet. This is done with the Flags.NewFlagSet method. After that, I can still use the static class Flags to set my flags. This is because Flags will store a reference to that new flag set whenever Flags.NewFlagSet is called. After defining three different ways to use booleans, you can call Parse(string[] args) directly on the FileSet. The printout will show all three flags being true:

```
-insert value: True
-equalsFlag value: True
-intFlag value: True
```

## Generics to the rescue!

Golang doesn't have generics ([yet](https://github.com/golang/go/issues/39716)). C# does, and it would be a shortcoming of the library to not include this. Let's see how to use generics and custom setters to get a little crazy.

Let's say we have console app and we want to pass json in as an argument (again, we're getting crazy).

```C#
static void Main(string[] args)
{
    args = new[] { "-rect", "{\"height\": 200, \"width\": 400}" };
    var fs = Flag.NewFlagSet("JsonParse", ErrorHandling.ContinueOnError);
    var rectFlag = new ValueFlag<Rectangle>("rect", default, "create a rectangle from json", ParseJson);
    fs.Var(rectFlag);
    fs.Parse(args);
    var rect = rectFlag.Value;
    // This works too: Rectangle rect = rectFlag;

    Console.WriteLine($"I have a rectangle with a height of {rect.Height} and a width of {rect.Width}");
}

static Rectangle ParseJson(Rectangle existingValue, string json)
{
    return JsonConvert.DeserializeObject<Rectangle>(json);
}
```

In this example, we're also shown a new way to create a flag. We can do it directly using a Flag<T> constructor. One of the constructors allows us to pass a function in that will set our flag's value with the text passed in from the command-line. The Func<Rectangle,string,Rectangle> here passes in the existing value of rectangle, the string value from the command-line, and expects a Rectangle to be returned. Once we've created a Flag, we then need our parser to track it, so we use the Var method to add it.

In our example, we're using the method ParseJson to parse the json using Newtonsoft.Json and returning the deserialized object. Once the object is returned, we can either implicitly use it's value, refer to the Value property, or create a new variable that references that value;

## Multiple flags

One excellent example in the go docs for this is using multiple flags to build an array. This is the reason for passing in the existing value into that Func above. Here is an example:

```C#
static void Main(string[] args)
{
    args = new[] { "-book", "The Hitchhiker's Guide to the Galaxy",
        "-book", "The Restaurant at the End of the Universe",
        "-book", "Life, the Universe and Everything"};
    var fs = Flag.NewFlagSet("ArrayBuilder", ErrorHandling.ContinueOnError);
    var bookFlag = new ValueFlag<string[]>("book", new string[0], "No b", ParseArray);
    fs.Var(bookFlag);
    fs.Parse(args);
    Console.WriteLine("Books:");
    foreach (var b in bookFlag.Value)
    {
        Console.WriteLine($"\t{b}");
    }
}

private static string[] ParseArray(string[] existingValue, string value)
{
    List<string> books;
    if (existingValue == null)
    {
        books = new List<string>();
    }
    else
    {
        books = existingValue.ToList();
    }
    books.Add(value);
    return books.ToArray();
}

```

Much of this is the same as the boolean example. We create a new Flag<T> and pass in a setter Func. This time, we're using the existing value to continue to add values to an array. You could further simplify this by just using a List<string> as your Flag type. With this example, we get the following output:

```
Books:
        The Hitchhiker's Guide to the Galaxy
        The Restaurant at the End of the Universe
        Life, the Universe and Everything
```

## Generating Help Text

This section came from a [request for help](https://github.com/mikeruhl/goflag/issues/1) from issues.  If you have a question, please don't hesitate to ask and I will add documentation on it.  Thanks!

You can generate some simple, nice help text by passing `--help` or `-help` at the command line.  By default, passing the help flag and no other flag will not result in an error, however, you can have it error out still by creating your own `FlagSet` and passing in a stricter error handling flag.  The following two examples show two outcomes from passing the `help` flag.


By default, we exit on error.  The error in this case would be that flags weren't supplied.  By just passing `-help`, we'll generate the help text then exit the program with a success code.
```csharp
static void Main(string[] args)
{
    var f = Flag.String("path", string.Empty, "path to file");
    var m = Flag.String("message", "hello world", "message to place in file");
    Flag.Parse();
    Console.WriteLine($"Path supplied: {f}");
    Console.WriteLine($"message: {m}");
}
```
if I run `.\TestApp.exe -help`
I will get:
```
Usage of C:\Users\miker\source\temp\TestApp\TestApp\bin\Debug\netcoreapp3.1\TestApp.dll:
  -message string
        message to place in file (default "hello world")
  -path string
        path to file
```

We can change the behavior and pass in different error handling, which will result in different outcome:
```csharp
static void Main(string[] args)
{
    var flagSet = new FlagSet("main", ErrorHandling.PanicOnError);
    var f = flagSet.String("path", string.Empty, "path to file");
    var m = flagSet.String("message", "hello world", "message to place in file");
    flagSet.Parse(args);
    Console.WriteLine($"Path supplied: {f}");
    Console.WriteLine($"message: {m}");
}
```
It will still print the help message but also throw an exception, which will result in a non-zero exit code `.\TestApp.exe -help`:
```
Usage of main:
  -message string
        message to place in file (default "hello world")
  -path string
        path to file
Unhandled exception. System.InvalidOperationException: flag: help requested
   at GoFlag.FlagSet.Parse(String[] arguments)
   at TestApp.Program.Main(String[] args) in C:\Users\miker\source\temp\TestApp\TestApp\Program.cs:line 15
```
You can see the reason it errrored (help requested), and the stacktrace.

## Error Handling

There are three levels of error handling available per the [original spec](https://golang.org/pkg/flag/#ErrorHandling) and those have been included here:

**ContinueOnError**

As it states, this will continue execution on error.  Errors occur when a flag cannot be parsed to the desired value type.  The response of the `FlagSet.Parse()` method is `ParsingError`, which will be null if the parsing was successful.  Once an error occurs, parsing stops and the `Parse()` method returns the error.

**ExitOnError (default)**

When using the static implementation of `Flag`, this is the error handling strategy you get.  It will exit on error.  Passing the help command line is considered an error.  If this is passed, the program will exit with a successful exit code (0) while printing help text.  If the error is due to command-line parsing, it will exit with an error exit code (2), printing help text along with the error message.

**PanicOnError**

Panic is analogous to exceptions in C#.  As such, that is what this does.  It will throw an `InvalidOperationException` with the message of the exception being the reason for the error.  If you wrap the `Parse()` method in a try/catch, this is the exception to catch for parsing errors or the help flag being passed.

If you want to override default behavior, you will need to create a FlagSet and not use the static implementation:

```csharp
var flagSet = new FlagSet("main", ErrorHandling.PanicOnError);
```

Not supplying all the defined flags at runtime will not result in an error.  The undefined flags will simply set to their default value.  Providing flags that aren't defined will result in an error.

## Redirecting Output

While using the static `Flag` class, output is directed to `Console.Error`.  This can be overridden by creating a `FlagSet` class and calling the `SetOutput` method.  An easy example of this would be to write to a text file:

```csharp
static void Main(string[] args)
{
    using (var writer = File.CreateText("output.txt"))
    {
        var flagSet = new FlagSet("default", ErrorHandling.ContinueOnError);
        flagSet.SetOutput(writer);
        var name = flagSet.String("name", "Mike", "The name of the person you wish to say hi to");
        var goodbye = flagSet.Bool("bye", false, "Say goodbye instead");
        var error = flagSet.Parse(args);
        Console.WriteLine($"{(goodbye ? "Goodbye" : "Hello")}, {name}");
    }
}
```

In this example, any error text will be output to a file called "output.txt".

### Credits

<a target="_blank" href="https://icons8.com/icons/set/empty-flag">Empty Flag icon</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>
