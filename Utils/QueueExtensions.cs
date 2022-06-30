using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace ChatCommands.Utils
{
    public static class QueueExtensions
    {
        public static bool TryDequeueFloat(this Queue<string> queue, out string stringValue, out float value)
        {
            value = default;
            return queue.TryDequeue(out stringValue) && float.TryParse(stringValue, out value);
        }

        public static bool TryDequeueLong(this Queue<string> queue, out string stringValue, out long value)
        {
            value = default;
            return queue.TryDequeue(out stringValue) && long.TryParse(stringValue, out value);
        }

        public static bool TryDequeueInt(this Queue<string> queue, out string stringValue, out int value)
        {
            value = default;
            return queue.TryDequeue(out stringValue) && int.TryParse(stringValue, out value);
        }

        public static bool TryDequeueShort(this Queue<string> queue, out string stringValue, out short value)
        {
            value = default;
            return queue.TryDequeue(out stringValue) && short.TryParse(stringValue, out value);
        }

        public static bool TryDequeueByte(this Queue<string> queue, out string stringValue, out byte value)
        {
            value = default;
            return queue.TryDequeue(out stringValue) && byte.TryParse(stringValue, out value);
        }

        public static bool TryDequeueBool(this Queue<string> queue, out string stringValue, out bool value)
        {
            value = default;
            if (!queue.TryDequeue(out stringValue))
                return false;
            if (bool.TryParse(stringValue, out value))
                return true;
            if (stringValue.ToLowerInvariant() is { } lowerCase)
            {
                switch (lowerCase)
                {
                    case "t":
                    case "true":
                        value = true;
                        return true;
                    case "f":
                    case "false":
                        value = false;
                        return true;
                }
            }

            return false;
        }
    }
}
