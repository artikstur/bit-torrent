namespace TorrentClient;

using static PackageHelper;

public class PackageBuilder
{
    private readonly byte[] _package;

    public PackageBuilder(int sizeOfContent)
    {
        if (sizeOfContent > MaxSizeOfContent)
        {
            throw new ArgumentException(
                $"size of content must be less or equal {nameof(MaxSizeOfContent)}",
                nameof(sizeOfContent));
        }
        
        _package = new byte[MaxFreeBytes + sizeOfContent];
        CreateBasePackage();
    }
    
    private void CreateBasePackage()
    {
        Array.Copy(BasePackage, _package, BasePackage.Length);
        
        _package[^1] = EndByte;
    }

    public PackageBuilder WithContent(byte[] content)
    {
        if (content.Length > _package.Length - MaxFreeBytes)
        {
            throw new ArgumentException(nameof(content));
        }

        for (var i = 0; i < content.Length; i++)
        {
            _package[i + MaxFreeBytes - 1] = content[i];
        }

        return this;
    }
    
    public PackageBuilder WithPackageType(PackageType packageType)
    {
        _package[PackageTypeIndex] = (byte)packageType;

        return this;
    }

    public PackageBuilder WithQuery(QueryType queryType)
    {
        _package[QueryIndex] = (byte)queryType;

        return this;
    }
    
    public PackageBuilder WithCommand(CommandType commandType)
    {
        _package[CommandIndex] = (byte)commandType;

        return this;
    }

    public byte[] Build()
    {
        return _package;
    }
}