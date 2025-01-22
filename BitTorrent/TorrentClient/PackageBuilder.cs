namespace TorrentClient;
using static PackageBuilder;

public class PackageBuilder
{
    private readonly byte[] _package;

    public PackageBuilder(int sizeOfContent)
    {
        _package = new byte[sizeOfContent];
        CreateBasePackage();
    }
    
    // TODO: Здесь будет настраиваться начало и конец сообщения
    private void CreateBasePackage()
    {
        return;
    }

    public PackageBuilder WithContent(byte[] content)
    {
        for (var i = 0; i < content.Length; i++)
        {
            _package[i] = content[i];
        }

        return this;
    }
    
    public byte[] Build()
    {
        return _package;
    }
}
