/////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs : Creates Client GUI                                     //
//                                                                             // 
// ver 1.0                                                                     //
//                                                                             //
// Platform     : HP Pavilion, Windows 10 Pro x64, Visual Studio 2017          //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Prannoy Singh, EECS Department, Syracuse University          //
//                (315)-728-8099, psingh07@syr.edu                             //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: Provides GUI for the client. 
 *              
 * MainWindow_Load() -> automates the test request and initiates the child processes.             
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MsgPassing;
using NavigatorClient;
using MessagePassingComm;
using SpawnProc;
using System.Threading;

namespace NavigatorClient
{
    public partial class MainWindow : Window
    {
        static Comm comm;
        private IFileMgr fileMgr = null;  // note: Navigator just uses interface declarations
        Stack<string> testElement = new Stack<string>();
        string testDriver = "";
        static int index = 1;

        static List<string> testnames { get; set; } = new List<string>();
        public MainWindow()
        {
            comm = new Comm("http://localhost", 8079);
            InitializeComponent();
            fileMgr = FileMgrFactory.create(FileMgrType.Local);
            getTopFiles();
            Thread GUI_Handler = new Thread(processMessages);
            GUI_Handler.Start();
            MainWindow_Load();

            //sendMessage();

        }

        // automate the test request
        private void MainWindow_Load()
        {
            initialize_Child_Processes();
            // Request 1
            initialize_TestRequest();
            initialize_XML();
            // Request 2
            initialize_TestRequest();
            initialize_XML();
            // Request 3
            initialize_TestRequest();
            initialize_XML();
            // Request 4
            initialize_TestRequest();
            initialize_XML();
            // Request 5
            initialize_TestRequest();
            initialize_XML();
        }

        //automate test driver and test element
        private void initialize_TestRequest()
        {
            List<string> testNames = new List<string>();
            testNames.Add("Interfaces.cs");
            testNames.Add("TestLib.cs");
            testNames.Add("TestedLibDependency.cs");
            testNames.Add("TestedLib.cs");
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "create Test Request";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments = testNames;
            comm.postMessage(csndMsg);

        }

        // initializing the XML build request
        private void initialize_XML()
        {
            List<string> ind = new List<string>();
            ind.Add(index.ToString());
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "Create XML File";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments = ind;
            comm.postMessage(csndMsg);
            index++;

        }

        // invokes child process automatically
        private void initialize_Child_Processes()
        {
            string s = t1.Text = "5";
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            // it will contact the mother for creating of child processes
            csndMsg.command = "Create Process";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments.Add(s);
            comm.postMessage(csndMsg);
        }

