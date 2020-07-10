using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoFlag.Enums;
using GoFlag.Interfaces;
using Goflag.Tests;
using Xunit;

namespace GoFlag.Tests
{
    public class FlagTests
    {
        private void ResetForTesting(Action usage)
        {
            var commandLine = Flag.NewFlagSet(Environment.GetCommandLineArgs()[0], ErrorHandling.ContinueOnError);
            commandLine.Usage = usage;
            Flag.Usage = usage;
        }
        //https://github.com/golang/go/blob/master/src/flag/flag_test.go
        public FlagTests()
        {
            var args = Environment.GetCommandLineArgs();
            Flag.NewFlagSet(args.First(), ErrorHandling.ContinueOnError);
        }

        [Fact]
        public void TestEverything()
        {
            Flag.Bool("test_bool", false, "bool value");
            Flag.Int("test_int", 0, "int value");
            Flag.Int64("test_int64", 0, "int64 value");
            Flag.Uint("test_uint", 0, "uint value");
            Flag.Uint64("test_uint64", 0, "uint64 value");
            Flag.String("test_string", "0", "string value");
            Flag.Float("test_float64", 0, "float64 value");
            Flag.Duration("test_duration", TimeSpan.FromSeconds(0), "time.Duration value");

            var desired = "0";
            var m = new Dictionary<string, IFlag>();
            var visitor = new Action<IFlag>(f =>
            {

                if (f.Name.Length > 5 && f.Name.Substring(0, 5) == "test_")
                {
                    m[f.Name] = f;
                    var ok = false;
                    if (f.ToString() == desired)
                    {
                        ok = true;
                    }
                    else if (f.Name == "test_bool" && f.ToString() == (desired == "1" ? "True" : "False"))
                    {
                        ok = true;

                    }
                    else if (f.Name == "test_duration" && f.ToString() == TimeSpan.Parse(desired == "1" ? "00:00:01" : "00:00:00").ToString())
                    {
                        ok = true;
                    }

                    Assert.True(ok, $"Visit: bad value {f} for {f.Name}");
                }

            });
            Flag.VisitAll(visitor);

            Assert.True(8 == m.Count, "VisitAll misses some flags");

            m = new Dictionary<string, IFlag>();

            Flag.Visit(visitor);
            Assert.True(0 == m.Count, "Visit sees unset flags");


            // Now set all flags
            Flag.Set("test_bool", "true");
            Flag.Set("test_int", "1");
            Flag.Set("test_int64", "1");
            Flag.Set("test_uint", "1");
            Flag.Set("test_uint64", "1");
            Flag.Set("test_string", "1");
            Flag.Set("test_float64", "1");
            Flag.Set("test_duration", "00:00:01");
            desired = "1";
            Flag.Visit(visitor);
            Assert.True(8 == m.Count, "Visit fails after set");

            // Now test they're visited in sort order.
            var flagNames = new List<string>();
            Flag.Visit(f => { flagNames.Add(f.Name); });
            var sorted = flagNames.ToArray();
            Array.Sort(sorted);

            for (var i = 0; i < flagNames.Count; i++)
            {
                Assert.True(flagNames[i] == sorted[i], $"flag names not sorted: {string.Join(' ', flagNames)}");
            }
        }

        [Fact]
        public void TestGet()
        {
            Flag.Bool("test_bool", true, "bool value");
            Flag.Int("test_int", 1, "int value");
            Flag.Int64("test_int64", 2, "int64 value");
            Flag.Uint("test_uint", 3, "uint value");
            Flag.Uint64("test_uint64", 4, "uint64 value");
            Flag.String("test_string", "5", "string value");
            Flag.Float("test_float64", 6, "float64 value");
            Flag.Duration("test_duration", TimeSpan.FromSeconds(7), "time.Duration value");

            var visitor = new Action<IFlag>(f =>
            {
                var ok = false;
                if (f.Name.Length > 5 && f.Name.Substring(0, 5) == "test_")
                {
                    if (f.TrackedType == typeof(bool))
                    {
                        var b = f as ValueFlag<bool>;
                        ok = b;
                    }
                    else if (f.TrackedType == typeof(int))
                    {
                        var b = f as ValueFlag<int>;
                        ok = b == 1;
                    }
                    else if (f.TrackedType == typeof(long))
                    {
                        var b = f as ValueFlag<long>;
                        ok = b == 2;
                    }
                    else if (f.TrackedType == typeof(uint))
                    {
                        var b = f as ValueFlag<uint>;
                        ok = b == 3;
                    }
                    else if (f.TrackedType == typeof(ulong))
                    {
                        var b = f as ValueFlag<ulong>;
                        ok = b == 4;
                    }
                    else if (f.TrackedType == typeof(string))
                    {
                        var b = f as ValueFlag<string>;
                        ok = b == "5";
                    }
                    else if (f.TrackedType == typeof(float))
                    {
                        var b = f as ValueFlag<float>;
                        ok = b == 6;
                    }
                    else if (f.TrackedType == typeof(TimeSpan))
                    {
                        var b = f as ValueFlag<TimeSpan>;
                        ok = b == TimeSpan.FromSeconds(7);
                    }

                    Assert.True(ok, $"Visit: bad value {f.TrackedType.Name}({f}) for {f.Name}");
                }
            });
            Flag.VisitAll(visitor);
        }

