/////////////////////////////////////////////////////////////////////////////////
// Dll_Loader.cs : Evaluates the test request and sends the test logs to the   //                 
//                 repository.                                                  // 
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: If user has entered args on command line then DllLoader assumes that the
 * first parameter is the path to a directory with testers to run.*
 * 
 * Otherwise DllLoader checks if it is running from a debug directory.
 * 1.  If so, it assumes the testers directory is "../../Testers"
 * 2.  If not, it assumes the testers directory is "../testers"
 * 
 * If none of these are the case, then DllLoader emits an error message and
 * quits.
 * 
 * It sends the test logs to the Repository Storage which is displayed on the GUI.
 */

using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SWTools;
//using MsgPassing;
using MessagePassingComm;
using Federation;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;

namespace Federation
{
    public class TestHarnessMock
    {
        public static Comm comm;
        const int port = 8083;
        const string testHarnessAddress = "http://localhost:8083/IPluggableComm";       
        public static string testersLocation { get; set; } = "../../../TestHarnessStorage";
        public static int th_count = 1;
        private static string dllName;
        // constructor
        public TestHarnessMock(int a)
        {
            comm = new Comm("http://localhost", 8083);
            Thread TestHandler = new Thread(processMessages);
            TestHandler.Start();
        }

        // constructor
        public TestHarnessMock()
        { }

        // processes all the messages recieved
        public static void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    if (!msg.type.Equals("connect"))
                    {
                        msg.show();
                        if(msg.command == "Sending DLL to the test harness")
                        {
                            dllName = msg.arguments[0];
                            Console.WriteLine("DLL {0} recieved by test harness", msg.arguments[0]);

                            CommMessage csndMsg1 = new CommMessage(CommMessage.MessageType.request);

                            csndMsg1.command = "test request";
                            csndMsg1.author = "Prannoy";
                            csndMsg1.to = msg.from;
                            csndMsg1.from = "http://localhost:8083/IPluggableComm";
                            csndMsg1.arguments.Add(msg.arguments[0]);
                            comm.postMessage(csndMsg1);
                        }
                        if(msg.command == "test request")
                        {
                            evaluateTestRequest(msg.arguments[0]);                           
                        }
                    }
                }
            }
        }

        // sends the logs generated to the Repo Store
        public static void evaluateTestRequest(string dllName)
        {
            TestHarnessMock.testersLocation = Path.GetFullPath(TestHarnessMock.testersLocation);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", TestHarnessMock.testersLocation);
            TestHarnessMock loader = new TestHarnessMock();
            //msg.arguments contains the DLL Name                            
            string result = loader.loadAndExerciseTesters(dllName);
            string[] DLL_Contents = { "Test Results","Author: Prannoy Singh", "Date: " +
                            DateTime.UtcNow.Date.ToString("dd/MM/yyyy"),"Time: " +
                            string.Format("{0:HH:mm:ss tt}", DateTime.Now) ,"Build Status:"};
            DLL_Contents[4] = result;

            string absPath = Path.GetFullPath("../../../TestHarnessStorage/" + dllName + "_TestLog.txt");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(absPath))
            {
                foreach (string line in DLL_Contents)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file.
                    file.WriteLine(line);

                }
            }

            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "Sending test logs to the repository";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8083/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments.Add(dllName + "_TestLog.txt");

            comm.postFile2(dllName + "_TestLog.txt");

            comm.postMessage(csndMsg);
            //Console.Write("\n\n the result is {0}", result);
            Console.Write("\n\n");
        }
        

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  called binding error event handler");
            //string folderPath = testersLocation;
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        
        //----< load assemblies from testersLocation and run their tests >-----
        public string loadAndExerciseTesters(string testName)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);
            try
            {
                TestHarnessMock loader = new TestHarnessMock();
                string[] files = { Path.GetFullPath("../../../TestHarnessStorage/" + testName) };
                foreach (string file in files)
                {
                    //Assembly asm = Assembly.LoadFrom(file);
                    Assembly asm = Assembly.LoadFile(file);
                    string fileName = Path.GetFileName(file);
                    Console.Write("\n The file loaded is  {0}\n", fileName);
                    Console.WriteLine("==============================================");
                    // exercise each tester found in assembly
                    Type[] types = asm.GetTypes();
                    foreach (Type t in types)
                    {
                        // if type supports ITest interface then run test
                        if (t.GetInterface("DllLoaderDemo.ITest", true) != null)
                        {
                            if (!loader.runSimulatedTest(t, asm))
                                Console.Write("\n The test {0} failed to run", t.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("==================================");
                return ex.Message;
            }
            return "\n\n Simulated Testing completed";
        }
        //
        //----< run tester t from assembly asm >-------------------------------

        bool runSimulatedTest(Type t, Assembly asm)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("\nCreating instance of {0}", t.ToString());
                Console.WriteLine("==============================================");
                object obj = asm.CreateInstance(t.ToString());
                // run test
                bool status = false;
                MethodInfo method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);

                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)
                        return "Passed";
                    return "Failed";
                };
                Console.Write("\n  test {0}", act(status));
            }
            catch (Exception ex)
            {
                Console.Write("\n  test failed with message \"{0}\"", ex.Message);
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            //  You would think that the code below should work, but it fails
            //  with invalidcast exception, even though the types are correct.
            //
            //    DllLoaderDemo.ITest tester = (DllLoaderDemo.ITest)obj;
            //    tester.say();
            //    tester.test();
            //
            //  This is a design feature of the .Net loader.  If code is loaded 
            //  from two different sources, then it is considered incompatible
            //  and typecasts fail, even thought types are Liskov substitutable.
            //
            return true;
        }

        //----< run demonstration >--------------------------------------------

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("*************************************");
            Console.WriteLine("            TEST HARNESS ");
            Console.WriteLine("*************************************");

            TestHarnessMock loader = new TestHarnessMock(0);
        }
    }
}

