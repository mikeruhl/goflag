using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GoFlag.Enums;
using GoFlag.Interfaces;

[assembly: InternalsVisibleTo("Goflag.Tests")]
namespace GoFlag
{
    /// <summary>
    /// Allows one to define independent sets of flags, such as to implement subcommands in a command-line interface. The methods of FlagSet are analogous to the top-level functions for the command-line flag set.
    /// </summary>
    public class FlagSet
    {
        /// <summary>
        /// Create a flagset to add flags to and parse those flags based on command-line or customized arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        public FlagSet(string name, ErrorHandling errorHandling)
        {
            Name = name;
            ErrorHandling = errorHandling;
            _usage = DefaultUsage;
            _exit = Environment.Exit;
        }

        /// <summary>
        /// For unit tests, can pass method for Environment.Exit
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        /// <param name="environmentExit"></param>
        internal FlagSet(string name, ErrorHandling errorHandling, Action<int> environmentExit)
        {
            Name = name;
            ErrorHandling = errorHandling;
            _usage = DefaultUsage;
            _exit = environmentExit;
        }

        private Action _usage;
        private readonly Action<int> _exit;

        /// <summary>
        /// ErrHelp is the error returned if the -help or -h flag is invoked
        /// but no such flag is defined.
        /// </summary>
        public readonly ParsingError ErrHelp = new ParsingError("flag: help requested");


        /// <summary>
        /// Gets or sets the usage of the configured flags
        /// </summary>
        public Action Usage
        {
            get
            {
                if (_usage == null)
                {
                    return DefaultUsage;
                }
                return _usage;
            }
            set
            {
                _usage = value;
            }
        }

        /// <summary>
        /// Name returns the name of the flag set.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Parsed reports whether f.Parse has been called.
        /// </summary>
        public bool Parsed { get; private set; }

        /// <summary>
        /// Flags which have been set
        /// </summary>
        internal Dictionary<string, IFlag> Actual { get; set; }

        /// <summary>
        /// All flags
        /// </summary>
        internal Dictionary<string, IFlag> Formal { get; set; }

        private TextWriter _output;

        /// <summary>
        /// ErrorHandling returns the error handling behavior of the flag set.
        /// </summary>
        public ErrorHandling ErrorHandling { get; set; }

        /// <summary>
        /// TextWriter output of where parsing output will write to
        /// </summary>
        /// <returns></returns>
        public TextWriter Output()
        {
            if (_output == null)
            {
                return Console.Error;
            }
            return _output;
        }

        /// <summary>
        /// Init sets the name and error handling property for a flag set. By default, the zero FlagSet uses an empty name and the ContinueOnError error handling policy.  This is redundant of the constructor solely to preserve the footprint of the Go package.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorHandling"></param>
        public void Init(string name, ErrorHandling errorHandling)
        {
            Name = name;
            ErrorHandling = errorHandling;
        }

        internal Action DefaultUsage
        {
            get
            {
                return () =>
                {
                    if (Name == string.Empty)
                    {
                        Output().Write("Usage:\n");
                    }
                    else
                    {
                        Output().Write($"Usage of {Name}:\n");
                    }
                    PrintDefaults();

                };
            }
        }

        /// <summary>
        /// SetOutput sets the destination for usage and error messages. If output is nil, Console.Error is used.
        /// </summary>
        /// <param name="output"></param>
        public void SetOutput(TextWriter output)
        {
            _output = output;
        }

        /// <summary>
        /// Print the default usage to the configured output
        /// </summary>
        public void PrintDefaults()
        {
            VisitAll(flag =>
            {
                var s = new StringBuilder();
                s.Append($"  -{flag.Name}");
                (string name, string usage) = flag.UnquoteUsage();
                if (name.Length > 0)
                {
                    s.Append($" {name}");
                }
                //Boolean flags of one ASCII letter are so common we
                //treat them specially, putting their usage on teh same line.
                if (s.Length <= 4)//space, space, '-', 'x'.
                {
                    s.Append("\t");
                }
                else
                {
                    // Four spaces before the tab triggers good alignment
                    // for both 4- and 8-space tab stops.
                    s.Append("\n    \t");
                }
                s.Append(usage.Replace("\n", "\n    \t"));

                if (!IsZeroValue(flag, flag.DefaultValue))
                {
                    if (flag.TrackedType == typeof(string))
                    {
                        s.Append($" (default \"{flag.DefaultValue}\")");
                    }
                    else
                    {
                        s.Append($" (default {flag.DefaultValue})");
                    }
                }
                Output().WriteLine(s.ToString());
            });
        }

        /// <summary>
        /// NFlag returns the number of flags that have been set.
        /// </summary>
        /// <returns></returns>
        public int NFlag()
        {
            return Actual.Count;
        }

