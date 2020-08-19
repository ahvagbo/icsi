using System;

namespace ICsi.Core
{
    [AttributeUsage(AttributeTargets.Method,
                    AllowMultiple = false)]
    internal sealed class ReplMetaCommandAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }

        public ReplMetaCommandAttribute(string command, string description)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Description = description ?? throw new ArgumentNullException(nameof(command));
        }
    }
}