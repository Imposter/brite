using System;

namespace Brite.Utility
{
    public static class ArrayExtensions
    {
        public static void Reverse(this Array array, int index, int length)
        {
            throw new InvalidOperationException();
        }

        public static void Reverse(this Array array)
        {
            for (var i = 0; i < array.Length / 2; i++)
            {
                var tmp = array.GetValue(i);
                array.SetValue(array.GetValue(array.Length - i - 1), i);
                array.SetValue(tmp, array.Length - i - 1);
            }
        }
    }
}
