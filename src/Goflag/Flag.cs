using System;
using System.Linq;
using GoFlag.Enums;
using GoFlag.Interfaces;

namespace GoFlag
{
    /// <summary>
    /// Wrapper enabling easy access to command-line flags
    /// </summary>
    public static class Flag
    {
        private static FlagSet _commandLine = new FlagSet(Environment.GetCommandLineArgs().First(), Enums.ErrorHandling.ExitOnError);

        /// <summary>
        /// Bool defines a bool flag with specified name, default value, and usage string. The return value is a Flag&lt;bool&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<bool> Bool(string flagName, bool defaultValue, string usage)
        {
            return _commandLine.Bool(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Int defines an int flag with specified name, default value, and usage string. The return value is a Flag&lt;int&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<int> Int(string flagName, int defaultValue, string usage)
        {
            return _commandLine.Int(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Uint defines a uint flag with specified name, default value, and usage string. The return value is a Flag&lt;uint&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<uint> Uint(string flagName, ushort defaultValue, string usage)
        {
            return _commandLine.Uint(flagName, defaultValue, usage);
        }

        /// <summary>
        /// String defines a string flag with specified name, default value, and usage string. The return value is a Flag&lt;string&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<string> String(string flagName, string defaultValue, string usage)
        {
            return _commandLine.String(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Int64 defines an int64 flag with specified name, default value, and usage string. The return value is a Flag&lt;int64&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<long> Int64(string flagName, long defaultValue, string usage)
        {
            return _commandLine.Int64(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Uint64 defines a long flag with specified name, default value, and usage string. The return value is a Flag&lt;long&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<ulong> Uint64(string flagName, ulong defaultValue, string usage)
        {
            return _commandLine.Uint64(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Float defines a float flag with specified name, default value, and usage string. The return value is a Flag&lt;float&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<float> Float(string flagName, float defaultValue, string usage)
        {
            return _commandLine.Float(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Decimal defines a decimal flag with specified name, default value, and usage string. The return value is a Flag&lt;decimal&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<decimal> Decimal(string flagName, decimal defaultValue, string usage)
        {
            return _commandLine.Decimal(flagName, defaultValue, usage);
        }

        /// <summary>
        /// Duration defines a TimeSpan flag with specified name, default value, and usage string. The return value is a Flag&lt;TimeSpan&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public static ValueFlag<TimeSpan> Duration(string flagName, TimeSpan defaultValue, string usage)
        {
            return _commandLine.Duration(flagName, defaultValue, usage);
        }


        /// <summary>
        /// The current FlagSet that Flags is referencing
        /// </summary>
        public static FlagSet CommandLine => _commandLine;


        /// <summary>
        /// Var defines a flag with the specified name and usage string. 
        /// This implementation differs slightly from the Go implementation because
        /// it takes advantage of generics.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="usage"></param>
        /// <param name="defaultValue"></param>
        public static ValueFlag<T> Var<T>(string name, T defaultValue, string usage)
        {
            return _commandLine.Var(defaultValue, name, usage);
        }

        /// <summary>
        /// NewFlagSet returns a new, empty flag set with the specified name and
        /// error handling property. If the name is not empty, it will be printed
        /// in the default usage message and in error messages.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        /// <returns></returns>
        public static FlagSet NewFlagSet(string name, ErrorHandling errorHandling)
        {
            _commandLine = new FlagSet(name, errorHandling);
            return _commandLine;
        }

        /// <summary>
        /// For unit tests, replaces Environment.Exit with action for testing purposes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        /// <param name="environmentExit"></param>
        /// <returns></returns>
        internal static FlagSet NewFlagSet(string name, ErrorHandling errorHandling, Action<int> environmentExit)
        {
            _commandLine = new FlagSet(name, errorHandling, environmentExit);
            return _commandLine;
        }

        /// <summary>
        /// Init sets the name and error handling property for a flag set.
        /// By default, the zero FlagSet uses an empty name and the
        /// ContinueOnError error handling policy.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        /// <returns></returns>
        public static FlagSet Init(string name, ErrorHandling errorHandling)
        {
            _commandLine.Name = name;
            _commandLine.ErrorHandling = errorHandling;
            return _commandLine;
        }

        /// <summary>
        /// Returns bool indicating if parsing has taken place on the current Flagset
        /// </summary>
        public static bool Parsed => _commandLine.Parsed;

        /// <summary>
        /// Execute an Action against all flags
        /// </summary>
        /// <param name="fn"></param>
        public static void VisitAll(Action<IFlag> fn)
        {
            _commandLine.VisitAll(fn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        public static void Visit(Action<IFlag> fn)
        {
            _commandLine.Visit(fn);
        }

        /// <summary>
        /// Sets the value of the named flag.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException">Thrown when name does not exist as flag or when string value cannot be converted to desired type.</exception>
        public static void Set(string name, string value)
        {
            _commandLine.Set(name, value);
        }

        /// <summary>
        /// Lookup a flag which has been set
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFlag Lookup(string name)
        {
            return _commandLine.Formal[name];
        }
        /// <summary>
        /// PrintDefaults prints, to standard error unless configured otherwise,
        /// a usage message showing the default settings of all defined
        /// command-line flags.
        /// </summary>
        /// <remarks>
        /// For an integer valued flag x, the default output has the form
        /// 	-x int
        ///         usage-message-for-x (default 7)
        /// The usage message will appear on a separate line for anything but
        /// a bool flag with a one-byte name.For bool flags, the type is
        /// omitted and if the flag name is one byte the usage message appears
        /// on the same line.The parenthetical default is omitted if the
        /// default is the zero value for the type. The listed type, here int,
        /// can be changed by placing a back-quoted name in the flag's usage
        /// string; the first such item in the message is taken to be a parameter
        /// name to show in the message and the back quotes are stripped from
        /// the message when displayed.For instance, given
        /// 
        ///     flag.String("I", "", "search `directory` for include files")
        /// the output will be
        /// 	-I directory
        /// 
        ///         search directory for include files.
        /// To change the destination for flag messages, call CommandLine.SetOutput.
        /// </remarks>
        public static void PrintDefaults()
        {
            _commandLine.PrintDefaults();
        }

        /// <summary>
        /// Usage prints a usage message documenting all defined command-line flags
        /// to CommandLine's output, which by default is os.Stderr.
        /// It is called when an error occurs while parsing flags.
        /// The function is a variable that may be changed to point to a custom function.
        /// By default it prints a simple header and calls PrintDefaults; for details about the
        /// format of the output and how to control it, see the documentation for PrintDefaults.
        /// Custom usage functions may choose to exit the program; by default exiting
        /// happens anyway as the command line's error handling strategy is set to
        /// ExitOnError.
        /// </summary>
        public static Action Usage { get; set; } = DefaultUsage;

        /// <summary>
        /// Writes default usage of configured flags to output
        /// </summary>
        public static Action DefaultUsage => () =>
        {
            _commandLine.Output().Write($"Usage of {_commandLine.Name}:\n");
            PrintDefaults();
        };

        /// <summary>
        /// NFlag returns the number of command-line flags that have been set.
        /// </summary>
        /// <returns></returns>
        public static int NFlag()
        {
            return _commandLine.Actual.Count;
        }

        /// <summary>
        /// NArg is the number of arguments remaining after flags have been processed.
        /// </summary>
        /// <returns></returns>
        public static int NArg()
        {
            return _commandLine.Args.Length;
        }

        /// <summary>
        /// Arg returns the i'th command-line argument. Arg(0) is the first remaining argument
        /// after flags have been processed.Arg returns an empty string if the
        /// requested element does not exist.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string Arg(int i)
        {
            return _commandLine.Arg(i);
        }

        /// <summary>
        /// Args returns the non-flag arguments.
        /// </summary>
        /// <returns></returns>
        public static string[] Args()
        {
            return _commandLine.Args;
        }

        /// <summary>
        /// Parse parses the command-line flags from os.Args[1:]. Must be called
        /// after all flags are defined and before flags are accessed by the program.
        /// </summary>
        public static void Parse()
        {
            // Ignore errors; CommandLine is set for ExitOnError.
            _commandLine.Parse(Environment.GetCommandLineArgs().Skip(1).ToArray());
        }
    }
}
