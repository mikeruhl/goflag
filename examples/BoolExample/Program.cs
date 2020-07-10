using System;
using GoFlag;
using GoFlag.Enums;

namespace BoolExample
{
    class Program
    {
        static void Main(string[] args)
        {
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
    }
}
