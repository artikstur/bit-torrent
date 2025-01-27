using MerkleTree;
using TorrentClient;

var sharingFile = new FileMetaData
{
    FileStatus = FileStatus.Sharing,
    FilePath = @"C:\Users\artur\OneDrive\Desktop\building.png",
    BlockSize = 1024,
};

var blocks = FileWorker.SplitFileIntoBlocks(sharingFile);
var merkleTree = new ByteMerkleTree(blocks);
var auditPath = merkleTree.GetAuditPath(2);
var isValid = merkleTree.VerifyBlock(blocks[2], 2, auditPath);
sharingFile.RootHash = BitConverter.ToString(merkleTree.Root.Hash);

var clients = new List<Client>();
var client1 = new Client(new Dictionary<string, FileMetaData>
{
    { sharingFile.RootHash, new FileMetaData {FileStatus = FileStatus.Downloading}}
});

var client2 = new Client(new Dictionary<string, FileMetaData>
{
    { sharingFile.RootHash, new FileMetaData {FileStatus = FileStatus.Sharing}}
});


clients.Add(client1);
clients.Add(client2);

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();

await Task.WhenAll(tasks);