using MessagePack;
using System.IO;

namespace View3D.Tools.Model;

public static class AnimateExtensions
{
    public static void Save(this Animate animate, string fileName)
    {
        var bytes = MessagePackSerializer.Serialize(animate);
        File.WriteAllBytes(fileName, bytes);
    }
}