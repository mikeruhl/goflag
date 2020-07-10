using System;
using GoFlag;
using GoFlag.Interfaces;

namespace Goflag.Tests
{
    /// <summary>
    /// This is silly but is an example in the package source.
    /// https://github.com/golang/go/blob/9f33108dfa22946622a8a78b5cd3f64cd3e455dd/src/flag/flag_test.go#L271
    /// Just shows you the flexibility of the design.
    /// </summary>
    public class CustomFlag : IFlag
    {
        public Type TrackedType => typeof(bool);

        public string Name => "b";

        public string Usage => "Pass it in as many times as you want";

        public string DefaultValue => "-100";
        private int _counter = 0;

        public bool TrySetValue(string value, out ParsingError errorMsg)
        {
            _counter++;
            errorMsg = null;
            return true;
        }

        public int Counter => _counter;

        public (string, string) UnquoteUsage()
        {
            return (Name, Usage);
        }
    }
}
