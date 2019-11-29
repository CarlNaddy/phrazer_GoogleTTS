using System;
using System.IO;
using System.Collections.Generic;
using Google.Cloud.TextToSpeech.V1;

using NAudio;
using NAudio.Wave;


namespace Phrazer
{

    class GTTSGenerator
    {
        public List<Google.Protobuf.ByteString> AudioContents = new List<Google.Protobuf.ByteString>();

        public Google.Protobuf.ByteString SynthesizeSSML(string voice, string ssml)
        {
            var client = TextToSpeechClient.Create();
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = new SynthesisInput
                {
                    Ssml = ssml
                },
                // Note: voices can also be specified by name
                Voice = new VoiceSelectionParams
                {
                    LanguageCode = voice.Substring(0, 5),
                    Name = voice
                    //SsmlGender = SsmlVoiceGender.Female
                },
                AudioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Linear16
                }
            });

            AudioContents.Add(response.AudioContent);

            return response.AudioContent;
        }

        public void SaveToFile(string filename, Google.Protobuf.ByteString audioContent)
        {
            // Write Audio to File
            using (Stream output = File.Create(filename))
            {
                audioContent.WriteTo(output);
            }
        }

        public static void ListAllVoices()
        {
            // string value = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            // Console.WriteLine("GOOGLE_APPLICATION_CREDENTIALS");
            // Console.WriteLine(value); 
            // return;

            // var client = TextToSpeechClient.Create();
            // var response = client.ListVoices("de");
            // foreach (var voice in response.Voices)
            // {
            //     Console.WriteLine($"{voice.Name} ({voice.SsmlGender}); Language codes: {string.Join(", ", voice.LanguageCodes)}");
            // }

        }

        public void ConcatenateAndSaveWavContents(string outputFile)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (Google.Protobuf.ByteString audioContent in AudioContents.ToArray())
                {
                    using (var audioStream = new MemoryStream(audioContent.ToByteArray()))
                    {
                        using (WaveFileReader reader = new WaveFileReader(audioStream))
                        {
                            if (waveFileWriter == null)
                            {
                                // first time in create new Writer
                                waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                            }
                            else
                            {
                                if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                                {
                                    throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                                }
                            }

                            int read;
                            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                waveFileWriter.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                {
                    waveFileWriter.Dispose();
                }
            }
        }


    }
}
