// Copied from http://www.codeproject.com/KB/cs/WAVE_Processor_In_C_.aspx

using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Class Name  : clsWaveProcessor
/// By          : Sujoy G
/// Dt          : 12th Apr 2007
/// 
/// Description :-
/// 
/// The clsWaveProcessor class is intended to encapsulate basic but necessary methods for manipulating 
/// WAVE audio files programmatically. Initially the class is developed to meet basic requirements 
/// of voice mail and telephony applications. That is why the class expects input files in [16bit 8kHz Mono] format. 
/// However, with minor modification, the class can be used for other formats too.
/// 
/// The basic idea is adapted from the CodeProject article "Concatenation Wave Files using C# 2005" by Ehab Mohamed Essa,
/// URL - http://www.codeproject.com/useritems/Concatenation_Wave_Files.asp. Then modified for practical purposes.
/// 
/// The class is highly independent and doesn't require expensive 3rd party libraries to function.
/// <summary>

namespace PublicDomain
{
    public class clsWaveProcessor
    {
        // Constants for default or base format [16bit 8kHz Mono]
        private const short CHNL = 1;
        private const int SMPL_RATE = 8000;
        private const int BIT_PER_SMPL = 16;
        private const short FILTER_FREQ_LOW = -10000;
        private const short FILTER_FREQ_HIGH = 10000;

        // Public Fields can be used for various operations
        public int Length;
        public short Channels;
        public int SampleRate;
        public int DataLength;
        public short BitsPerSample;
        public ushort MaxAudioLevel;
        public int HeaderLength = 44;
        //-----------------------------------------------------
        //Methods Starts Here
        //-----------------------------------------------------
        /// <summary>
        /// Read the wave file header and store the key values in public variable.
        /// Adapted from - Concatenation Wave Files using C# 2005 by By Ehab Mohamed Essa
        /// URL - http://www.codeproject.com/useritems/Concatenation_Wave_Files.asp
        /// </summary>
        /// <param name="strPath">The physical path of wave file incl. file name for reading</param>
        /// <returns>True/False</returns>
        private bool WaveHeaderIN(string strPath)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;

            FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);
            try
            {
                Length = (int)fs.Length - 8;
                fs.Position = 22;
                Channels = br.ReadInt16(); //1
                fs.Position = 24;
                SampleRate = br.ReadInt32(); //8000
                fs.Position = 34;
                BitsPerSample = br.ReadInt16(); //16
                DataLength = (int)fs.Length - HeaderLength;
                byte[] arrfile = new byte[fs.Length - HeaderLength];
                fs.Position = HeaderLength;
                fs.Read(arrfile, 0, arrfile.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                br.Close();
                fs.Close();
            }
            return true;
        }

        /// <summary>
        /// Write default WAVE header to the output. See constants above for default settings.
        /// Adapted from - Concatenation Wave Files using C# 2005 by By Ehab Mohamed Essa
        /// URL - http://www.codeproject.com/useritems/Concatenation_Wave_Files.asp
        /// </summary>
        /// <param name="strPath">The physical path of wave file incl. file name for output</param>
        /// <returns>True/False</returns>
        private bool WaveHeaderOUT(string strPath)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;

            FileStream fs = new FileStream(strPath, FileMode.Create, FileAccess.Write);

            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                fs.Position = 0;
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

                bw.Write(Length);

                bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

                bw.Write((int)16);

                bw.Write((short)1);
                bw.Write(Channels);

                bw.Write(SampleRate);

                bw.Write((int)(SampleRate * ((BitsPerSample * Channels) / 8)));

                bw.Write((short)((BitsPerSample * Channels) / 8));

