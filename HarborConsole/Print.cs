using System;

namespace Harbor.Console
{
    public static class Print
    {
        public static void Value<T>(in T value)
        {
            Type type = value.GetType();
            System.Console.WriteLine($"[{type.Name}]: {value}");
        }

        public static void Value<T>(in T value, string label)
        {
            System.Console.WriteLine($"{label}: {value}");
        }
    }
}