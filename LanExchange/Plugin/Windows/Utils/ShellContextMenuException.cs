using System;

namespace LanExchange.Plugin.Windows.Utils
{
    public class ShellContextMenuException : Exception
    {
        /// <summary>Default contructor</summary>
        public ShellContextMenuException()
        {
        }

        /// <summary>Constructor with message</summary>
        /// <param name="message">Message</param>
        public ShellContextMenuException(string message)
            : base(message)
        {
        }
    }
}