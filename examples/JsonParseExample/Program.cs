using System;
using GoFlag;
using GoFlag.Enums;
using Newtonsoft.Json;

namespace JsonParseExample
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new[] { "-rect", "{\"height\": 200, \"width\": 400}" };
            var fs = Flag.NewFlagSet("JsonParse", ErrorHandling.ContinueOnError);
            var rectFlag = new ValueFlag<Rectangle>("rect", default, "create a rectangle from json", ParseJson);
            fs.Var(rectFlag);
            fs.Parse(args);
            Rectangle rect = rectFlag;
            Console.WriteLine($"I have a rectangle with a height of {rect.Height} and a width of {rect.Width}");
        }

        static Rectangle ParseJson(Rectangle existingValue, string json)
        {
            return JsonConvert.DeserializeObject<Rectangle>(json);
        }
    }
}
