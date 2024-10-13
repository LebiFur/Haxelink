namespace Haxelink
{
    public static class Extensions
    {
        public static int AddOrGetIndex<T>(this List<T> list, T element)
        {
            int index = list.IndexOf(element);

            if (index == -1)
            {
                list.Add(element);
                return list.Count - 1;
            }
            else return index;
        }
    }
}