        [Fact]
        public void TestUsage()
        {
            var called = false;
            ResetForTesting(() => called = true);
            Assert.True(Flag.CommandLine.Parse(new[] { "-x" }) != null, "parse did not fail for unknown flag");
            Assert.True(called, "did not call Usage for unknown flag");
        }
        const double MinNormal = 2.2250738585072014E-308d;

        private void PrivateTestParse(FlagSet f)
        {
            Assert.False(f.Parsed, "f.Parse = true before Parse");

            var boolFlag = f.Bool("bool", false, "bool value");
            var bool2Flag = f.Bool("bool2", false, "bool2 value");
            var intFlag = f.Int("int", 0, "int value");
            var int64Flag = f.Int64("int64", 0, "int64 value");
            var uintFlag = f.Uint("uint", 0, "uint value");
            var uint64Flag = f.Uint64("uint64", 0, "uint64 value");
            var stringFlag = f.String("string", "0", "string value");
            var float64Flag = f.Float("float64", 0, "float64 value");
            var durationFlag = f.Duration("duration", 5 * TimeSpan.FromSeconds(1), "time.Duration value");
            var extra = "one-extra-argument";

            var args = new[]{
                "-bool",
        "-bool2=true",
        "--int", "22",
        "--int64", "0x23",
        "-uint", "24",
        "--uint64", "25",
        "-string", "hello",
        "-float64", "2718e28",
        "-duration", "00:02:00",
        extra};

            var error = f.Parse(args);
            Assert.True(error == null, $"error parsing: {error}");
            Assert.True(f.Parsed, "f.Parse() = false after Parse");
            Assert.True(boolFlag, $"bool flag should be true, but is {boolFlag}");
            Assert.True(bool2Flag, $"bool2 flag should be true, is {bool2Flag}");
            Assert.True(intFlag == 22, $"int flag should be 22, is {intFlag}");
            Assert.True(int64Flag == 0x23, $"int64 flag should be 0x23, is {int64Flag}");
            Assert.True(uintFlag == 24, $"uint flag should be 24, is {uintFlag}");
            Assert.True(uint64Flag == 25, $"uint64 flag should be 25, is {uint64Flag}");
            Assert.True(stringFlag == "hello", $"string flag should be `hello`, is {stringFlag}");
            Assert.True(AlmostEqual2sComplement(float64Flag, (float)2718e28, 1), $"float64 flag should be 2718e28, is {float64Flag}");
            Assert.True(durationFlag == TimeSpan.FromMinutes(2), $"duration flag should be 2m, is {durationFlag}");

            Assert.True(f.Args.Length == 1, $"expected one argument, got {f.Args.Length}");
            Assert.True(f.Args[0] == extra, $"expected argument \"{extra}\" got {f.Args[0]}");
        }

        [Fact]
        public void TestParse()
        {
            ResetForTesting(() => Assert.False(true, "bad parse"));
            PrivateTestParse(Flag.CommandLine);
        }
        [Fact]
        public void TestFlagSetParse()
        {
            //var flags = new FlagSet("test", Enums.ErrorHandling.ContinueOnError);
            PrivateTestParse(Flag.NewFlagSet("test", ErrorHandling.ContinueOnError));
        }

