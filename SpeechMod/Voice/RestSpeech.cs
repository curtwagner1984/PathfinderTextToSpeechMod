using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpeechMod.Voice;

/// <summary>
/// ISpeech implementation that forwards prepared text to a REST endpoint.
/// The endpoint is expected to handle audio playback on its own.
/// </summary>
public class RestSpeech : ISpeech
{
    private static readonly HttpClient s_Client = new HttpClient();

    private static async void SendAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(Main.Settings.RestApiUrl))
        {
            Main.Logger?.Warning("REST API URL is not configured.");
            return;
        }

        try
        {
            var json = JsonConvert.SerializeObject(new { text });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            await s_Client.PostAsync(Main.Settings.RestApiUrl, content);
        }
        catch (Exception ex)
        {
            Main.Logger?.Error($"REST request failed: {ex.Message}");
        }
    }

    private static string PrepareDialog(string text)
    {
        text = text.PrepareText();
        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");
        return text;
    }

    public bool IsSpeaking() => false;

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = text.PrepareText();
        SendAsync(text);
    }

    public void SpeakDialog(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = PrepareDialog(text);
        SendAsync(text);
    }

    public void SpeakAs(string text, VoiceType voiceType, float delay = 0f)
    {
        Speak(text, delay);
    }

    public void Speak(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = text.PrepareText();
        SendAsync(text);
    }

    public void Stop()
    {
        // Endpoint handles playback; no local audio to stop.
    }

    public string[] GetAvailableVoices() => ["REST#Default"];

    public string GetStatusMessage() => "REST speech ready!";
}
