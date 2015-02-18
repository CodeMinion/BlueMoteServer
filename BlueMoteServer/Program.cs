using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace BlueMoteServer
{
    /// <summary>
    /// Simple Bluetooth Server using 32 feet library. 
    /// Intended to be used along with the BlueMote mobile app 
    /// to allow you to have some minor control over your PC.
    /// </summary>
    class Program
    {
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;

        private const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
        private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
        private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;
        private const int APPCOMMAND_MEDIA_PLAY = 0x2E0000;
        private const int APPCOMMAND_MEDIA_PAUSE = 0x2F0000;
        
        private const int WM_APPCOMMAND = 0x319;

        private const String MESSAGE_VOLUME_UP = "VOLUME_UP";
        private const String MESSAGE_VOLUME_DOWN = "VOLUME_DOWN";
        private const String MESSAGE_VOLUME_MUTE = "VOLUME_MUTE";

        private const String MESSAGE_MEDIA_NEXTTRACK = "MEDIA_NEXTTRACK";
        private const String MESSAGE_MEDIA_PLAY_PAUSE = "MEDIA_PLAY_PAUSE";
        private const String MESSAGE_MEDIA_PLAY = "MEDIA_PLAY";
        private const String MESSAGE_MEDIA_PAUSE = "MEDIA_PAUSE";
        private const String MESSAGE_MEDIA_PREVIOUSTRACK = "MEDIA_PREVIOUSTRACK";

        private const String MESSAGE_HIBERNATE = "COMPUTER_HIBERNATE";

        private const String MESSAGE_MOUSE_DRAG = "EVENT_DRAG";
        private const String MESSAGE_MOUSE_RIGHT_CLICK = "EVENT_RIGHT_CLICK";
        private const String MESSAGE_MOUSE_LEFT_CLICK = "EVENT_LEFT_CLICK";



        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


        private static bool bConnected = false;

        static void Main(string[] args)
        {
            Guid serviceClass = new Guid("{1d374b6f-4e12-4126-abec-5e92daf7c434}");

            //serviceClass = BluetoothService.SerialPort;

            var lsnt = new BluetoothListener(serviceClass);

            lsnt.Start();

            while (true)
            {
                Console.Clear();

                Console.WriteLine("**************************************************************************\n");
                Console.WriteLine("BlueMote Bluetooth Server v0.01 \n");
                Console.WriteLine("Developed by: Frank E. Hernandez\n");
                Console.WriteLine("Email: Hernandez.Frank@Gmail.com\n");
                Console.WriteLine("About:");
                Console.WriteLine("Very simple Bluetooth server companion to the BlueMote Android App.");
                Console.WriteLine("BlueMote App allows you for some simple manipulation of your PC from your Android device.");
                Console.WriteLine("\nCurrent Supported Features:");
                Console.WriteLine("-Volume Controls (Up, Down, Mute)");
                Console.WriteLine("-Media Controls (Play, Pause, Next Track, Previous Track)");
                Console.WriteLine("-Mouse Controls (Move, Left Click, Right Click)");
                Console.WriteLine("-Sleep (Set a sleep time for your PC)");

                Console.WriteLine("\n**************************************************************************\n");
                

                Console.WriteLine("Waiting for client...");

                BluetoothClient conn = lsnt.AcceptBluetoothClient();

                Console.WriteLine("Client Connected...");
                Stream peerStream = conn.GetStream();

                bConnected = true;
                ThreadPool.QueueUserWorkItem(ReadMessagesToEnd, peerStream);

                // Hold the console until enter is pressed.
                //Console.ReadLine();
                while (bConnected)
                {

                }
                conn.Close();
            }
        }

        /// <summary>
        /// Handle the reading of data from the remote device. 
        /// This should be run on a separate thread.
        /// </summary>
        /// <param name="state"></param>
        private static void ReadMessagesToEnd(Object state)
        {
            Stream peer = (Stream)state;
            var rdr = new StreamReader(peer);
            while (true)
            {
                string line;

                try
                {
                    line = rdr.ReadLine();

                    if(line != null)
                    {
                        processCommand(line);
                    }
                    //Console.WriteLine(line);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Connection Ended");
                    break;
                }

                if (line == null)
                {
                    Console.WriteLine("Connection Ended");
                    break;
                }
            }
            bConnected = false;
        }

        /// <summary>
        /// Process the incoming commands from the Remote Device. 
        /// </summary>
        /// <param name="command">Command received.</param>
        private static void processCommand(String command)
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle; 
            
            if (MESSAGE_VOLUME_MUTE.CompareTo(command) == 0)
            {
                Console.WriteLine("Muting Volume");
                
                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
            }
            else if (MESSAGE_VOLUME_UP.CompareTo(command) == 0)
            {
                Console.WriteLine("Raising Volume");
                
                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
            }
            else if (MESSAGE_VOLUME_DOWN.CompareTo(command) == 0)
            {
                Console.WriteLine("Lowering Volume");
                
                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
            }
            else if (MESSAGE_MEDIA_NEXTTRACK.CompareTo(command) == 0)
            {
                Console.WriteLine("Media Next Track");

                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
            }
            else if (MESSAGE_MEDIA_PLAY_PAUSE.CompareTo(command) == 0)
            {
                Console.WriteLine("Media PLAY/Pause Track");

                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
            }
            else if (MESSAGE_MEDIA_PLAY.CompareTo(command) == 0)
            {
                Console.WriteLine("Media PLAY Track");

                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_MEDIA_PLAY);
            }
            else if (MESSAGE_MEDIA_PAUSE.CompareTo(command) == 0)
            {
                Console.WriteLine("Media PAUSE Track");

                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_MEDIA_PAUSE);
            }
            else if (MESSAGE_MEDIA_PREVIOUSTRACK.CompareTo(command) == 0)
            {
                Console.WriteLine("Media Previous Track");

                SendMessageW(handle, WM_APPCOMMAND, handle,
                (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
            }

            else if (MESSAGE_HIBERNATE.CompareTo(command) == 0)
            {
                Console.WriteLine("Hibernating Computer");

                // Put computer to sleep instead.
                // Params: Hibernate, ForceCritical, DisableWakeEvent
                SetSuspendState(false, false, false);
            }

            else if (command.Contains(MESSAGE_MOUSE_DRAG))
            {
                Console.WriteLine("Moving Mouse ");

                string[] cmd = command.Split(new char[]{' '});

                int dx = (int)Convert.ToDouble(cmd[1]);
                int dy = (int)Convert.ToDouble(cmd[2]);


                Cursor.Position = new Point(Cursor.Position.X + dx, Cursor.Position.Y + dy);

            }
            else if (MESSAGE_MOUSE_RIGHT_CLICK.CompareTo(command) == 0)
            {
                Console.WriteLine("Mouse Right Click");
                MouseHandler.ClickRightMouseButton();
            }

            else if (MESSAGE_MOUSE_LEFT_CLICK.CompareTo(command) == 0)
            {
                Console.WriteLine("Mouse LEFT Click");
                MouseHandler.ClickLeftMouseButton();
            }

        }

        
    }
}
