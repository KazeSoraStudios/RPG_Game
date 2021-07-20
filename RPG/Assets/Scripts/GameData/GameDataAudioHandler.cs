using System.Collections.Generic;

namespace RPG_GameData
{
    public class GameDataAudioHandler : GameDataHandler
    {
        public static Dictionary<string, AudioData> ProcessAudio(int index, int count, int columnAdvance, string[] data)
        {
            LogManager.LogDebug("Creating GameData Audio.");
            LogManager.LogDebug($"Processing Audio for data {data}");
            var audio = new Dictionary<string, AudioData>();
            for (int i = 0; i < count; i++)
            {
                var audioData = new AudioData
                {
                    Id = data[index++],
                    SoundName = data[index++],
                    FadeDuration = GetFloatFromCell(data[index++]),
                    FadeIn = data[index++].Equals("TRUE"),
                    FadeOut = data[index++].Equals("TRUE"),
                    delay = GetFloatFromCell(data[index++])
                };

                audio.Add(audioData.Id, audioData);
                index += columnAdvance;
            }
            LogManager.LogDebug("Processing Gamedata Audio finished.");
            return audio;
        }
    }
}
