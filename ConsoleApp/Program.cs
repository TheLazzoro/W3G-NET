// See https://aka.ms/new-console-template for more information
using W3GNET;

Console.WriteLine("Hello, World!");


//using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\DS MIXED 501. gogo.w3g", FileMode.Open))
W3GReplay w3GReplay1;
W3GReplay w3GReplay2;
using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\66e41bcf0ebc9196a24e988b (1).w3g", FileMode.Open))
{
    w3GReplay1 = new W3GReplay(true);
    Task.WaitAll(w3GReplay1.Parse(stream));
}

using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\66e41bcf0ebc9196a24e988b (1).w3g", FileMode.Open))
{
    w3GReplay2 = new W3GReplay(false);
    Task.WaitAll(w3GReplay2.Parse(stream));
}