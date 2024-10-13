namespace Haxelink
{
    public static class Perpendicular
    {
        public static bool Parallel { get; set; } = true;

        public static void ForEach<T>(IEnumerable<T> source, Action<T> body, bool? overrideParallel = null)
        {
            if (overrideParallel ?? Parallel) _ = System.Threading.Tasks.Parallel.ForEach(source, body);
            else foreach (T item in source) body(item);
        }
    }
}
