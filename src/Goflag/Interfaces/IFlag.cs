using System;

namespace GoFlag.Interfaces
{
    /// <summary>
    /// Interface for defining flags to parse
    /// </summary>
    public interface IFlag
    {
        /// <summary>
        /// The Type of the flag's value.
        /// </summary>
        Type TrackedType { get; }

        /// <summary>
        /// Attempt to set the value of the flag with the provided string representation.  Returns bool of success of setting.  Will set errorMsg if unsuccessful.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        bool TrySetValue(string value, out ParsingError errorMsg);

        /// <summary>
        /// Flag name, used to reference on command-line and in help.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Instructions on usage of flag.
        /// </summary>
        string Usage { get; }

        /// <summary>
        /// Value of flag if no value passed on command-line.
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// UnquoteUsage extracts a back-quoted name from the usage string for a flag and returns it and the un-quoted usage. Given "a `name` to show" it returns ("name", "a name to show"). If there are no back quotes, the name is an educated guess of the type of the flag's value, or the empty string if the flag is boolean.
        /// </summary>
        /// <returns></returns>
        (string, string) UnquoteUsage();
    }
}
