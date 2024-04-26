namespace Font.Extensions;

public static class BinaryReaderExtensions
{
    public static void Seek(this BinaryReader reader, long position, string desc)
    {
        //Debug.WriteLine($"Position={position} ({desc})");
        reader.BaseStream.Seek(position, SeekOrigin.Begin);
    }
}
