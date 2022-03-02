using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using System.IO;

namespace TestClientApp
{
    public struct ResponesFromServer
    {
        public ResponesFromServer(string _initComand)
        {   
            rowString = new List<string>();
            rowBytes = new List<byte[]>(); 
        }

        public void initLists()
        {
            rowString = new List<string>();
            rowBytes = new List<byte[]>();
        }


        public List<string> rowString { get; set; }
        public List<byte[]> rowBytes { get; set; }

    }
        



    class ClientConnection
    {
        static int destPort = 8005;
        static string destIP = "192.168.1.119";

        IPEndPoint ipPoint = null;
        Socket mainSocket = null;

        public bool isWork = false;

        private int appID = 0;

        private List<ResponesFromServer> responesStotage = new List<ResponesFromServer>();

        private bool responesWillRecive = false;

        public bool getWorkStatus()
        {
            return isWork;
        }
        public void setWorkStatus(bool nWorkStatus)
        {
            isWork = nWorkStatus;
        }


        public ClientConnection(int destinationPort = 8005, string destinationIP = "192.168.1.119")
        {
            destPort = destinationPort;
            destIP = destinationIP;

            ipPoint = new IPEndPoint(IPAddress.Parse(destIP), destPort);
            mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainSocket.Connect(ipPoint);


            isWork = true;

            Random rand = new Random();
            appID = rand.Next(1, 50);
            appID = 1;



        }


        public void serverAnsweresHandler()
        {
            while (!responesWillRecive)
            {
                if (!mainSocket.Connected)
                {
                    return;
                }
                //ResponesFromServer resp = new ResponesFromServer();
                //resp.initLists();
                //bool endRespones = false;

                //while (!endRespones)
                //{
                if (mainSocket.Available > 0)
                {
                    byte[] reciveData = new byte[256];
                    StringBuilder answerBuilder = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = mainSocket.Receive(reciveData, reciveData.Length, 0);
                        answerBuilder.Append(Encoding.Unicode.GetString(reciveData, 0, bytes));
                    } while (mainSocket.Available > 0);

                    commanHandler(answerBuilder.ToString());
                    //Console.WriteLine(count++);
                    //Console.WriteLine(answerBuilder.ToString());

                    /*     if (answerBuilder.ToString() == "go_bytes")
                     {
                         byteDataHandler(ref resp);
                     }
                         //printResponesData();

                         if (answerBuilder.ToString() == "end_respones;")
                         {
                             endRespones = true;
                         responesStotage.Add(resp);
                             Console.WriteLine("Final");
                         }

                         sendMessageToServer("ready");
                     }*/
                    //}
                    printResponesData();
                }
            }

            responesWillRecive = false;
        }

  

        public void sendRequestToServer()
        {
            string mes;
            mes = Console.ReadLine();
            sendMessageToServer(mes);
        }
        
        // Respones pattern:
        // "respones;comand_from_client_name;....;(some_info);....;//(<-row separator);......;//.....(some_info);.......;end_respones"
        // forward "end_respones" may be "go_bytes". Bytes will sending until keyword "end_bytes" and its dont need to convert to string.
        private void commanHandler(string iCommand)
        {
            string[] reciveComand = iCommand.Split(";");

            switch(reciveComand[0])
            {
                case ("connected_succesful"):
                    Console.WriteLine("Connected succesful, code " + reciveComand[1]);
                    responesWillRecive = true;
                    break;

                case ("bad_request"):
                    Console.WriteLine("bad_request");
                    break;

                case ("start_respones"):
                    Console.WriteLine("good_respones");
                    trueResponesHandlerB();
                    break;

                case ("end_respones"):
                    Console.WriteLine("End_respones");
                    break;

                case ("on_connection"):
                    sendMessageToServer("on_connection", "single");
                    break;


                default:
                    Console.WriteLine("bad_request");
                    break;
            }
            
            foreach (var s in reciveComand)
            {
                //Console.WriteLine(s);
            }
        }

        private void responesStringFiller(ref string[] _respones, ref ResponesFromServer _resp)
        {
            string row = "";
            foreach (string rowPart in _respones)
            {
                if (rowPart == "//")
                {
                    _resp.rowString.Add(row);
                    row = "";
                }
                else if (rowPart == "end_respones")
                {
                    return;
                }
                else if (rowPart == "")
                {
                    continue;
                }
                else if (rowPart != "//")
                {
                    row += rowPart;
                }

            }
        }