        [Fact]
        public void TestUserDefined()
        {
            var flags = Flag.NewFlagSet("test", ErrorHandling.ContinueOnError);
            var f = flags.Var(new string[0], "v", "usage", (existing, newVal) =>
            {
                var listOf = new List<string>();
                if (existing != null)
                    listOf.AddRange(existing);
                listOf.Add(newVal);
                return listOf.ToArray();
            });
            var err = flags.Parse(new[] { "-v", "1", "-v", "2", "-v=3" });
            Assert.True(err == null, err?.ToString());
            Assert.True(f.Value.Length == 3, $"expected 3 args, got {f.Value.Length}");
            var expect = string.Join(' ', new[] { "1", "2", "3" });
            Assert.True(Enumerable.SequenceEqual(f.Value, new[] { "1", "2", "3" }), $"expected value \"{expect}\" got \"{string.Join(' ', f)}\"");
        }

        [Fact]
        public void TestUserDefinedForCommandLine()
        {
            var help = "HELP";
            string result = null;
            ResetForTesting(() => result = help);
            Flag.Usage();
            Assert.True(help == result, $"got \"{result}\"; expected\"{help}\"");
        }

        [Fact]
        public void TestUserDefinedFlag()
        {
            var flags = Flag.NewFlagSet("test", ErrorHandling.ContinueOnError);
            var b = flags.Var(new CustomFlag());
            var err = flags.Parse(new[] { "-b", "-b", "-b", "-b=true", "-b=false", "-b", "barg", "-b" });
            Assert.True(err == null, err?.ToString());
            Assert.True(6 == b.Counter, $"counter value unexpeced, got: {b.Counter}, wanted: 6");
        }

        [Fact]
        public void TestSetOutput()
        {
            var flags = new FlagSet("test", ErrorHandling.ContinueOnError);
            var writer = new StringWriter();
            flags.SetOutput(writer);
            flags.Parse(new[] { "-unknown" });
            var output = writer.ToString();
            Assert.True(output.Contains("-unknown"), $"expected output mentioning unknown; got \"{output}\"");
        }

        /// <summary>
        /// Note from Go Source:
        /// This tests that one can reset the flags. This still works but not well, and is
        /// superseded by FlagSet.
        /// </summary>
        [Fact]
        public void TestChangingArgs()
        {
            ResetForTesting(() => Assert.True(false, "bad parse"));
            //the go src sets os.Args, which is the command line arguments, something that is read-only in .net
            var args = new[] { "cmd", "-before", "subcmd", "-after", "args" };
            var before = Flag.Bool("before", false, "");
            var err = Flag.CommandLine.Parse(args.Skip(1).ToArray());
            Assert.True(err == null, err?.ToString());

            var cmd = Flag.Arg(0);
            var after = Flag.Bool("after", false, "");
            args = Flag.Args();
            Flag.CommandLine.Parse(args.Skip(1).ToArray());
            args = Flag.Args();
            Assert.True(before && cmd == "subcmd" && after && args[0] == "args", $"expected true subcmd true [args] got {before} {cmd} {after} {string.Join(',', args)}");
        }

        [Fact]
        public void TestHelp()
        {
            var helpCalled = false;
            var fs = Flag.NewFlagSet("help test", ErrorHandling.ContinueOnError);
            fs.Usage = () => { helpCalled = true; };
            var flag = fs.Bool("flag", false, "regular flag");
            var err = fs.Parse(new[] { "-flag=true" });
            Assert.True(err == null, $"expected no error, got {err}");
            Assert.True(flag, "flag was not set by -flag");
            Assert.False(helpCalled, "help called for regular flag");

            err = fs.Parse(new[] { "-help" });
            Assert.True(err != null, "error expected");
            Assert.True(err.Message == fs.ErrHelp.Message, $"expected ErrHelp; got {err}");
            Assert.True(helpCalled, "help was not called");

            var help = fs.Bool("help", false, "help flag");
            helpCalled = false;
            err = fs.Parse(new[] { "-help" });
            Assert.True(err == null, $"expected no error for defined -help; got {err}");
            Assert.False(helpCalled, "help was called; should not have been for defined help flag");
        }

