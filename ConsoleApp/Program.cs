// See https://aka.ms/new-console-template for more information
using W3GNET;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        //using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\DS MIXED 501. gogo.w3g", FileMode.Open))
        W3GReplay w3GReplay1;
        using (var stream = new FileStream("C:\\Users\\Lasse\\Downloads\\6712f0d30ebc9196a2cd0d52.w3g", FileMode.Open))
        {
            w3GReplay1 = new W3GReplay();
            Task.WaitAll(w3GReplay1.Parse(stream));
            //Console.ReadLine();
        }
    }
}
    
