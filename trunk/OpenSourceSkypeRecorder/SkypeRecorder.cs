using System;
using SKYPE4COMLib;
using System.IO;

namespace PublicDomain
{
	public class SkypeRecorder
	{
        public const int SkypeWavHeaderSize = 46;

		private Skype m_skype;
		private string m_microphoneWavFile;
		private string m_speakerWavFile;
        private string m_outputWavFile;

		internal SkypeRecorder(Skype skype)
		{
			m_skype = skype;
			m_skype.CallStatus += new _ISkypeEvents_CallStatusEventHandler(EventCallStatus);
		}

		internal void EventCallStatus(Call call, TCallStatus status)
		{
			Console.WriteLine("New Status: " + m_skype.Convert.CallStatusToText(status));
			
			if (status == TCallStatus.clsInProgress)
			{
                call.set_CaptureMicDevice(TCallIoDeviceType.callIoDeviceTypeFile, MicrophoneWavFile);
                call.set_OutputDevice(TCallIoDeviceType.callIoDeviceTypeFile, SpeakerWavFile);
			}
            else if (status == TCallStatus.clsFinished)
            {
                // the call is finished, so write the wave file
                Console.WriteLine("Call finished, writing final file {0}...", OutputWaveFile);

                FinishCall();

                Console.WriteLine("Finished writing audio file");
            }
		}

        private void FinishCall()
        {
            if (File.Exists(m_outputWavFile))
            {
                File.Delete(m_outputWavFile);
            }

            clsWaveProcessor processor = new clsWaveProcessor();
            processor.HeaderLength = SkypeWavHeaderSize;
            if (!processor.WaveMix(SpeakerWavFile, MicrophoneWavFile))
            {
                Console.WriteLine("Failed mixing speaker with microphone");
            }
            File.Copy(SpeakerWavFile, OutputWaveFile);
        }

		public string MicrophoneWavFile
		{
			get
			{
				return m_microphoneWavFile;
			}
			set
			{
				m_microphoneWavFile = value;
			}
		}

		public string SpeakerWavFile
		{
			get
			{
				return m_speakerWavFile;
			}
			set
			{
				m_speakerWavFile = value;
			}
		}

        public string OutputWaveFile
        {
            get
            {
                return m_outputWavFile;
            }
            set
            {
                m_outputWavFile = value;
            }
        }
	}
}
