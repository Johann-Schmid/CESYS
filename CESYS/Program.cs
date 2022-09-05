/*
 * Console sample project
 * 
 * This project shows the useage of the CESYS api to read two AD channel of a CEBO-stick
 * 
 * Written by Johann Schmid <johann.schmid@ur.de>, 2022
*/

using System;
using System.Collections.Generic;
using CeboMsrNet;
using System.IO;
using Range = CeboMsrNet.Range;
using Timer = System.Timers.Timer;
using System.Timers;

/**
 * CeboMsr - C# example - Info.
 *
 * Print out various information about the device.
 */
namespace CESYS
{
    class ceSys
    {
        Device device = null;
        private static int time = 0;

        private IList<String> frameString = new List<String>();

        public static Timer aTimer = new System.Timers.Timer();

        static readonly object _locker = new object();

        private void setupDevice()
        {
            try
            {
                // Search for devices ...
                IList<Device> devices = LibraryInterface.Enumerate(DeviceType.All);

                // If at least one has been found, use the first one ...
                if (devices.Count > 0)
                {
                    device = devices[0];

                    // Open device, nothing can be done without doing this.
                    device.Open();

                    IInput[] inputs = new IInput[] {
                    device.SingleEndedInputs[0],
                    device.SingleEndedInputs[1],
                    };

                    device.SetupInputFrame(inputs);

                    aTimer.Start();

                    device.StartContinuousDataAcquisition(10, false);

                    // Put out info.
                    //PrintInformation(device);
                    //readValues(endTime);

                    // Finalize device usage, this free's up the device, so it can be used
                    // again, including other applications.


                }
            }
            // These exceptions are currently used to report different problems ...
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void closeDevice()
        {
            if (device != null)
            {
                aTimer.Stop();
                aTimer.Dispose();

                device.ResetDevice();
                device.Close();
            }
        }

        public void readDevice()
        {
            IList<InputFrame> frame = device.ReadNonBlocking();

            foreach (InputFrame inFrame in frame)
            {
                frameString.Add(time.ToString() + ";" + inFrame.GetSingleEnded(0) + ";" + inFrame.GetSingleEnded(1) + ";");

                Console.WriteLine(time.ToString() +
                " SingleEnded #0: " + inFrame.GetSingleEnded(0) + " V, " +
                "SingleEnded #1: " + inFrame.GetSingleEnded(1) + " V");
            }


        }

        private static void OnTimerEvent(object sender, ElapsedEventArgs e, ceSys myCeSys)
        {
            lock (_locker)
            {
                time++;
                myCeSys.readDevice();
            }

        }

        static public void Main(string[] args)
        {
            int timeArgs = 10;
            ceSys myCeSys = new ceSys();
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();

            Console.Title = "Press ESC to stop reading...";

            aTimer.Elapsed += (sender, e) => OnTimerEvent(sender, e, myCeSys);
            aTimer.Interval = 1000;

            DateTime beginTime = DateTime.Now;

            //drawGrid.png();

            if (args.Length == 1)
            {
                timeArgs = Convert.ToInt32(args[0]);

            }


            myCeSys.setupDevice();

            while (time < timeArgs && keyInfo.Key != ConsoleKey.Escape)
            {
                lock (_locker)
                {
                    File.AppendAllLines(@"WriteText.csv", myCeSys.frameString);
                    myCeSys.frameString.Clear();
                }
                if(Console.KeyAvailable == true)
                {
                    keyInfo = Console.ReadKey();
                }
            }

            myCeSys.closeDevice();

            DateTime endTime = DateTime.Now;
            Int64 elapsedTicks = endTime.Ticks - beginTime.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            Console.WriteLine("Elapsed Seconds: " + elapsedSpan.TotalSeconds.ToString());
            File.AppendAllText(@"WriteText.csv", elapsedSpan.TotalSeconds.ToString());
        }

    }
}