                bw.Write(BitsPerSample);

                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(DataLength);
            }
            catch
            {
                return false;
            }
            finally
            {
                bw.Close();
                fs.Close();
            }
            return true;
        }

        /// <summary>
        /// Ensure any given wave file path that the file matches with default or base format [16bit 8kHz Mono]
        /// </summary>
        /// <param name="strPath">Wave file path</param>
        /// <returns>True/False</returns>
        public bool Validate(string strPath)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;

            clsWaveProcessor wa_val = new clsWaveProcessor();
            wa_val.WaveHeaderIN(strPath);
            if (wa_val.BitsPerSample != BIT_PER_SMPL) return false;
            if (wa_val.Channels != CHNL) return false;
            if (wa_val.SampleRate != SMPL_RATE) return false;
            return true;
        }

        /// <summary>
        /// Compare two wave files to ensure both are in same format
        /// </summary>
        /// <param name="Wave1">ref. to processor object</param>
        /// <param name="Wave2">ref. to processor object</param>
        /// <returns>True/False</returns>
        private bool Compare(ref clsWaveProcessor Wave1, ref clsWaveProcessor Wave2)
        {
            if (Wave1.Channels != Wave2.Channels) return false;
            if (Wave1.BitsPerSample != Wave2.BitsPerSample) return false;
            if (Wave1.SampleRate != Wave2.SampleRate) return false;
            return true;
        }

        /// <summary>
        /// Increase or decrease volume of a wave file by percentage
        /// </summary>
        /// <param name="strPath">Source wave</param>
        /// <param name="booIncrease">True - Increase, False - Decrease</param>
        /// <param name="shtPcnt">1-100 in %-age</param>
        /// <returns>True/False</returns>
        public bool ChangeVolume(string strPath, bool booIncrease, short shtPcnt)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;
            if (shtPcnt > 100) return false;

            clsWaveProcessor wain = new clsWaveProcessor();
            clsWaveProcessor waout = new clsWaveProcessor();

            waout.DataLength = waout.Length = 0;

            if (!wain.WaveHeaderIN(@strPath)) return false;

            waout.DataLength = wain.DataLength;
            waout.Length = wain.Length;

            waout.BitsPerSample = wain.BitsPerSample;
            waout.Channels = wain.Channels;
            waout.SampleRate = wain.SampleRate;

            byte[] arrfile = GetWAVEData(strPath);


            //change volume
            for (int j = 0; j < arrfile.Length; j += 2)
            {
                short snd = ComplementToSigned(ref arrfile, j);
                try
                {
                    short p = Convert.ToInt16((snd * shtPcnt) / 100);
                    if (booIncrease)
                        snd += p;
                    else
                        snd -= p;
                }
                catch
                {
                    snd = ComplementToSigned(ref arrfile, j);
                }
                byte[] newval = SignedToComplement(snd);
                arrfile[j] = newval[0];
                arrfile[j + 1] = newval[1];
            }

            //write back to the file
            waout.DataLength = arrfile.Length;
            waout.WaveHeaderOUT(@strPath);
            WriteWAVEData(strPath, ref arrfile);

            return true;
        }

        /// <summary>
        /// Mix two wave files. The mixed data will be written back to the main wave file.
        /// </summary>
        /// <param name="strPath">Path for source or main wave file.</param>
        /// <param name="strMixPath">Path for wave file to be mixed with source.</param>
        /// <returns>True/False</returns>
        public bool WaveMix(string strPath, string strMixPath)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;

            if (strMixPath == null) strMixPath = "";
            if (strMixPath == "") return false;

            clsWaveProcessor wain = new clsWaveProcessor();
            clsWaveProcessor wamix = new clsWaveProcessor();
            clsWaveProcessor waout = new clsWaveProcessor();

            wain.DataLength = wamix.Length = 0;

            if (!wain.WaveHeaderIN(strPath)) return false;
            if (!wamix.WaveHeaderIN(strMixPath)) return false;

            waout.DataLength = wain.DataLength;
            waout.Length = wain.Length;

            waout.BitsPerSample = wain.BitsPerSample;
            waout.Channels = wain.Channels;
            waout.SampleRate = wain.SampleRate;

            byte[] arrfile = GetWAVEData(strPath);
            byte[] arrmix = GetWAVEData(strMixPath);

            for (int j = 0, k = 0; j < arrfile.Length; j += 2, k += 2)
            {
                if (k >= arrmix.Length) k = 0;
                short snd1 = ComplementToSigned(ref arrfile, j);
                short snd2 = ComplementToSigned(ref arrmix, k);
                short o = 0;
                // ensure the value is within range of signed short
                if ((snd1 + snd2) >= -32768 && (snd1 + snd2) <= 32767)
                    o = Convert.ToInt16(snd1 + snd2);
                byte[] b = SignedToComplement(o);
                arrfile[j] = b[0];
                arrfile[j + 1] = b[1];
            }

            //write mixed file
            waout.WaveHeaderOUT(@strPath);
            WriteWAVEData(strPath, ref arrfile);

            return true;
        }
        /// <summary>
        /// Filter out sielence or noise from wave file. The noise or silence frequencies are set in filter constants -
        /// FILTER_FREQ_HIGH and FILTER_FREQ_LOW. For a given application, some experimentation may be required in 
        /// begining to decide the HIGH and LOW filter frequencies (alternate suggestion are most welcome).
        /// </summary>
        /// <param name="strPath">Path for wave file</param>
        /// <returns>True/False</returns>
        public bool StripSilence(string strPath)
        {
            if (strPath == null) strPath = "";
            if (strPath == "") return false;

            clsWaveProcessor wain = new clsWaveProcessor();
            clsWaveProcessor waout = new clsWaveProcessor();

            waout.DataLength = waout.Length = 0;

            if (!wain.WaveHeaderIN(@strPath)) return false;

            waout.DataLength = wain.DataLength;
            waout.Length = wain.Length;

            waout.BitsPerSample = wain.BitsPerSample;
            waout.Channels = wain.Channels;
            waout.SampleRate = wain.SampleRate;

            byte[] arrfile = GetWAVEData(strPath);

            //check for silence
            int startpos = 0;
            int endpos = arrfile.Length - 1;
            //At start
            try
            {
                for (int j = 0; j < arrfile.Length; j += 2)
                {
                    short snd = ComplementToSigned(ref arrfile, j);
                    if (snd > FILTER_FREQ_LOW && snd < FILTER_FREQ_HIGH) startpos = j;
                    else
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            //At end
            for (int k = arrfile.Length - 1; k >= 0; k -= 2)
            {
                short snd = ComplementToSigned(ref arrfile, k - 1);
                if (snd > FILTER_FREQ_LOW && snd < FILTER_FREQ_HIGH) endpos = k;
                else
                    break;
            }

            if (startpos == endpos) return false;
            if ((endpos - startpos) < 1) return false;

            byte[] newarr = new byte[(endpos - startpos) + 1];

            for (int ni = 0, m = startpos; m <= endpos; m++, ni++)
                newarr[ni] = arrfile[m];

            //write file
            waout.DataLength = newarr.Length;
            waout.WaveHeaderOUT(@strPath);
            WriteWAVEData(strPath, ref newarr);

            return true;
        }

        /// <summary>
        /// Adapted from - Concatenation Wave Files using C# 2005 by By Ehab Mohamed Essa
        /// URL - http://www.codeproject.com/useritems/Concatenation_Wave_Files.asp
        /// </summary>
        /// <param name="StartFile">Wave file to stay in the begining</param>
        /// <param name="EndFile">Wave file to stay at the end</param>
        /// <param name="OutFile">Merged output to wave file</param>
        /// <returns>True/False</returns>
        public bool Merge(string strStartFile, string strEndFile, string strOutFile)
        {
            if ((strStartFile == strEndFile) && (strStartFile == null)) return false;
            if ((strStartFile == strEndFile) && (strStartFile == "")) return false;
            if ((strStartFile == strOutFile) || (strEndFile == strOutFile)) return false;

            clsWaveProcessor wa_IN_Start = new clsWaveProcessor();
            clsWaveProcessor wa_IN_End = new clsWaveProcessor();
            clsWaveProcessor wa_out = new clsWaveProcessor();

            wa_out.DataLength = 0;
            wa_out.Length = 0;

            try
            {
                //Gather header data
                if (!wa_IN_Start.WaveHeaderIN(@strStartFile)) return false;
                if (!wa_IN_End.WaveHeaderIN(@strEndFile)) return false;
                if (!Compare(ref wa_IN_Start, ref wa_IN_End)) return false;

                wa_out.DataLength = wa_IN_Start.DataLength + wa_IN_End.DataLength;
                wa_out.Length = wa_IN_Start.Length + wa_IN_End.Length;

                //Recontruct new header
                wa_out.BitsPerSample = wa_IN_Start.BitsPerSample;
                wa_out.Channels = wa_IN_Start.Channels;
                wa_out.SampleRate = wa_IN_Start.SampleRate;
                wa_out.WaveHeaderOUT(@strOutFile);

                //Write data - modified code
                byte[] arrfileStart = GetWAVEData(strStartFile);
                byte[] arrfileEnd = GetWAVEData(strEndFile);
                int intLngthofStart = arrfileStart.Length;
                Array.Resize(ref arrfileStart, (arrfileStart.Length + arrfileEnd.Length));
                Array.Copy(arrfileEnd, 0, arrfileStart, intLngthofStart, arrfileEnd.Length);
                WriteWAVEData(strOutFile, ref arrfileStart);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// In stereo wave format, samples are stored in 2's complement. For Mono, it's necessary to 
        /// convert those samples to their equivalent signed value. This method is used 
        /// by other public methods to equilibrate wave formats of different files.
        /// </summary>
        /// <param name="bytArr">Sample data in array</param>
        /// <param name="intPos">Array offset</param>
        /// <returns>Mono value as signed short</returns>
        private short ComplementToSigned(ref byte[] bytArr, int intPos) // 2's complement to normal signed value
        {
            short snd = BitConverter.ToInt16(bytArr, intPos);
            if (snd != 0)
                snd = Convert.ToInt16((~snd | 1));
            return snd;
        }
        /// <summary>
        /// Convert signed sample value back to 2's complement value equivalent to Stereo. This method is used 
        /// by other public methods to equilibrate wave formats of different files.
        /// </summary>
        /// <param name="shtVal">The mono signed value as short</param>
        /// <returns>Stereo 2's complement value as byte array</returns>
        private byte[] SignedToComplement(short shtVal) //Convert to 2's complement and return as byte array of 2 bytes
        {
            byte[] bt = new byte[2];
            shtVal = Convert.ToInt16((~shtVal | 1));
            bt = BitConverter.GetBytes(shtVal);
            return bt;
        }

        /// <summary>
        /// Read the WAVE file then position to DADA segment and return the chunk as byte array 
        /// </summary>
        /// <param name="strWAVEPath">Path of WAVE file</param>
        /// <returns>byte array</returns>
        private byte[] GetWAVEData(string strWAVEPath)
        {
            try
            {
                FileStream fs = new FileStream(@strWAVEPath, FileMode.Open, FileAccess.Read);
                byte[] arrfile = new byte[fs.Length - HeaderLength];
                fs.Position = HeaderLength;
                fs.Read(arrfile, 0, arrfile.Length);
                fs.Close();
                return arrfile;
            }
            catch (IOException ioex)
            {
                throw ioex;
            }
        }

        /// <summary>
        /// Write data chunk to the file. The header must be written before hand.
        /// </summary>
        /// <param name="strWAVEPath">Path of WAVE file</param>
        /// <param name="arrData">data in array</param>
        /// <returns>True</returns>
        private bool WriteWAVEData(string strWAVEPath, ref byte[] arrData)
        {
            try
            {
                FileStream fo = new FileStream(@strWAVEPath, FileMode.Append, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fo);
                bw.Write(arrData);
                bw.Close();
                fo.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    } // End of clsWaveProcessor class
}