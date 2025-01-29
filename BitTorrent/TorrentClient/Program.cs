using MerkleTree;
using TorrentClient;

string filePath = @"C:\Users\artur\OneDrive\Desktop\life.png";
string torrentPath = @"C:\Users\artur\OneDrive\Desktop\test-torrent\life.png";
int blockSize = 1024; // 1кб

var sharingFile = new FileMetaData
{
    FileStatus = FileStatus.Sharing,
    FilePath = filePath,
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

var client1 = new Client();
await client1.AddFile(sharingFile.RootHash, new FileMetaData
{
    FileStatus = FileStatus.Downloading,
    FilePath = torrentPath,
    BlockSize = blockSize,
    TotalBlocks = blocks.Length,
    RootHash = sharingFile.RootHash,
    Blocks = new byte[blocks.Length][],
});

var client2 = new Client();
var client3 = new Client();
var client4 = new Client();
var client5 = new Client();
var client6 = new Client();

clients.Add(client1);
clients.Add(client2);
clients.Add(client3);
clients.Add(client4);
clients.Add(client5);
clients.Add(client6);

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();
await Task.Delay(3000);
await client2.AddFile(sharingFile.RootHash, sharingFile);

await Task.WhenAll(tasks);