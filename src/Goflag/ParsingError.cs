using System;

namespace GoFlag
{
    /// <summary>
    /// Returned from the parsing function if there is an error with the arguments
    /// </summary>
    public class ParsingError
    {
        /// <summary>
        /// Create an error with a descriptive message
        /// </summary>
        /// <param name="message"></param>
        public ParsingError(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        /// <summary>
        /// Create a parsing error with the message and the applicable exception to the error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public ParsingError(string message, Exception exception)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Error message describing error
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Exception that occurred to cause the error, may be null
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Compares obj with this
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns hash code of the error's message
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Message.GetHashCode();
        }

        /// <summary>
        /// Returns error's message as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Message;
        }
    }
}
