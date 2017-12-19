/////////////////////////////////////////////////////////////////////////////////
// TestExecutive.cs : Displays all the requirements                            //
//                                                                             // 
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: It displays all the project requirements               
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executive
{
    class TestExecutive
    {
        // the test executive mentions all the requirements for the project 4.
        static void Main(string[] args)
        {
            Console.WriteLine("Test Executive");
            Console.WriteLine("================\n");
            Console.WriteLine("Requirement 1");
            Console.WriteLine("***************");
            Console.WriteLine("Shall be prepared using C#, the .Net Frameowrk, and Visual Studio 2017.\n");         
            Console.WriteLine("Requirement 2");
            Console.WriteLine("***************");
            Console.WriteLine("Shall include a Message-Passing Communication Service built with WCF. It is expected that you will build on your Project #3 Comm Prototype.\n");            
            Console.WriteLine("Requirement 3");
            Console.WriteLine("***************");
            Console.WriteLine("The Communication Service shall support accessing build requests by Pool Processes from the mother Builder process, sending and receiving build requests, and sending and receiving files.\n");            
            Console.WriteLine("Requirement 4");
            Console.WriteLine("***************");
            Console.WriteLine("Shall provide a Repository server that supports client browsing to find files to build, builds an XML build request string and sends that and the cited files to the Build Server.\n");            
            Console.WriteLine("Requirement 5");
            Console.WriteLine("***************");
            Console.WriteLine("Shall provide a Process Pool component that creates a specified number of processes on command.\n");            
            Console.WriteLine("Requirement 6");
            Console.WriteLine("***************");
            Console.WriteLine("Pool Processes shall use message-passing communication to access messages from the mother Builder process.\n");         
            Console.WriteLine("Requirement 7");
            Console.WriteLine("***************");
            Console.WriteLine("Each Pool Process shall attempt to build each library, cited in a retrieved build request, logging warnings and errors.\n");            
            Console.WriteLine("Requirement 8");
            Console.WriteLine("***************");
            Console.WriteLine("If the build succeeds, shall send a test request and libraries to the Test Harness for execution, and shall send the build log to the repository.\n");
            Console.WriteLine("Requirement 9");
            Console.WriteLine("***************");
            Console.WriteLine("The Test Harness shall attempt to load each test library it receives and execute it. It shall submit the results of testing to the Repository.\n");
            Console.WriteLine("Requirement 10");
            Console.WriteLine("***************");
            Console.WriteLine("Shall include a Graphical User Interface, built using WPF.\n");
            Console.WriteLine("Requirement 11");
            Console.WriteLine("***************");
            Console.WriteLine("The GUI client shall be a separate process, implemented with WPF and using message-passing communication. It shall provide mechanisms to get \nfile lists from the Repository, and select files for packaging into a test library.\n");
            Console.WriteLine("Requirement 12");
            Console.WriteLine("***************");
            Console.WriteLine("The client shall send build request structures to the repository for storage and transmission to the Build Server.\n");
            Console.WriteLine("Requirement 13");
            Console.WriteLine("***************");
            Console.WriteLine("The client shall be able to request the repository to send a build request in its storage to the Build Server for build processing.\n");
            Console.ReadLine();
        }
    }
}








