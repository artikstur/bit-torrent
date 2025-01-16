# Educational Project: Building Your Own BitTorrent

This project is focused on understanding and implementing the fundamentals of a BitTorrent-like system. It involves creating a peer-to-peer file-sharing network that uses **Merkle Trees** for data integrity verification.

For more details on the concept and benefits of Merkle Trees, you can refer to this explanation:  
[Why Merkle Trees?](https://github.com/cliftonm/MerkleTree/blob/master/why%20merkle%20trees.txt)

---

## What is an Audit Path in a Merkle Tree?

An **audit path** is a sequence of hashes that allows you to verify the integrity of a specific piece of data (a leaf) within a Merkle Tree. 

### How It Works:
1. **Choose a Leaf**: Start with the specific data block (leaf) you want to verify.
2. **Get the Path**: The server or a peer provides the hashes required to compute the path from the leaf to the root of the tree.
3. **Verify the Path**: Use the provided hashes to calculate the root hash. If the computed root matches the known root, the data is verified as part of the tree and hasn't been tampered with.

### Why It’s Useful:
The audit path ensures that:
- A data block belongs to the tree corresponding to a specific root.
- No changes or falsifications occurred in the data or its structure.

### Example Use Case:
When downloading a file in chunks, the server provides an audit path for each chunk. The client computes hashes to verify the chunk against the root hash, ensuring the integrity of the downloaded data.

### Also, you can view some usefull links on this on this topic:
[Merkle-tree: How to check data integrity without full access?](https://habr.com/ru/articles/873718/) 

[How Do Merkle Trees Work?](https://www.baeldung.com/cs/merkle-trees)


