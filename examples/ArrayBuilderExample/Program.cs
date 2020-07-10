using System;
using System.Collections.Generic;
using System.Linq;
using GoFlag;
using GoFlag.Enums;

namespace ArrayBuilderExample
{
    class Program
    {
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
    }
}
