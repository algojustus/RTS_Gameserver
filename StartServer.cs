namespace RtsServer;
using System.Threading;
public class StartServer
{
    private static bool isRunning;
    private static int TicksPerSecond = 30;
    private static int TicksPerMS = 1000/TicksPerSecond;
    private static void Main(string[] args)
    {
        Thread mainThread = new Thread(MainThread);
        Console.Title = "RtsServer";
        isRunning = true;
        mainThread.Start();
        RtsServer.Start(12, 11000);
    }

    private static void MainThread()
    {
        Console.WriteLine("Server running hot");
        DateTime dateTime = DateTime.UtcNow;
        while (isRunning)
        {
            while (dateTime < DateTime.UtcNow)
            {
                Update();
                dateTime = dateTime.AddMilliseconds(TicksPerMS);
                if (dateTime > DateTime.UtcNow)
                {
                    Thread.Sleep(dateTime - DateTime.UtcNow);
                }
            }
        }
    }
    private static void Update()
    {
        ThreadManager.UpdateMain();
    }
}