using System.IO;
using MessagePack;

namespace View3D.Tools.Model;

public static class AnimateExtensions
{
    public static void Save(this Animate animate, string fileName)
    {
        var bytes = MessagePackSerializer.Serialize(animate);
        File.WriteAllBytes(fileName, bytes);
    }
}