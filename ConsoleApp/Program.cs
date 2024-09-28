// See https://aka.ms/new-console-template for more information
using W3GNET;

Console.WriteLine("Hello, World!");


//using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\DS MIXED 501. gogo.w3g", FileMode.Open))
using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\66e41bcf0ebc9196a24e988b (1).w3g", FileMode.Open))
{
    W3GReplay w3GReplay = new W3GReplay();
    Task.WaitAll(w3GReplay.Parse(stream));
}