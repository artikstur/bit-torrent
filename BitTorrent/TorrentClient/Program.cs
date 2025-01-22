﻿using TorrentClient;

var clients = new List<Client>();

for (int i = 0; i < 10; i++)
{
    int clientPort = 12132 + i;  
    var client = new Client(new List<FileMetaData>(), 
        23232, clientPort);
    clients.Add(client);
}

var tasks = clients.Select(client => Task.Run(async () => await client.Start())).ToArray();

await Task.WhenAll(tasks);