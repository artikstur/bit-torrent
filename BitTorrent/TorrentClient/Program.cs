using TorrentClient;

var client1 = new Client(new List<FileMetaData>(), 5000);
var client2 = new Client(new List<FileMetaData>(), 5001);
var client3 = new Client(new List<FileMetaData>(), 5002);
var client4 = new Client(new List<FileMetaData>(), 5003);
var client5 = new Client(new List<FileMetaData>(), 5004);
var client6 = new Client(new List<FileMetaData>(), 5005);

client1.Start();
client2.Start();
client3.Start();
client4.Start();
client5.Start();
client6.Start();
