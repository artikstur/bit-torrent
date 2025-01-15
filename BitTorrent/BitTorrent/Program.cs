namespace BitTorrent;

static class Program
{
    static void Main()
    {
        var dataBlocks = new List<string> {
            "Block1",
            "Block2", 
            "Block3",
            "Block4" };
        
        var merkleTree = new MerkleTree(dataBlocks);
        Console.WriteLine($"Root Hash: {merkleTree.Root.Hash}");

        var firstHash = merkleTree.ComputeHash(dataBlocks[0]);
        var secondHash = merkleTree.ComputeHash(dataBlocks[1]);
        var thirdHash = merkleTree.ComputeHash(dataBlocks[2]);
        var fourthHash = merkleTree.ComputeHash(dataBlocks[3]);

        Console.WriteLine(firstHash, secondHash, thirdHash, fourthHash);
        
        int blockIndex = 1; 
        string blockData = dataBlocks[blockIndex];
        
        // Это хеши всех парных элементов отн-о данного, чтоб вычислить потом вершину
        // Конкретнее: нужно, чтобы вычислять родителей каждого элемента (тк везде нужна пара)
        var auditPath = merkleTree.GetAuditPath(blockIndex);
        
        // А здесь мы берем наш элемент и от него начинаем идти вверх и сравниваем вершину, если равны, то все ок
        bool isValid = merkleTree
            .VerifyBlock(blockData, blockIndex, auditPath);
        
        Console.WriteLine($"Block {blockIndex} verification:" +
                          $" {(isValid ? "Valid" : "Invalid")}");
    }
}