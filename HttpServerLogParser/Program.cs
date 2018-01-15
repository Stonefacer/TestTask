using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Ninject;

using LogParser;
using LogParser.LinesSource;
using GeoLocation;

using HttpServerLogParser.Settings;

namespace HttpServerLogParser
{

    class Program
    {
        private const string HelpMessage = @"
usage:
    HttpServerLogParser.exe [/t <number>] <file to parse>
arguments:
    /t - threads count (optional)
    file to parse - path to the file needed to be parsed
examples:
    HttpServerLogParser.exe ""access_log_Jul95""
    HttpServerLogParser.exe /t 4 ""access_log_Jul95""
warning:
    only default log format is supported (which is ""%h %l %u %t \""%r\"" %>s %b"")
";

        private static Object _syncOutput = new Object();

        static void Main(string[] args)
        {
            var ninject = NinjectCommon.Instance();
            var settings = ninject.Kernel.Get<ApplicationSettings>();

            // default settings
            settings.ThreadsCount = 1;
            settings.Cancelation = new CancellationTokenSource();

            // parse command line
            if (!ParseCommandLineArguments(args))
            {
                return; // command line arguments cannot be parsed
            }
            // load settings from file
            var fileSettings = LoadSettingsFile();
            if (fileSettings == null)
            {
                PrintMessage("Settings cannot be loaded. Please check settings.xml file.");
                return;
            }
            ninject.BindFileSettings(fileSettings);

            // output settings
            Console.WriteLine("Filename: {0}", settings.Filename);
            Console.WriteLine("Connection settings: {0}", fileSettings.ConnectionString);
            Console.WriteLine("Threads count: {0}", settings.ThreadsCount);

            // check file existance
            var fileInfo = new FileInfo(settings.Filename);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("File \"{0}\" doesn't exist", fileInfo.FullName);
                return;
            }

            // bind rest of interfaces
            ninject.BindDatabaseProvider(fileSettings.ConnectionString);
            ninject.BindLinesSource(fileInfo);

            // bind ctrl+c event so we could free resources correctly
            Console.CancelKeyPress += Console_CancelKeyPress;

            // create and start worker threads
            var threads = CreateWorkers();

            // save start date time for benchmarks
            var startDatetime = DateTime.Now;

            // wait for threads to exit
            var linesSource = ninject.Kernel.Get<ILinesSource>();
            for (var i = 0; i < threads.Length; i++)
            {
                while (!threads[i].Join(5000))
                {
                    // output progress any 5 seconds
                    Console.WriteLine("File progress: {0}/{1} ({2:F03}%); Current buffer progress: {3}/{4} ({5:F03}%)",
                        linesSource.FilePosition, linesSource.FileTotalBytes, linesSource.FilePosition / (double)linesSource.FileTotalBytes * 100.0,
                        linesSource.CurrentBufferPosition, linesSource.BufferSize, linesSource.CurrentBufferPosition / (double)linesSource.BufferSize * 100.0);
                }
            }

            var timeSpend = DateTime.Now - startDatetime;
            Console.WriteLine("Done in {0:00}:{1:00}:{2:00}.{3:000}", timeSpend.TotalHours, timeSpend.Minutes, timeSpend.Seconds, timeSpend.Milliseconds);
        }

        private static FileSettings LoadSettingsFile()
        {
            if (!File.Exists("settings.xml"))
            {
                return null;
            }
            var xmlSerializer = new XmlSerializer(typeof(FileSettings));
            using (var streamReader = new StreamReader("settings.xml"))
            {
                var fileSettings = new FileSettings();
                return xmlSerializer.Deserialize(streamReader) as FileSettings;
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            PrintMessage("Exiting...");
            var ninject = NinjectCommon.Instance();
            var settings = ninject.Kernel.Get<ApplicationSettings>();
            // request termination of worker threads
            settings.Cancelation.Cancel();
            // cancel process termination
            e.Cancel = true;
        }

        /// <summary>
        /// Print help message
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine(HelpMessage);
        }

        /// <summary>
        /// Prints given message
        /// </summary>
        /// <param name="message">message to print</param>
        private static void PrintMessage(string message)
        {
            lock (_syncOutput)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Output error message based on failed line and exception
        /// </summary>
        /// <param name="lineWithError">failed line</param>
        /// <param name="exception">thrown exception</param>
        private static void PrintFailedMessage(string lineWithError, Exception exception)
        {
            lock (_syncOutput)
            {
                Console.WriteLine("Failed to parse line: \"{0}\"", lineWithError);
                Console.WriteLine("Thread terminated!");
                Console.WriteLine("-------------Exceptions------------");
                while (exception != null)
                {
                    Console.WriteLine("Type: {0}", exception.GetType().FullName);
                    Console.WriteLine("Message: {0}", exception.Message);
                    Console.WriteLine("Stack trace: {0}", exception.StackTrace);
                    Console.WriteLine();
                    exception = exception.InnerException;
                }
            }
        }

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <returns>returns true if sucess and false otherwise</returns>
        private static bool ParseCommandLineArguments(string[] args)
        {
            var ninject = NinjectCommon.Instance();
            var settings = ninject.Kernel.Get<ApplicationSettings>();
            if (args.Length < 1)
            {
                PrintHelp();
                return false;
            }
            for (var argumentIndex = 0; argumentIndex < args.Length; argumentIndex++)
            {
                var currentArgument = args[argumentIndex];
                if (currentArgument == "/t") // set count of threads
                {
                    if (args.Length < 3) // one or two arguments missed
                    {
                        PrintHelp();
                        return false;
                    }
                    argumentIndex++;
                    // check does threads count is correct number
                    int threadsCount;
                    if (!int.TryParse(args[argumentIndex], out threadsCount))
                    {
                        Console.WriteLine("Count of threads is not a number");
                        return false;
                    }
                    if (threadsCount < 1)
                    {
                        Console.WriteLine("Count of threads must be greater than zero");
                        return false;
                    }
                    settings.ThreadsCount = threadsCount;
                }
                else
                {
                    settings.Filename = currentArgument;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates array of threads to process a file
        /// </summary>
        /// <returns>created array of threads</returns>
        private static Thread[] CreateWorkers()
        {
            var ninject = NinjectCommon.Instance();
            var settings = ninject.Kernel.Get<ApplicationSettings>();
            var threads = new Thread[settings.ThreadsCount];
            for (int i = 0; i < settings.ThreadsCount; i++)
            {
                threads[i] = new Thread(ThreadWorker);
                threads[i].Start();
                threads[i].Priority = ThreadPriority.Lowest;
            }
            return threads;
        }

        /// <summary>
        /// Start method for each thread
        /// </summary>
        private static void ThreadWorker()
        {
            var ninject = NinjectCommon.Instance();
            var settings = ninject.Kernel.Get<ApplicationSettings>();
            var worker = ninject.Kernel.Get<Worker>();
            try
            {
                worker.ProcessFile(settings.Cancelation.Token);
            }
            catch (OperationCanceledException)
            {
                // cancelation requested
            }
            catch (Exception ex)
            {
                PrintFailedMessage(worker.CurrentLine, ex);
            }
        }
    }
}
