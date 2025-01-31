using MerkleTree;
using TorrentClient;

string filePath = @"";
string torrentPath = @"";
int blockSize = 1024; // 1кб
var blocks = FileWorker.SplitFileIntoBlocks(filePath, blockSize);
var merkleTree = new ByteMerkleTree(blocks);
var auditPath = merkleTree.GetAuditPath(2);
var isValid = merkleTree.VerifyBlock(blocks[1], 1, auditPath);

var sharingFile = new FileMetaData
{
    FileStatus = FileStatus.Sharing,
    FilePath = filePath,
    BlockSize = blockSize,
    TotalBlocks = blocks.Length,
    RootHash = BitConverter.ToString(merkleTree.Root.Hash),
    Blocks = blocks,
    FileSize = FileWorker.GetFileSize(filePath),
};

var downloadingFile = new FileMetaData
{
    FileStatus = FileStatus.Downloading,
    FilePath = torrentPath,
    BlockSize = blockSize,
    TotalBlocks = blocks.Length,
    RootHash = sharingFile.RootHash,
    Blocks = new byte[blocks.Length][],
};

var clients = new List<Client>();
var client1 = new Client();
var client2 = new Client();
//var client3 = new Client();
//var client4 = new Client();
//var client5 = new Client();
//var client6 = new Client();

clients.Add(client2);
//clients.Add(client3);
//clients.Add(client4);
//clients.Add(client5);
//clients.Add(client6);

var task = Task.Run(async () => await client1.Start());
var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();
await Task.Delay(1000);
await client1.AddFile(sharingFile.RootHash, downloadingFile); 
await client2.AddFile(sharingFile.RootHash, sharingFile);

await Task.WhenAll(tasks);