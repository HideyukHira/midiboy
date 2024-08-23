using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NAudio.Midi;

public class MidiToKeyboard
{
    private MidiIn midiIn;
    private Dictionary<int, Keys> midiToKeyMap;
    private string configFilePath = "config.json";

    public MidiToKeyboard(int midiDeviceIndex)
    {
        midiIn = new MidiIn(midiDeviceIndex);
        midiIn.MessageReceived += MidiIn_MessageReceived;
        midiIn.Start();

        midiToKeyMap = new Dictionary<int, Keys>();
        LoadConfig();
    }

    private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
    {
        if (e.MidiEvent is NoteOnEvent noteOnEvent)
        {
            if (midiToKeyMap.TryGetValue(noteOnEvent.NoteNumber, out Keys key))
            {
                SendKeys.SendWait(key.ToString());
            }
        }
    }

    public void AddMapping(int midiNote, Keys key)
    {
        midiToKeyMap[midiNote] = key;
        SaveConfig();
    }

    private void SaveConfig()
    {
        var config = Newtonsoft.Json.JsonConvert.SerializeObject(midiToKeyMap);
        File.WriteAllText(configFilePath, config);
    }

    private void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            var config = File.ReadAllText(configFilePath);
            midiToKeyMap = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, Keys>>(config);
        }
    }

    public static void Main()
    {
        Application.Run(new MidiToKeyboard(0));
    }
}
