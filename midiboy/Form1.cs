using System;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio.Midi;

namespace midiboy
{
    public partial class Form1 : Form
    {
        private MidiIn midiIn;
        private ComboBox comboBoxMidi;

        public Form1()
        {
            InitializeComponent();
            // comboBoxMidi へ comboBox1 を代入
            comboBoxMidi = comboBox1;
            comboBoxMidi.SelectedIndexChanged += ComboBoxMidi_SelectedIndexChanged;
            LoadMidiDevices();

            // デバッグ出力にMIDIデバイスのリストを出力
            Debug.WriteLine("Available MIDI Input Devices:");
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                Debug.WriteLine($"{device}: {MidiIn.DeviceInfo(device).ProductName}");
            }
        }

        private void LoadMidiDevices()
        {
            // MIDIデバイスを読み込んでコンボボックスに追加
            int deviceCount = MidiIn.NumberOfDevices;
            comboBoxMidi.Items.Clear();

            for (int i = 0; i < deviceCount; i++)
            {
                string deviceName = MidiIn.DeviceInfo(i).ProductName;
                comboBoxMidi.Items.Add(deviceName);
            }

            if (deviceCount == 0)
            {
                comboBoxMidi.Items.Add("No MIDI devices found");
            }
        }

        private void ComboBoxMidi_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択されたMIDIデバイスのインデックスを取得
            int selectedIndex = comboBoxMidi.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < MidiIn.NumberOfDevices)
            {
                // 既存のMIDIデバイスを停止して破棄
                if (midiIn != null)
                {
                    midiIn.Stop();
                    midiIn.Dispose();
                }

                // 新しいMIDIデバイスを選択
                midiIn = new MidiIn(selectedIndex);
                midiIn.MessageReceived += MidiIn_MessageReceived;
                midiIn.ErrorReceived += MidiIn_ErrorReceived;
                midiIn.Start();
            }
        }

        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent is NoteOnEvent noteOnEvent)
            {
                // Note On イベントを処理
                HandleNoteOn(noteOnEvent);
            }
            else if (e.MidiEvent is NoteEvent noteEvent && noteEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                // Note Off イベントを処理
                HandleNoteOff(noteEvent);
            }
        }

        private void HandleNoteOn(NoteOnEvent noteOnEvent)
        {
            // ノート番号をキーコードに変換
            Keys key = ConvertNoteToKey(noteOnEvent.NoteNumber);
            // キーの押下をシミュレート
            SendKeys.SendWait(key.ToString());
        }

        private void HandleNoteOff(NoteEvent noteEvent)
        {
            // ノート番号をキーコードに変換
            Keys key = ConvertNoteToKey(noteEvent.NoteNumber);
            // キーのリリースをシミュレート（必要に応じて実装）
            // SendKeys.SendWait("{UP " + key.ToString() + "}");
        }

        private Keys ConvertNoteToKey(int noteNumber)
        {
            // ノート番号をキーコードに変換するロジックを実装
            // 例: C4 (ノート番号60) を A キーにマッピング
            switch (noteNumber)
            {
                case 60: return Keys.A;
                case 61: return Keys.W;
                case 62: return Keys.S;
                // 他のノート番号も同様にマッピング
                default: return Keys.None;
            }
        }


        private void MidiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            // エラーメッセージをTextBoxに表示
            Invoke(new Action(() =>
            {
                textBox1.AppendText($"MIDI Error Received: {e.MidiEvent}\r\n");
            }));
        }
    }
}