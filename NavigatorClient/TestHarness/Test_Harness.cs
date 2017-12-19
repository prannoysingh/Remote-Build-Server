/////////////////////////////////////////////////////////////////////////////////
// testHarness.cs : generates the XML request and sends it to the repository   //
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: The test harness will process all the dll files present in the 
 *              test harness storage folder, and run the test cases on them 
 *              accordingly, the results produced are displayed on the console.
 *              
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SWTools;
//using MsgPassing;
using MessagePassingComm;
using Federation;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;

namespace Test_Harness
{
    public class testHarness
    {    
        protected Thread thrd = null;

        //public testHarness()
        //{
        //    Console.WriteLine("--------------------------------------");
        //    Console.WriteLine("              TEST HARNESS");
        //    Console.WriteLine("--------------------------------------");
        //    start();
        //}

        //public Thread start()
        //{
        //    thrd = new Thread(
        //        () =>
        //        {
        //            while (true)
        //            {
        //                Message msg = BlockingQueue<Message>.BuildTest.deQ();
        //                //Console.Write("\n  {0}", msg.ToString());
        //                processMessage(msg);
        //                if (msg.body == "quit")
        //                {
        //                    Console.Write("\n  {0} thread quitting", msg.to);
        //                    break;
        //                }
        //            }
        //        }
        //       );
        //    thrd.IsBackground = true;
        //    thrd.Start();
        //    return thrd;
        //}

        

        //// evaluates request recieved from the build server
        //public  void processMessage(Message msg)
        //{
        //    Console.WriteLine("the test harness is here ;)");
        //    TestHarnessMock.testersLocation = Path.GetFullPath(TestHarnessMock.testersLocation);
        //    TestHarnessMock loader = new TestHarnessMock();
        //    string result = loader.loadAndExerciseTesters();
        //    // Console.WriteLine(" the th_count is " + TestHarnessMock.th_count);
        //}
        
        //static void Main(string[] args)
        //{
            
        //}
    }
}
