using System;

namespace Harbor.ConsoleUI
{
    public static class Print
    {
        public static void Value<T>(in T value)
        {
            Type type = value.GetType();
            Console.WriteLine($"[{type.Name}]: {value}");
        }

        public static void Value<T>(in T value, string label)
        {
            Console.WriteLine($"{label}: {value}");
        }
    }
}