using System.Collections.Generic;

namespace Axinom.Toolkit
{
    public static class ExtensionsForCollections
    {
        // https://stackoverflow.com/a/22668974/2928
        public static void Shuffle<T>(this IList<T> list)
        {
            void Swap(int i, int j)
            {
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }

            for (var i = 0; i < list.Count; i++)
                Swap(i, Helpers.Random.GetInteger(i, list.Count));
        }
    }
}
