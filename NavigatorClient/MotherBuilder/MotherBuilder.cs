/////////////////////////////////////////////////////////////////////////////////
// MotherBuilder.cs : Sends Build request to the child process                 //
//                                                                             // 
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: The Mother builder recieves messages from the GUI regarding the no of
 *              child process to be created and build request from the repository. 
 *              Based on these request each build is alloted to child processes.
 *              
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using System.Diagnostics;
using System.Threading;
using SWTools;
using System.IO;
using System.Windows;

namespace SpawnProc
{
    public class SpawnProc
    {
        public static Comm comm;
        const int port = 8081;
        static int childPort = 8090;
        //static int childCount;
        const string motherBuilderAddress = "http://localhost:8081/IPluggableComm";
        public static BlockingQueue<CommMessage> readyMessagesQ { get; set; } = null;
        public static BlockingQueue<CommMessage> buildRequestsQ { get; set; } = null;

        // public static BlockingQueue<string> ready { get; set; } = null;
        static Thread MsgHandler;
        static Thread queueHandler;

        // creates child process based on GUI input from the user
        static bool createProcess(int i)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\ChildProc\\bin\\debug\\ChildProc.exe";
            string absFileSpec = Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }

        // sends request to the child processes, based on the build request from the repository 
        // and the ready messages from the child processes.
        public static void queueProcessHandler()
        {
            while (true)
            {
                List<string> allFileNames = new List<string>();
                while (readyMessagesQ != null && buildRequestsQ != null)
                {
                    CommMessage commR = readyMessagesQ.deQ();
                    //if (buildRequestsQ != null)
                    CommMessage commB = buildRequestsQ.deQ();
                    allFileNames = commB.arguments;
                    //Console.WriteLine(commR.arguments);

                    //get unique items
                    var unique_items = new HashSet<string>(allFileNames);
                    List<string> FileNames = new List<string>();
                    foreach (string s in unique_items)
                        FileNames.Add(s);
                    Console.WriteLine("========================================");
                    for (int i = 0; i < FileNames.Count; i++)
                    {
                        Console.WriteLine("{0}", FileNames[i]);
                    }
                    //Console.WriteLine(allFileNames);
                    Console.WriteLine("========================================");
                    sendMessage(commR.from, FileNames, commB.xmlFile);
                }
            }
        }

        static List<string> childURLs = new List<string>();
        // Handles messages based on the type of request
        public static void processMotherMessage()
        {
            while (true)
            {   CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {   if (!msg.type.Equals("connect") && !msg.command.Equals("file"))
                        msg.show();
                    if (msg.command.Equals("ready"))
                        readyMessagesQ.enQ(msg);
                    else if (msg.command.Equals("file"))
                        File.WriteAllText("../../../BuilderStorage/b.xml", msg.arguments[0]);
                    else if (msg.command.Equals("Create Process"))
                    {
                        int count = int.Parse(msg.arguments[0]);
                        for (int i = 1; i <= count; ++i)
                        {   if (createProcess(i))
                            {
                                //Console.WriteLine("Sending first Ready message to childport" + (childPort + i));
                                sendFirstMessage("http://localhost:" + (childPort + i) + "/IPluggableComm", childPort + i);
                                if (childURLs.Contains("http://localhost:" + (childPort + i) + "/IPluggableComm"))
                                {
                                }
                                else
                                {
                                    childURLs.Add("http://localhost:" + (childPort + i) + "/IPluggableComm");
                                }
                                Console.Write(" Child Processes created successfully");
                            }
                        }
                    }
                    else if (msg.command.Equals("build"))
                        buildRequestsQ.enQ(msg);
                    else if (msg.command.Equals("Quit")) // sending QUIT message
                    {
                        foreach (string s in childURLs)
                            sendQuitMessage(s);
                        Console.WriteLine("Quit Message Recieved");
                        
                    }

                }
            }
        }

        // starts the WCF communication
        public SpawnProc()
        {
            comm = new Comm("http://localhost", port);
            readyMessagesQ = new BlockingQueue<CommMessage>();
            buildRequestsQ = new BlockingQueue<CommMessage>();
            MsgHandler = new Thread(processMotherMessage);
            MsgHandler.Start();
            queueHandler = new Thread(queueProcessHandler);
            queueHandler.Start();

        }

        // sends first ready message to the child process after being created
        static void sendFirstMessage(string childURL, int port)
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);

            csndMsg1.command = "First Message to Child";
            csndMsg1.author = "Prannoy";
            csndMsg1.to = childURL;
            csndMsg1.from = "http://localhost:8081/IPluggableComm";
            csndMsg1.arguments.Add(port.ToString());
            comm.postMessage(csndMsg1);
        }

        // sends quit message to its child processes
        static void sendQuitMessage(string childURL)
        {
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);

            csndMsg1.command = "Quit";
            csndMsg1.author = "Prannoy";
            csndMsg1.to = childURL;
            csndMsg1.from = "http://localhost:8081/IPluggableComm";
            csndMsg1.arguments.Add(port.ToString());
            comm.postMessage(csndMsg1);
        }

        // sends message to child after recieving build request from repository
        // and ready message from the child processes
        static void sendMessage(string childURL, List<string> files, string xml)
        {
            //Console.WriteLine("Sending build messages to the child.");
            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);

            csndMsg1.command = "Process Pool";
            csndMsg1.author = "Prannoy";
            csndMsg1.to = childURL;
            csndMsg1.from = "http://localhost:8081/IPluggableComm";
            csndMsg1.arguments = files;
            csndMsg1.xmlFile = xml;
            comm.postMessage(csndMsg1);
        }

        static void Main(string[] args)
        {
            SpawnProc mother = new SpawnProc();
            Console.WriteLine("*************************************");
            Console.WriteLine("            MOTHER BUILDER ");
            Console.WriteLine("*************************************");
        }
    }
}

