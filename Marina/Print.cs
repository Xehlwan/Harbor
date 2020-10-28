using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Marina.ConsoleUI
{
    public static class Print
    {
        public static void Value<T>(in T value)
        {
            var type = value.GetType();
            Console.WriteLine($"[{type.Name}]: {value}");
        }
        public static void Value<T>(in T value, string label)
        {
            Console.WriteLine($"{label}: {value}");
        }
    }
}