        [Fact]
        public void TestUsageOutput()
        {
            ResetForTesting(Flag.DefaultUsage);
            var sw = new StringWriter();
            Flag.CommandLine.SetOutput(sw);
            Assert.True(Flag.CommandLine.Output() is StringWriter, "expected Output is not Stringwriter");
            var args = new[] { "app", "-i=1", "-unknown" };
            //In the go tests, they set os.Args, but you can't do that in .NET so we have to cheat.
            Flag.CommandLine.Init("app", ErrorHandling.ContinueOnError);
            Flag.CommandLine.Parse(args.Skip(1).ToArray());
            var want = "flag provided but not defined: -i\nUsage of app:\n";
            var result = sw.ToString();
            Assert.True(want == result, $"output = \"{sw}\", want \"{want}\"");
        }

        [Fact]
        public void TestGetters()
        {
            var expectedName = "flag set";
            var expectedErrorHandling = ErrorHandling.ContinueOnError;
            var expectedOutput = Console.Error;

            var fs = Flag.NewFlagSet(expectedName, expectedErrorHandling);

            Assert.True(expectedName == fs.Name, $"unexpected name: got {fs.Name}, expected {expectedName}");
            Assert.True(expectedErrorHandling == fs.ErrorHandling, $"unexpected ErrorHandling: got {fs.ErrorHandling}, expected {expectedErrorHandling}");
            Assert.True(expectedOutput == fs.Output(), $"unexpected output: got {fs.Output()}, expected {expectedOutput}");

            expectedName = "gopher";
            expectedErrorHandling = ErrorHandling.ExitOnError;
            expectedOutput = Console.Out;
            fs.Init(expectedName, expectedErrorHandling);
            fs.SetOutput(Console.Out);
            Assert.True(expectedName == fs.Name, $"unexpected name: got {fs.Name}, expected {expectedName}");
            Assert.True(expectedErrorHandling == fs.ErrorHandling, $"unexpected ErrorHandling: got {fs.ErrorHandling}, expected {expectedErrorHandling}");
            Assert.True(expectedOutput == fs.Output(), $"unexpected output: got {fs.Output()}, expected {expectedOutput}");
        }

        [Theory]
        [InlineData("bool")]
        [InlineData("int")]
        [InlineData("int64")]
        [InlineData("uint")]
        [InlineData("uint64")]
        [InlineData("float64")]
        [InlineData("duration")]
        public void TestParseError(string typ)
        {
            var fs = Flag.NewFlagSet("parse error test", ErrorHandling.ContinueOnError);
            var tw = new StringWriter();
            fs.SetOutput(tw);
            var b = fs.Bool("bool", false, "");
            var i = fs.Int("int", 0, "");
            var i6 = fs.Int64("int64", 0, "");
            fs.Uint("uint", 0, "");
            fs.Uint64("uint64", 0, "");
            fs.Float("float64", 0, "");
            fs.Duration("duration", TimeSpan.FromSeconds(0), "");
            //Strings cannot give errors
            var args = new string[] { $"-{typ}=x" };
            var err = fs.Parse(args); // x is not a valid setting for any flag.
            Assert.True(err != null, $"Parse({string.Join(',', args)})={err}; expected parse error");
            Assert.True(err.Message.Contains("invalid") && err.Message.Contains("parse error"),
                $"Parse({string.Join(',', args)})={err}; expected parse error");
        }



        [Theory]
        [InlineData("-int=123456789012345678901")]
        [InlineData("-int64=123456789012345678901")]
        [InlineData("-uint=123456789012345678901")]
        [InlineData("-uint64=123456789012345678901")]
        //[InlineData("-float64=1e1000")] //not a valid comparison in c# since exceded range just results in Infinity.
        public void TestRangeError(string arg)
        {
            var fs = Flag.NewFlagSet("parse error test", ErrorHandling.ContinueOnError);
            fs.SetOutput(new StringWriter());
            fs.Int("int", 0, "");
            fs.Int64("int64", 0, "");
            fs.Uint("uint", 0, "");
            fs.Uint64("uint64", 0, "");
            fs.Float("float64", 0, "");
            // Strings cannot give errors, and bools and durations do not return strconv.NumError.
            var err = fs.Parse(new[] { arg });
            Assert.True(err != null, $"Parse(%q)=%v; expected range error");
            Assert.True(err.Message.Contains("invalid") && err.Message.Contains("value out of range"), $"Parse(\"{arg}\")={err}; expected range error");
        }

        [Theory]
        [InlineData("-h", 0)]
        [InlineData("-help", 0)]
        [InlineData("-undefined", 2)]

