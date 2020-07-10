using System;
using GoFlag;

namespace RepeaterExample
{
    class Program
    {
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
    }
}