        /// <summary>
        /// Go calls Default values zero values, so we do this.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsZeroValue(IFlag flag, string value)
        {
            var typ = flag.TrackedType;
            if (typ.IsValueType)
            {
                var defValue = Activator.CreateInstance(typ);
                return value == defValue.ToString();
            }
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// VisitAll visits the flags in lexicographical order, calling fn for each. It visits all flags, even those not set.
        /// </summary>
        /// <param name="fn"></param>
        public void VisitAll(Action<IFlag> fn)
        {
            foreach (var f in SortFlags(Formal))
            {
                fn(f);
            }
        }

        /// <summary>
        /// Visit visits the flags in lexicographical order, calling fn for each. It visits only those flags that have been set.
        /// </summary>
        /// <param name="fn"></param>
        public void Visit(Action<IFlag> fn)
        {
            foreach (var f in SortFlags(Actual))
            {
                fn(f);
            }
        }

        private IFlag[] SortFlags(Dictionary<string, IFlag> flags)
        {
            if (flags == null)
                return new IFlag[0];
            var result = flags.Select(kvp => kvp.Value).OrderBy(v => v.Name).ToArray();
            return result;
        }

        /// <summary>
        /// Sets the value of the named flag.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException">Thrown when name does not exist as flag or when string value cannot be converted to desired type.</exception>
        public void Set(string name, string value)
        {
            var flagExists = Formal.TryGetValue(name, out var flag);
            if (!flagExists)
            {
                throw new ArgumentException("No such flag", nameof(name));
            }

            if (!flag.TrySetValue(value, out var err))
            {
                throw new ArgumentException($"Could not set value: {err}", nameof(value));
            }

            if (Actual == null)
                Actual = new Dictionary<string, IFlag>();

            if (Actual.ContainsKey(name))
            {
                Actual[name] = flag;
            }
            else
            {
                Actual.Add(name, flag);
            }
        }

        /// <summary>
        /// Arg returns the i'th argument. Arg(0) is the first remaining argument
        /// after flags have been processed. Arg returns an empty string if the
        /// requested element does not exist.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string Arg(int i)
        {
            if (i < 0 || i >= Args.Length)
            {
                return "";
            }
            return Args[i];
        }

        /// <summary>
        /// NArg is the number of arguments remaining after flags have been processed.
        /// </summary>
        /// <returns></returns>
        public int NArg()
        {
            return Args.Length;
        }

        /// <summary>
        /// Args returns the non-flag arguments.
        /// </summary>
        /// <returns></returns>
        public string[] Args { get; internal set; }

        /// <summary>
        /// Var defines a flag with the specified name and usage string. 
        /// This implementation differs slightly from the Go implementation because
        /// it takes advantage of generics.
        /// </summary>
        /// <typeparam name="T">Expected type output</typeparam>
        /// <param name="defaultValue">Value if no flag passed</param>
        /// <param name="name">Flag name</param>
        /// <param name="usage">Help text displayed</param>
        public ValueFlag<T> Var<T>(T defaultValue, string name, string usage)
        {
            return Var(defaultValue, name, usage, null);
        }

        /// <summary>
        /// Var defines a flag with the specified name and usage string. 
        /// This implementation differs slightly from the Go implementation because
        /// it takes advantage of generics.
        /// Pass setter to use a custom Func to set the value.
        /// </summary>
        /// <typeparam name="T">Expected type output</typeparam>
        /// <param name="defaultValue">Value if no flag passed</param>
        /// <param name="name">Flag name</param>
        /// <param name="usage">Help text displayed</param>
        /// <param name="setter">Sets value with existing value passed in.</param>
        /// <returns></returns>
        public ValueFlag<T> Var<T>(T defaultValue, string name, string usage, Func<T, string, T> setter)
        {
            var flag = new ValueFlag<T>(name, defaultValue, usage, setter);
            return Var(flag);

        }

        /// <summary>
        /// Pass a user-implemented IFlag directly to the FlagSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flag"></param>
        /// <returns></returns>
        public T Var<T>(T flag) where T : IFlag
        {
            if (Formal != null && Formal.ContainsKey(flag.Name))
            {
                var msg = Name == "" ? $"flag redefined: {flag.Name}" : $"{Name} flag redefined: {flag.Name}";
                Output().WriteLine(msg);
                throw new InvalidOperationException(msg);
            }
            if (Formal == null)
                Formal = new Dictionary<string, IFlag>();
            Formal.Add(flag.Name, flag);
            return flag;
        }

        internal ParsingError FailF(string format, params object[] args)
        {
            var error = new ParsingError(string.Format(format, args));
            Output().Write($"{error}\n");
            Usage();
            return error;
        }

        internal (bool, ParsingError) ParseOne()
        {
            if (Args.Length == 0)
            {
                return (false, null);
            }
            var s = Args[0];
            if (s.Length < 2 || s[0] != '-')
            {
                return (false, null);
            }
            var numMinuses = 1;
            if (s[1] == '-')
            {
                numMinuses++;
                if (s.Length == 2) // "--" terminates the flags
                {
                    Args = Args.Skip(1).ToArray();
                    return (false, null);
                }
            }
            var name = s.Substring(numMinuses);
            if (name.Length == 0 || name[0] == '-' || name[0] == '=')
            {
                return (false, FailF($"bad flag syntax: {0}", s));
            }

            //it's a flag, does it have an argument?
            Args = Args.Skip(1).ToArray();
            var hasValue = false;
            var value = "";
            for (var i = 1; i < name.Length; i++)
            {
                if (name[i] == '=')
                {
                    value = name.Substring(i + 1);
                    hasValue = true;
                    name = name.Substring(0, i);
                }
            }
            IFlag flag = null;
            var alreadyThere = Formal != null && Formal.TryGetValue(name, out flag);
            if (!alreadyThere)
            {
                if (name.Equals("help", StringComparison.CurrentCultureIgnoreCase)
                    || name.Equals("h", StringComparison.CurrentCultureIgnoreCase))
                {
                    Usage();
                    return (false, ErrHelp);
                }
                return (false, FailF("flag provided but not defined: -{0}", name));
            }

            if (flag?.TrackedType == typeof(bool)) //special case, doesn't need an arg
            {
                if (hasValue)
                {
                    if (!flag.TrySetValue(value, out var err))
                    {
                        if (value.Equals("0") && !flag.TrySetValue("false", out err)) //account for 0 and 1 values (inherint in Go, not in C#)
                        {
                            return (false, FailF("invalid boolean value \"{0}\" for -{1}: {2}", value, name, err));
                        }
                        else if (value.Equals("1") && !flag.TrySetValue("true", out err))
                        {
                            return (false, FailF("invalid boolean value \"{0}\" for -{1}: {2}", value, name, err));
                        }
                        else
                        {
                            return (false, FailF("invalid boolean value \"{0}\" for -{1}: {2}", value, name, err));
                        }

                    }
                }
                else
                {
                    if (!flag.TrySetValue("true", out var err))
                    {
                        return (false, FailF("invalid boolean flag {0}: {1}", name, err));
                    }
                }
            }
            else
            {
                //It must have a value, which might be the next argument.
                if (!hasValue && Args.Length > 0)
                {
                    //value is the next arg
                    hasValue = true;
                    value = Args[0];
                    Args = Args.Skip(1).ToArray();
                }
                if (!hasValue)
                {
                    return (false, FailF("flag needs an argument: -{0}", name));
                }
                if (!flag.TrySetValue(value, out var err))
                {
                    return (false, FailF("invalid value \"{0}\" for flag -{1}: {2}", value, name, err));
                }
            }
            if (Actual == null)
                Actual = new Dictionary<string, IFlag>();

            if (Actual.ContainsKey(name))
                Actual[name] = flag;
            else
                Actual.Add(name, flag);

            return (true, null);
        }

        /// <summary>
        /// Parse parses the command-line flags from os.Args[1:]. Must be called after all flags are defined and before flags are accessed by the program.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ParsingError Parse(string[] arguments)
        {
            Parsed = true;
            Args = arguments;
            while (true)
            {
                (var seen, var error) = ParseOne();
                if (seen) { continue; }
                if (error == null) { break; }
                switch (ErrorHandling)
                {
                    case ErrorHandling.ContinueOnError:
                        return error;
                    case ErrorHandling.ExitOnError:
                        if (error == ErrHelp)
                        {
                            _exit(0);
                        }
                        else
                        {
                            _exit(2);
                        }
                        return error; //just here for testing.
                    case ErrorHandling.PanicOnError:
                        throw new InvalidOperationException(error.ToString());
                }
            }
            return null;
        }

        /// <summary>
        /// Bool defines a bool flag with specified name, default value, and usage string. The return value is a Flag&lt;bool&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<bool> Bool(string flagName, bool defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Int defines an int flag with specified name, default value, and usage string. The return value is a Flag&lt;int&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<int> Int(string flagName, int defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Uint defines a uint flag with specified name, default value, and usage string. The return value is a Flag&lt;uint&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<uint> Uint(string flagName, uint defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// String defines a string flag with specified name, default value, and usage string. The return value is a Flag&lt;string&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<string> String(string flagName, string defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Int64 defines an int64 flag with specified name, default value, and usage string. The return value is a Flag&lt;int64&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<long> Int64(string flagName, long defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Uint64 defines a long flag with specified name, default value, and usage string. The return value is a Flag&lt;long&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<ulong> Uint64(string flagName, ulong defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Float defines a float flag with specified name, default value, and usage string. The return value is a Flag&lt;float&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<float> Float(string flagName, float defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Decimal defines a decimal flag with specified name, default value, and usage string. The return value is a Flag&lt;decimal&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<decimal> Decimal(string flagName, decimal defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }

        /// <summary>
        /// Duration defines a TimeSpan flag with specified name, default value, and usage string. The return value is a Flag&lt;TimeSpan&gt; variable that stores the value of the flag.
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <returns></returns>
        public ValueFlag<TimeSpan> Duration(string flagName, TimeSpan defaultValue, string usage)
        {
            return Var(defaultValue, flagName, usage);
        }
    }
}
