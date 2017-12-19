/////////////////////////////////////////////////////////////////////////////////
// Repository.cs : Sends Build Messages to mother builder                      //
//                                                                             // 
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: When the repositrory recieves build request from the client
 *              it sends the build request to the mother builder for the build.
 *              
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using TestHarnessMessages;
using System.Xml.Linq;
using System.IO;
using MessagePassingComm;
using MsgPassing;
using System.Threading;
using System.Diagnostics;
using System.Windows;

namespace MsgPassing
{
    public class Repository
    {
        public static Comm comm;
        const int port = 8082;
        const string RepoAddress = "http://localhost:8082/IPluggableComm";

        private static Stack<TestElement> stack = new Stack<TestElement>();
        private static string xmlAsString { get; set; }
        public XDocument doc { get; set; } = new XDocument();
        static List<string> builderFiles { get; set; } = new List<string>(); // to send to the mother builder 
        static string xmlFileContent { get; set; }
        // creates indivisual test request
        public static void createTestRequest(List<string> tests)
        {
            TestElement te1 = new TestElement();
            te1.testName = "test";
            te1.addDriver(tests[0]);
            for (int i = 1; i < tests.Count; i++)
                te1.addCode(tests[i]);
            allTestRequestXML(te1);
        }

        // evaluates the test driver and test codes from the XML file
        private static void forXML_Request(string trXml)
        {
            // new approach
            //            string trXml = msg.body;
            //            xmlFile = msg.body;
            List<string> files = new List<string>();
            TestRequest newRequest = trXml.FromXml<TestRequest>();
            // string body = tr.ToXml();   body => trXml
            TestRequest newTrq = trXml.FromXml<TestRequest>();
            //Console.Write("\n{0}\n", newTrq);
            foreach (TestElement s in newTrq.tests)
            {
                Console.WriteLine("The test names are :");
                Console.WriteLine(s.testName);
                Console.WriteLine("the driver is");
                Console.WriteLine(s.testDriver);
                files.Add(s.testDriver);//builderFiles.Add(s.testDriver);
                Console.WriteLine("the test codes is");
                foreach (string a in s.testCodes)
                {
                    Console.WriteLine(a);
                    files.Add(a);//builderFiles.Add(a);
                }
            }
            getFiles(files);
        }

        // getter
        static void getFiles(List<string> f)
        {
            builderFiles = f;
        }

        // all Test Element are stored inside a stack
        public static void allTestRequestXML(TestElement te1)
        {
            stack.Push(te1);
        }

        // generates XML and stores it at specified location
        public static void createXML(int index)
        {
            TestRequest tr = new TestRequest();   // adds both the test cases in one XML here
            tr.author = "Prannoy Singh";
            foreach (TestElement te in stack)
            {
                tr.tests.Add(te);
            }
            string trXml = tr.ToXml();
            xmlFileContent = trXml;
            //"C:/Users/prann/Desktop/WpfApp/RepoStore/myXML"+index+".xml";
            string path = "../../../RepoStore/myXML" + index + ".xml";
            File.WriteAllText(path, trXml);
            Console.WriteLine("Storage Path for the XML file : " + "../../../RepoStore/myXML" + index + ".xml");
            stack.Clear();      // clears all the values from the stack
            xmlAsString = trXml;
            forXML_Request(trXml);// to get test driver and test codes

            createMotherMessage(trXml);
        }

        //display XML
        public static string showXML()
        {
            string s = xmlAsString;
            xmlAsString = " ";
            Console.WriteLine(xmlAsString);
            return s;
        }

        public static void GUI_Message()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "Update File";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8079/IPluggableComm";
            csndMsg.from = "http://localhost:8082/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments.Add("Repository Contents Updated");
            comm.postMessage(csndMsg);
        }


        // evaluates all the messages recieved
        public static void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    if (!msg.type.Equals("connect"))
                        msg.show();
                    if (msg.command.Equals("create Test Request"))
                        createTestRequest(msg.arguments);                    
                    if (msg.command.Equals("Create XML File"))
                       createXML(int.Parse(msg.arguments[0]));
                    if (msg.command.Equals("requesting files from Child"))
                    {
                        childCount++;
                        string p = "/build" + childCount;
                        CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
                        csndMsg.command = "Sending files to child.";
                        csndMsg.author = "Prannoy";
                        csndMsg.to = msg.from;//"http://localhost:8081/IPluggableComm";
                        csndMsg.from = "http://localhost:8081/IPluggableComm";// different ports for different communication levels 
                        string portID = msg.from.Substring(17, 4);
                        foreach (string name in msg.arguments)
                            comm.postFile(name, portID);
                        comm.postMessage(csndMsg);
                        Console.WriteLine("Files sent successfully to child process.");
                    }
                    if (msg.command.Equals("Sending test logs to the repository"))
                    {
                        Console.WriteLine("\nTest Logs {0} Recieved ", msg.arguments[0]);
                        Console.WriteLine("============================================================");
                        GUI_Message();
                    }
                    if (msg.command.Equals("Build Logs"))
                    {
                        Console.WriteLine("\nBuild Logs  {0} Recieved. ", msg.arguments[0]);
                        Console.WriteLine("=============================================================");
                        GUI_Message();
                    }
                    if (msg.command.Equals("Build Xml"))
                    {
                        buildFrom_XML();
                    }
                }
            }
        }

        private static void buildFrom_XML()
        {
            Console.WriteLine("Building From XML Request Recieved");
            Console.WriteLine("======================================");
            string absPath = Path.GetFullPath("../../../RepoStore/BuildXML1" + ".xml");
            string readText = File.ReadAllText(absPath);
            forXML_Request(readText);
            createMotherMessage(readText);
        }

        static int childCount = 0;
        // creates buiLd message for the mother builder
        public static void createMotherMessage(string xml)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            csndMsg.command = "build";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:8082/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments = builderFiles;
            csndMsg.xmlFile = xml;
            comm.postMessage(csndMsg);
        }

        // starts the WCF communication
        public Repository()
        {
            comm = new Comm("http://localhost", 8082);
            //CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            //csndMsg.command = "show";
            //csndMsg.author = "Jim Fawcett";
            //csndMsg.to = "http://localhost:8081/IPluggableComm";
            //csndMsg.from = "http://localhost:8082/IPluggableComm";// different ports for different communication levels 

            //comm.postMessage(csndMsg);
            Thread RepoHandler = new Thread(processMessages);
            RepoHandler.Start();
        }

        // constructor
        public Repository(string nP)
        {

        }

        // sends message to the test harness with the builder files
        static void sendFirstMessage()
        {
            CommMessage csndMsg3 = new CommMessage(CommMessage.MessageType.request);
            csndMsg3.command = "tEST hARNESS";
            csndMsg3.author = "Prannoy";
            csndMsg3.to = "http://localhost:8083/IPluggableComm";
            csndMsg3.from = "http://localhost:8082/IPluggableComm";// different ports for different communication levels 
            csndMsg3.arguments = builderFiles;
            comm.postMessage(csndMsg3);
        }

        static void Main(string[] args)
        {
            Repository r = new Repository();
            Console.WriteLine("*************************************");
            Console.WriteLine("            REPOSITORY ");
            Console.WriteLine("*************************************");

            
            
            //sendFirstMessage();
            //createMotherMessage();
        }
    }
}
