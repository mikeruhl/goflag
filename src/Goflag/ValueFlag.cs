using System;
using System.ComponentModel;
using System.Text;
using GoFlag.Interfaces;

namespace GoFlag
{
    /// <summary>
    /// Built-In Implemetation of IFlag, can cover most typical value types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueFlag<T> : IFlag
    {
        /// <summary>
        /// Create a new flag with a name, default value, and usage text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        public ValueFlag(string name,T defaultValue, string usage)
        {
            Name = name;
            _defaultValue = defaultValue;
            Usage = usage;

        }

        /// <summary>
        /// errParse is returned by Set if a flag's value fails to parse, such as with an invalid integer for Int.
        /// It then gets wrapped through failf to provide more information.
        /// </summary>
        private readonly ParsingError _errParse = new ParsingError("parse error");

        /// <summary>
        /// ErrRange is returned by Set if a flag's value is out of range.
        /// It then gets wrapped through failf to provide more information.
        /// </summary>
        private readonly ParsingError _errRange = new ParsingError("value out of range");

        /// <summary>
        /// Create a flag with a setter function to enable flexible conversion
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="usage"></param>
        /// <param name="setter"></param>
        public ValueFlag(string name, T defaultValue, string usage, Func<T, string, T> setter)
        {
            Name = name;
            _defaultValue = defaultValue;
            Usage = usage;
            _setter = setter;

        }

        private bool _hasValue;
        private T _value;
        private T _defaultValue;
        private Func<T, string, T> _setter;

        /// <summary>
        /// Parsed or default value of the flag
        /// </summary>
        public T Value
        {
            get
            {
                if (_hasValue)
                    return _value;
                return _defaultValue;
            }
        }

        /// <summary>
        /// The type of T
        /// </summary>
        public Type TrackedType => typeof(T);


        /// <summary>
        /// Implicit conversion of Flag&lt;T&gt; to T
        /// </summary>
        /// <param name="item"></param>
        public static implicit operator T(ValueFlag<T> item)
        {
            return item.Value;
        }

        /// <summary>
        /// Usage of flag, by default will display in help output.
        /// </summary>
        public string Usage { get; set; } = string.Empty;

        /// <summary>
        /// Name of the flag, which is how to reference it on the command-line.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of flag if no value is passed via the command-line.
        /// </summary>
        public string DefaultValue => _defaultValue.ToString();

        /// <summary>
        /// Override default type Converter with custom implementation.
        /// </summary>
        /// <param name="setter"></param>
        public void UseSetter(Func<T, string, T> setter)
        {
            _setter = setter;
        }

        /// <summary>
        /// Attempts to set the value by either TypeDescriptor.GetConverter or by a custom implementation was passed using UseSetter.  returns bool of indicating success of setting the value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool TrySetValue(string value, out ParsingError err)
        {
            try
            {
                if (_setter != null)
                {
                    _value = _setter(_value, value);
                    _hasValue = true;
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    _value = (T)converter.ConvertFrom(value);
                    _hasValue = true;
                }
                err = null;
                return true;
            }
            catch (Exception e)
            {
                if (e?.InnerException is OverflowException)
                    err = new ParsingError("value out of range", e);
                else
                    err = new ParsingError("parse error", e);

                return false;
            }
        }
        /// <summary>
        /// UnquoteUsage extracts a back-quoted name from the usage string for a flag and returns it and the un-quoted usage. Given "a `name` to show" it returns ("name", "a name to show"). If there are no back quotes, the name is an educated guess of the type of the flag's value, or the empty string if the flag is boolean.
        /// </summary>
        /// <returns></returns>
        public (string, string) UnquoteUsage()
        {
            var name = "value";
            var usage = Usage;

            for (var i = 0; i < Usage.Length; i++)
            {
                if (Usage[i] == '`')
                {
                    for (var j = i + 1; j < Usage.Length; j++)
                    {
                        if (Usage[j] == '`')
                        {
                            name = Usage.Substring(i + 1, j - i - 1);
                            var uSb = new StringBuilder();
                            uSb.Append(usage.Substring(0, i));
                            uSb.Append(name);
                            uSb.Append(Usage.Substring(j + 1));
                            usage = uSb.ToString();
                            return (name, usage);
                        }
                    }
                    break;
                }
            }
            switch (_defaultValue)
            {
                case bool _:
                    name = "";
                    break;
                case TimeSpan _:
                    name = "duration";
                    break;
                case float _:
                    name = "float";
                    break;
                case int _:
                case long _:
                    name = "int";
                    break;
                case string _:
                    name = "string";
                    break;
                case uint _:
                case ulong _:
                    name = "uint";
                    break;
                case double _:
                    name = "double";
                    break;
                default:
                    break;
            }
            return (name, Usage);
        }

        /// <summary>
        /// Returns the ToString value of Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