        // evaluates the messges recieved by the GUI Client
        public void processMessages()
        {
            while (true)
            {
                CommMessage msg = comm.getMessage();
                if (msg.command != null)
                {
                    if (!msg.type.Equals("connect"))
                        msg.show();

                    if (msg.command.Equals("Update File"))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            getTopFiles();
                            if (localFiles.IsLoaded)
                                MessageBox.Show(msg.arguments[0]);
                        });

                    }

                }
            }
        }

        //----< show files and dirs in root path >-----------------------

        static void sendMessage()// message to repository
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "GUI";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            //csndMsg.arguments = allFileName;
            comm.postMessage(csndMsg);
        }


        // gets all the files present in the folder
        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            localFiles1.Items.Clear();
            foreach (string file in files)
            {
                localFiles1.Items.Add(file);
                localFiles.Items.Add(file);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }
        //----< move to directory root and display files and subdirs >---

        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }
        //----< show selected file in code popup window >----------------

        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.localRoot, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        //----< move into subdir and show files and subdirs >------------

        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirName = localDirs.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
        }
        //----< move to root of remote directories >---------------------

        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            // coming soon
        }
        //----< download file and display source in popup window >-------

        private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // comming soon
        }
        //----< move to parent directory of current remote path >--------

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
            // comming soon
        }
        //----< move into remote subdir and display files and subdirs >--

        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // comming soon
        }

        // selects the test driver
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            testnames.Clear();
            string td = "";
            if (localFiles.IsLoaded)
            {
                foreach (string lbi in localFiles.SelectedItems)
                {
                    testnames.Add(lbi);
                    td = lbi;//.Content+""; //l1.ToString();//.Content.ToString();
                    //txt = lbi.Content.ToString();// +"";
                    //s.Push(lbi.Content.ToString());
                }
                //foreach (ListBoxItem lbi in l1.SelectedItems)
                //{
                //                testDriver = l1.ToString();//.Content.ToString();

                //}
            }
            setTestDriver(td);
        }

        // setter
        public void setTestElements(Stack<string> te)
        {
            testElement = te;
        }

        // setter
        public void setTestDriver(string td)
        {
            testDriver = td;
        }

        // returns test elements
        public Stack<string> getTestElements()
        {
            return testElement;
        }

        // returns test driver
        public string getTestDriver()
        {
            return testDriver;
        }

        // adds the test elements to a stack
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Stack<string> s = new Stack<string>();
            if (localFiles1.IsLoaded)
            {
                foreach (string lbi in localFiles1.SelectedItems)
                {
                    //string txt = "";
                    testnames.Add(lbi);
                    // txt = lbi.content.tostring();// +"";
                    s.Push(lbi);//.content.tostring());
                }
            }
            setTestElements(s);
        }

        // creates Test Request
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            //            Repository r = new Repository();
            //            r.createTestRequest(getTestDriver(), getTestElements());

            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "create Test Request";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments = testnames;
            comm.postMessage(csndMsg);

            //testnames.Clear();
        }

        // creates XML File
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //  SEND INDEX IN COMMUNICATION
            //            Repository r = new Repository();
            //            r.createXML(index);
            List<string> ind = new List<string>();
            ind.Add(index.ToString());
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

            csndMsg.command = "Create XML File";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments = ind;
            comm.postMessage(csndMsg);
            index++;
        }

        // builds the XML file selected.
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            testnames.Clear();
            string td = "";
            if (localFiles.IsLoaded)
            {
                foreach (string lbi in localFiles.SelectedItems)
                {
                    testnames.Add(lbi);
                    td = lbi;
                }
            }
            setTestDriver(td);
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            // it will contact the mother for creating of child processes
            csndMsg.command = "Build Xml";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments.Add(getTestDriver());
            comm.postMessage(csndMsg);
        }
        //// Display XML File
        //private void button5_Click(object sender, RoutedEventArgs e)
        //{
        //    //            Repository r = new Repository();
        //    //            MessageBox.Show(r.showXML());
        //    CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

        //    csndMsg.command = "Display XML";
        //    csndMsg.author = "Jim Fawcett";
        //    csndMsg.to = "http://localhost:8082/IPluggableComm";
        //    csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
        //    //csndMsg.arguments = allFileName;
        //    comm.postMessage(csndMsg);
        //}

        // No of process
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            string s = t1.Text;
            // i will start my WCF communication here
            //*Repository r = new Repository();
            //*SpawnProc.SpawnProc mB = new SpawnProc.SpawnProc(s);
            //MotherBuilder b = new MotherBuilder(s);
            //r.sendBuilderMessage(s); //sends message to the builder
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            // it will contact the mother for creating of child processes
            csndMsg.command = "Create Process";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            csndMsg.arguments.Add(s);
            comm.postMessage(csndMsg);
        }

        // Quit message to mother builder
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            // it will contact the mother for creating of child processes
            csndMsg.command = "Quit";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8082/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 
            comm.postMessage(csndMsg);
            sendQuitMother();

        }

        // sends quit message to the mother,to close child processes
        static void sendQuitMother()
        {
            CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);
            // it will contact the mother for creating of child processes
            csndMsg.command = "Quit";
            csndMsg.author = "Prannoy";
            csndMsg.to = "http://localhost:8081/IPluggableComm";
            csndMsg.from = "http://localhost:8079/IPluggableComm";// different ports for different communication levels 

            comm.postMessage(csndMsg);
        }

        private void localFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
