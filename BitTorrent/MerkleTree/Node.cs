namespace MerkleTree;

internal class Node
{
    public string Hash { get; set; }
    public Node Left { get; set; }
    public Node? Right { get; set; }

    public Node(string hash)
    {
        Hash = hash;
    }
}