using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace PublicDomain
{
    internal sealed class Program
	{
        public static int BytesRead = 0;
        static string tempFileSpeaker;
        static string tempFileMic;
        static string outputFile;

		static void Main(string[] args)
		{
            Console.WriteLine(@"OpenSourceSkypeRecorder -- record a skype call to an audio file
");
            outputFile = null;

            if (args != null && args.Length > 0)
            {
                outputFile = args[0];
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.Write("Type output file name: ");
                outputFile = Console.ReadLine();
            }

            tempFileSpeaker = Path.GetTempFileName();
            tempFileMic = Path.GetTempFileName();
            try
            {
                Console.WriteLine("Attaching to Skype...");

                new SkypeAttachment(new AttachedCallback(CallBack));

                Console.WriteLine(@"
Press Enter at any time to quit.
You can also type a command (e.g. ""mp3""). To see all commands, type help.
");

                bool go = true;
                while (go)
                {
                    string command = Console.ReadLine();
                    if (command == null)
                    {
                        break;
                    }
                    command = command.Trim().ToLower();
                    switch (command)
                    {
                        case "help":
                            Console.WriteLine(@"
Commands:
    open: If the call is finished, open the resulting audio file
    mp3: If the call is finished, convert the audio file to an MP3
");
                            break;
                        case "open":
                            if (File.Exists(outputFile))
                            {
                                Console.WriteLine("Launching in default media player...");
                                Process.Start(outputFile);
                            }
                            else
                            {
                                Console.WriteLine("No audio file exists yet");
                            }
                            break;
                        case "mp3":
                            // http://www.codeproject.com/KB/audio-video/MP3Compressor.aspx
                            Console.WriteLine("Not implemented yet");
                            break;
                        default:
                            go = false;
                            break;
                    }
                }
            }
            finally
            {
                File.Delete(tempFileSpeaker);
                File.Delete(tempFileMic);
            }
		}

		private static void CallBack(SkypeRecorder skypeRecorder)
		{
			skypeRecorder.MicrophoneWavFile = tempFileMic;
			skypeRecorder.SpeakerWavFile = tempFileSpeaker;
            skypeRecorder.OutputWaveFile = outputFile;
		}
	}
}
