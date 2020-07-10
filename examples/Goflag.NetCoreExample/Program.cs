using System;
using GoFlag;

namespace Goflag.NetCoreExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var name = Flag.String("name", "Mike", "user's name");
            Flag.Parse();
            Console.WriteLine($"Hello, {name}!");
        }
    }
}
