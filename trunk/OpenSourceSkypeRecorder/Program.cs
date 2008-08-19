using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using WaveLib;
using Yeti.MMedia.Mp3;

namespace PublicDomain
{
    internal sealed class Program
	{
        public static int BytesRead = 0;
        static string tempFileSpeaker;
        static string tempFileMic;
        static string outputFile;
        static string outputMp3File;

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
                                if (!string.IsNullOrEmpty(outputMp3File))
                                {
                                    Process.Start(outputMp3File);
                                }
                                else
                                {
                                    Process.Start(outputFile);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No audio file exists yet");
                            }
                            break;
                        case "mp3":
                            // http://www.codeproject.com/KB/audio-video/MP3Compressor.aspx
                            if (File.Exists(outputFile))
                            {
                                Console.Write("Path to new MP3 file: ");
                                outputMp3File = Console.ReadLine();
                                ConvertToMp3(outputFile, outputMp3File);
                                Console.WriteLine("Finished writing MP3 file");
                            }
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

        private static void ConvertToMp3(string outputFile, string mp3file)
        {
            using (WaveStream wavStream = new WaveStream(outputFile))
            {
                using (Mp3Writer writer = new Mp3Writer(new FileStream(mp3file,
                                                    FileMode.Create), wavStream.Format))
                {
                    byte[] buff = new byte[writer.OptimalBufferSize];
                    int read = 0;
                    while ((read = wavStream.Read(buff, 0, buff.Length)) > 0)
                    {
                        writer.Write(buff, 0, read);
                    }
                }
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
