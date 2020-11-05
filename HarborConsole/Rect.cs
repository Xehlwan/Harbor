using System;
using System.Collections.Generic;
using System.Text;

namespace Harbor.Console
{
    public readonly struct Rect
    {
        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public Rect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public Rect((int left, int top, int width, int height) tuple) : this(
            tuple.left, tuple.top, tuple.width, tuple.height)
        {

        }

        public void Deconstruct(out int left, out int top, out int width, out int height)
        {
            left = Left;
            top = Top;
            width = Width;
            height = Height;
        }

        public static implicit operator Rect((int left, int top, int width, int height) tuple) => 
            new Rect(tuple.left, tuple.top, tuple.width, tuple.height);

        public static Rect operator +(Rect a, Rect b)
        {
            int left = Math.Min(a.Left, b.Left);
            int top = Math.Min(a.Top, b.Top);
            int width = Math.Max(a.Left + a.Width, b.Left + b.Width) - left;
            int height = Math.Max(a.Top + a.Height, b.Top + b.Height) - top;
            return new Rect(left, top, width, height);
        }
    }
}
