using MerkleTree;
using TorrentClient;

int blockSize = 1024;
var sharingFile = new FileMetaData
{
    FileStatus = FileStatus.Sharing,
    FilePath = @"C:\Users\artur\OneDrive\Desktop\building.png",
    BlockSize = blockSize,
};

var blocks = FileWorker.SplitFileIntoBlocks(sharingFile, blockSize);
var fileSize = FileWorker.GetFileSize(sharingFile);

var merkleTree = new ByteMerkleTree(blocks);
var auditPath = merkleTree.GetAuditPath(2);
var isValid = merkleTree.VerifyBlock(blocks[1], 1, auditPath);

sharingFile.Blocks = blocks;
sharingFile.TotalBlocks = blocks.Length;
sharingFile.FileSize = fileSize;
sharingFile.RootHash = BitConverter.ToString(merkleTree.Root.Hash);

var clients = new List<Client>();
var client1 = new Client(new Dictionary<string, FileMetaData>
{
    {
        sharingFile.RootHash,
        new FileMetaData
        {
            FileStatus = FileStatus.Downloading,
            FilePath = @"C:\Users\artur\OneDrive\Desktop\test-torrent\building.png",
            BlockSize = blockSize,
            TotalBlocks = blocks.Length,
            RootHash = sharingFile.RootHash,
            Blocks = new byte[blocks.Length][],
        }
    }
});

var client2 = new Client(new Dictionary<string, FileMetaData>
{
    { sharingFile.RootHash, sharingFile }
});


clients.Add(client1);
clients.Add(client2);

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();

await Task.WhenAll(tasks);