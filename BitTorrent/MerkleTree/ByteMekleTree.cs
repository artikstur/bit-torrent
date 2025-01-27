using System.Security.Cryptography;

namespace MerkleTree;

public class ByteMerkleTree
{
    public Node Root { get; private set; }
    private readonly List<byte[]> _leafHashes;

    public ByteMerkleTree(List<byte[]> dataBlocks)
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
                    ? ComputeHash(CombineHashes(left.Hash, right.Hash))
                    : left.Hash;

                parentNodes.Add(new Node(parentHash) { Left = left, Right = right });
            }

            nodes = parentNodes;
        }

        Root = nodes.First();
    }

    public List<byte[]> GetAuditPath(int index)
    {
        var path = new List<byte[]>();
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
                var parentHash = right != null ? ComputeHash(CombineHashes(left.Hash, right.Hash)) : left.Hash;

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

    public bool VerifyBlock(byte[] block, int index, List<byte[]> auditPath)
    {
        byte[] currentHash = ComputeHash(block);

        foreach (var siblingHash in auditPath)
        {
            currentHash = index % 2 == 0
                ? ComputeHash(CombineHashes(currentHash, siblingHash))
                : ComputeHash(CombineHashes(siblingHash, currentHash));

            index /= 2;
        }

        return currentHash.SequenceEqual(Root.Hash);
    }

    private byte[] CombineHashes(byte[] left, byte[] right)
    {
        var combined = new byte[left.Length + right.Length];
        Buffer.BlockCopy(left, 0, combined, 0, left.Length);
        Buffer.BlockCopy(right, 0, combined, left.Length, right.Length);
        return combined;
    }

    public byte[] ComputeHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }

    public class Node
    {
        public byte[] Hash { get; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(byte[] hash)
        {
            Hash = hash;
        }
    }
}