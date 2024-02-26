using System;
using System.IO;
using System.Collections.Generic;
using Google.Cloud.TextToSpeech.V1;
using Google.Apis.Auth.OAuth2;

using NAudio;
using NAudio.Wave;


namespace Phrazer
{

    class GTTSGenerator
    {
        public GTTSGenerator() {
            // Set and Check Google TTS API Key
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Appdata.GetAppdataPath() + "fair-portal-143718-41e0b8fef741.json");
            string value = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            Console.WriteLine("GOOGLE_APPLICATION_CREDENTIALS: " + value);
            if(!File.Exists(value)) {
                Console.WriteLine("ERROR: GOOGLE_APPLICATION_CREDENTIALS ERROR is not set!");
                Console.WriteLine(value + " NOT EXISTS!");
                Console.WriteLine("*******************************************************");
            }
        }
        
        
        public List<byte[]> AudioContents = new List<byte[]>();

        public void SynthesizeSpeechAndAddToBuffer(string voice, string ssml)
        {
            //var sac = ServiceAccountCredential.FromServiceAccountData(new FileStream("_appdata\\fair-portal-143718-41e0b8fef741.json", FileMode.Open));

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

            AudioContents.Add(response.AudioContent.ToByteArray());
        }

        /* Legacy: for putting sound directly into track (we don't use it anymore) */
        public void JustAddWavSound(string sound)
        {
            string fileName = Appdata.GetSoundPath(sound, "", "wav");
            if(File.Exists(fileName)) AudioContents.Add(File.ReadAllBytes(fileName));
        }

        public void ConcatAndSaveWavContents(string outputFile)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (byte[] audioContentByteArray in AudioContents.ToArray())
                {
                    using (var audioStream = new MemoryStream(audioContentByteArray))
                    {
                        using (WaveFileReader reader = new WaveFileReader(audioStream))
                        {
                            //Console.WriteLine(reader.WaveFormat);
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


    }
}
