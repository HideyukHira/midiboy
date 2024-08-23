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
            // comboBoxMidi へ comboBox1 を代入

            InitializeComponent();
            comboBoxMidi = comboBox1;
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

            // MIDIデバイスの選択
  
            
                midiIn = new MidiIn(1);
                midiIn.MessageReceived += MidiIn_MessageReceived;
                midiIn.ErrorReceived += MidiIn_ErrorReceived;
                midiIn.Start();
            
        



        }


        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // MIDIメッセージをTextBoxに表示
            Invoke(new Action(() =>
            {
                // MIDIイベントを適切な形式で表示
                textBox1.AppendText($"MIDI Message Received: {e.MidiEvent}\r\n");
                // MIDIメッセージのデータを コンソールに表示
                Debug.WriteLine($"MIDI Message Received: {e.MidiEvent}\r\n");
            }));
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
