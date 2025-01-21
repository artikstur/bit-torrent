using System.Security.Cryptography;
using System.Text;

namespace MerkleTree;

class MerkleTree
{
    public Node Root { get; private set; }
    private readonly List<string> _leafHashes;

    public MerkleTree(List<string> dataBlocks)
    {
        _leafHashes = dataBlocks.Select(ComputeHash).ToList();
        BuildTree();
    }

    private void BuildTree()
    {
        var nodes = _leafHashes
            .Select(hash => new Node(hash))
            .ToList();

        while (nodes.Count > 1)
        {
            var parentNodes = new List<Node>();

            for (int i = 0; i < nodes.Count; i += 2)
            {
                var left = nodes[i];
                var right = i + 1 < nodes.Count
                    ? nodes[i + 1]
                    : null;

                var parentHash = right != null
                    ? ComputeHash(left.Hash + right.Hash)
                    : left.Hash;

                parentNodes.Add(new Node(parentHash) { Left = left, Right = right });
            }

            nodes = parentNodes;
        }

        Root = nodes.First();
    }

    public List<string> GetAuditPath(int index)
    {
        var path = new List<string>();
        var nodes = _leafHashes
            .Select(hash => new Node(hash))
            .ToList();

        while (nodes.Count > 1)
        {
            var parentNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i += 2)
            {
                var left = nodes[i];
                var right = i + 1 < nodes.Count ? nodes[i + 1] : null;
                var parentHash = right != null ? ComputeHash(left.Hash + right.Hash) : left.Hash;

                parentNodes.Add(new Node(parentHash) { Left = left, Right = right });

                if (i != index && i + 1 != index) continue;
                
                var sibling = i == index
                    ? right
                    : left;
                
                if (sibling != null)
                {
                    path.Add(sibling.Hash);
                }

                index /= 2;
            }

            nodes = parentNodes;
        }

        return path;
    }
    
    public bool VerifyBlock(string block, int index, List<string> auditPath)
    {
        string currentHash = ComputeHash(block);

        foreach (var siblingHash in auditPath)
        {
            currentHash = index % 2 == 0
                ? ComputeHash(currentHash + siblingHash)
                : ComputeHash(siblingHash + currentHash);

            index /= 2;
        }

        return currentHash == Root.Hash;
    }

    public string ComputeHash(string data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}