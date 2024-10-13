namespace Haxelink
{
    public readonly struct HashlinkConstant
    {
        public int Global { get; }
        public int[] Fields { get; }

        public HashlinkConstant(int global, int[] fields)
        {
            Global = global;
            Fields = fields;
        }
    }
}