        public void TestExitCode(string flag, int expectedExit)
        {
            var got = -1;
            var exit = new Action<int>(f =>
            {
                got = f;
            });
            var fs = Flag.NewFlagSet("test", ErrorHandling.ExitOnError, exit);

            var err = fs.Parse(new[] { flag });
            Assert.True(expectedExit == got, $"unexpected exit code for test case {flag} \n: got {got}, expect {expectedExit}");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("-help")]
        [InlineData("-undefined")]
        public void TestException(string flag)
        {
            var fs = Flag.NewFlagSet("test", ErrorHandling.PanicOnError);
            Assert.Throws<InvalidOperationException>(() => fs.Parse(new[] { flag }));
        }

        [Fact]
        public void ParsingErrorSetsMessage()
        {
            var expected = "test message";
            var error = new ParsingError(expected);
            Assert.True(expected == error.Message, "ParsingError constructor should set message");
        }

        [Fact]
        public void TestUsageStrings()
        {
            var fs = Flag.NewFlagSet("test", ErrorHandling.PanicOnError);
            fs.String("book", "On Tyranny", "name a book to read");
            var writer = new StringWriter();
            fs.SetOutput(writer);
            fs.PrintDefaults();
            var got = writer.ToString();
            var want = "  -book string\n    \tname a book to read (default \"On Tyranny\")\r\n";
            Assert.True(got == want, "Usage Message is not expected result");
        }

        [Fact]
        public void TestUsageBool()
        {
            var fs = Flag.NewFlagSet("test", ErrorHandling.PanicOnError);
            fs.Bool("watch", false, "watch tv");
            var writer = new StringWriter();
            fs.SetOutput(writer);
            fs.PrintDefaults();
            var got = writer.ToString();
            var want = "  -watch\n    \twatch tv\r\n";
            Assert.True(got == want, "Usage Message is not expected result");
        }

        [Fact]
        public void TestUsageInt()
        {
            var fs = Flag.NewFlagSet("test", ErrorHandling.PanicOnError);
            fs.Int("many", 1337, "pass number of books");
            var writer = new StringWriter();
            fs.SetOutput(writer);
            fs.PrintDefaults();
            var got = writer.ToString();
            var want = "  -many int\n    \tpass number of books (default 1337)\r\n";
            Assert.True(got == want, "Usage Message is not expected result");
        }

        [Theory]
        [InlineData("book", "Robinson Crusoe", "pass any `great work`", "great work", "pass any great work")]
        [InlineData("latop", "Macbook Prod", "denote which model of `laptop` you are using", "laptop", "denote which model of laptop you are using")]
        [InlineData("boring", "tunnel", "can you dig it?", "string", "can you dig it?")]
        public void TestUnquoteUsageString(string name, string defaultValue, string usage, string expectedName, string expectedUsage)
        {
            var f = new ValueFlag<string>(name, defaultValue, usage);
            var (n, u) = f.UnquoteUsage();
            Assert.True(expectedName == n, $"expected name does not match, got: {n}, wanted: {expectedName}");
            Assert.True(expectedUsage == u, $"expected usage does not match, got: {u}, wanted: {expectedUsage}");
        }
        [Theory]
        [InlineData("flatEarth", false, "determine if the earth is `flat`", "flat", "determine if the earth is flat")]
        [InlineData("moonCheese", true, "build a world where the moon is made of cheese", "", "build a world where the moon is made of cheese")]
        public void TestUnquoteUsageBool(string name, bool defaultValue, string usage, string expectedName, string expectedUsage)
        {
            var f = new ValueFlag<bool>(name, defaultValue, usage);
            var (n, u) = f.UnquoteUsage();
            Assert.True(expectedName == n, $"expected name does not match, got: {n}, wanted: {expectedName}");
            Assert.True(expectedUsage == u, $"expected usage does not match, got: {u}, wanted: {expectedUsage}");
        }

        private static bool AlmostEqual2sComplement(float a, float b, int maxDeltaBits)
        {
            int aInt = BitConverter.ToInt32(BitConverter.GetBytes(a), 0);
            if (aInt < 0)
                aInt = Int32.MinValue - aInt;  // Int32.MinValue = 0x80000000

            int bInt = BitConverter.ToInt32(BitConverter.GetBytes(b), 0);
            if (bInt < 0)
                bInt = Int32.MinValue - bInt;

            int intDiff = Math.Abs(aInt - bInt);
            return intDiff <= (1 << maxDeltaBits);
        }
    }
}