        private void trueResponesHandler()
        {
            ResponesFromServer resp = new ResponesFromServer();
            resp.initLists();
            bool endRespones = false;
            while (!endRespones)
            {
                sendMessageToServer("ready");

                if (mainSocket.Available > 0)
                {
                    byte[] reciveData = new byte[256];
                    StringBuilder answerBuilder = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = mainSocket.Receive(reciveData, reciveData.Length, 0);
                        answerBuilder.Append(Encoding.Unicode.GetString(reciveData, 0, bytes));
                    } while (mainSocket.Available > 0);

                    string[] ans = answerBuilder.ToString().Split(';');

                    if (ans[0] == "go_bytes")
                    {
                        byteDataHandler(ref resp);
                    }
                    else if (ans[0] == "end_respones")
                    {
                        endRespones = true;
                        responesStotage.Add(resp);
                        responesWillRecive = true;
                    }
                    else
                    {
                        responesStringFiller(ref ans, ref resp);
                    }
                }
            }
        }

        private void trueResponesHandlerB()
        {
            ResponesFromServer resp = new ResponesFromServer();
            resp.initLists();
            bool messageWasSended = false;
            bool endRespones = false;
            while (!endRespones)
            {
                if (!messageWasSended)
                {
                    sendMessageToServer("ready"); // to many messages to server couse loop
                    messageWasSended = true;
                }

                if (!(mainSocket.Available > 0))
                {
                    Thread.Sleep(100);
                    continue;
                }

                messageWasSended = false;

                    byte[] reciveData = new byte[256];
                    StringBuilder answerBuilder = new StringBuilder();
                    int bytes = 0;

                    do
                    {
                        bytes = mainSocket.Receive(reciveData, reciveData.Length, 0);
                        answerBuilder.Append(Encoding.Unicode.GetString(reciveData, 0, bytes));
                    } while (mainSocket.Available > 0);

                    string[] ans = answerBuilder.ToString().Split(';');

                    if (ans[0] == "go_bytes")
                    {
                        byteDataHandlerB(ref resp);
                    }
                    else if (ans[0] == "end_respones")
                    {
                        endRespones = true;
                        responesStotage.Add(resp);
                        responesWillRecive = true;
                    }
                    else
                    {
                        responesStringFiller(ref ans, ref resp);
                    }
            }
        }


        private void byteDataHandler(ref ResponesFromServer _resp)
        {
            bool endBytes = false;
            
            while (!endBytes)
            {
                if (mainSocket.Connected)
                {
                    sendMessageToServer("ready");

                    byte[] buffer = new byte[1024];
                    int bytes = 0;

                    byte[] reciveBytesData = new byte[0];

                    MemoryStream ms = new MemoryStream();



                    do
                    {
                        bytes = mainSocket.Receive(buffer, buffer.Length, 0);
                        string f = Encoding.Unicode.GetString(buffer,0, bytes).ToString();
                        if (((Encoding.Unicode.GetString(buffer, 0, bytes).ToString()) == "end_bytes;") ||  // Not always correct work but its fucking work b'lyat
                            ((Encoding.Unicode.GetString(buffer, 0, bytes).ToString()) == "end_respones;")) 
                        {
                            endBytes = true;
                            return;
                        }

                        ms.Write(buffer, 0, buffer.Length);

                    } while (mainSocket.Available > 0);

                    byte[] result = new byte[ms.Length];
                    Array.Copy(ms.GetBuffer(), result, ms.Length);
                    _resp.rowBytes.Add(result);
                }
            }

        }

        private void byteDataHandlerB(ref ResponesFromServer _resp)
        {
            bool endBytes = false;
            bool messageWasSended = false;


            while (!endBytes)
            {
                if (!messageWasSended)
                {
                    sendMessageToServer("ready");
                    messageWasSended = true;
                }

                    if (!(mainSocket.Available > 0))
                {
                    Thread.Sleep(100);
                    continue;
                }

                messageWasSended = false;

                    byte[] buffer = new byte[1024];
                    int bytes = 0;

                    byte[] reciveBytesData = new byte[0];

                    MemoryStream ms = new MemoryStream();



                    do
                    {
                        bytes = mainSocket.Receive(buffer, buffer.Length, 0);
                        string f = Encoding.Unicode.GetString(buffer, 0, bytes).ToString();
                        if (((Encoding.Unicode.GetString(buffer, 0, bytes).ToString()) == "end_bytes;") ||  // Not always correct work but its fucking work b'lyat
                            ((Encoding.Unicode.GetString(buffer, 0, bytes).ToString()) == "end_respones;"))
                        {
                            endBytes = true;
                            return;
                        }

                        ms.Write(buffer, 0, buffer.Length);

                    } while (mainSocket.Available > 0);

                    byte[] result = new byte[ms.Length];
                    Array.Copy(ms.GetBuffer(), result, ms.Length);
                    _resp.rowBytes.Add(result);
                
            }
        }


        private void printResponesData()
        {
            foreach (ResponesFromServer resp in responesStotage)
            {
                Console.WriteLine("________String_rows__________");
                for (int i = 0; i < resp.rowString.Count; ++i)
                {
                    Console.WriteLine($"Row {i+1}:\t{resp.rowString[i]}");
                }

                Console.WriteLine("_________Byte_rows__________");
                for (int i = 0; i < resp.rowBytes.Count; ++i)
                {
                    Console.WriteLine($"Byte row:\t{i}:");
                    Console.WriteLine($"Byte_count:\t{resp.rowBytes[i].Length}");
                    //foreach (byte b in resp.rowBytes[i])
                    //{
                     //   Console.Write(b);
                    //}
                }
            }
        }

        // If param isn't define: (message += {appID})
        public void sendMessageToServer(string _message, string _param = "on_rules")
        {
            if (mainSocket.Connected)
            {
                if (_param == "on_rules")
                {
                    _message += ";" + Convert.ToString(appID);
                }
                mainSocket.Send(Encoding.Unicode.GetBytes(_message));
            }
            else
            {
                debugConsoleMsg("Client doesn't conencted with server!");
            }
        }


        /// <summary>
        /// Method would be "get" or "set"
        /// </summary>
        /// <param name="message"></param>
        /// <param name="method"></param>
        public void sendGetSetQueryToServer(string _message, string _method)
        {
            try
            {
                string finalMessage = _method + ";" + Convert.ToInt32(appID) + ";" + _message;
                byte[] messageToServer = Encoding.Unicode.GetBytes(finalMessage);
                mainSocket.Send(messageToServer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void debugConsoleMsg(string message)
        {
            Console.WriteLine($"\nERROR::{message}\n");
        }

    }
}
