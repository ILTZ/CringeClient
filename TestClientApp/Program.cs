using System;
using System.Threading;

namespace TestClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientConnection con = new ClientConnection();
            con.sendMessageToServer("first_connection");
            Thread.Sleep(1000);
            con.serverAnsweresHandler();
            while (true)
            {
                //con.sendMessageToServer("get_cringe_collection");
                con.sendRequestToServer();
                con.serverAnsweresHandler();
            }
            //Console.ReadLine();
            //con.sendMessageToServer("close_connection;");
            //Console.ReadLine();
        }
    }
}
