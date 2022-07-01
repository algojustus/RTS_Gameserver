namespace RtsServer;

public class StartServer
{
    private static void Main(string[] args)
    {
        Console.Title = "RtsServer";
        RtsServer.Start(12,11000);
        Console.ReadKey();
        
    }
}