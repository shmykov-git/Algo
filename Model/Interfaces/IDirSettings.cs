namespace Model.Interfaces
{
    public interface IDirSettings
    {
        string OutputDirectory { get; }
        string InputDirectory { get; }
        string GetContentFileName(string fileName);
    }
}
