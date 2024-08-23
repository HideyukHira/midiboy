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
            // comboBoxMidi �� comboBox1 ����

            InitializeComponent();
            comboBoxMidi = comboBox1;
            LoadMidiDevices();



            // �f�o�b�O�o�͂�MIDI�f�o�C�X�̃��X�g���o��
            Debug.WriteLine("Available MIDI Input Devices:");
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                Debug.WriteLine($"{device}: {MidiIn.DeviceInfo(device).ProductName}");
            }
   

  

        }

 
        private void LoadMidiDevices()
        {
            // MIDI�f�o�C�X��ǂݍ���ŃR���{�{�b�N�X�ɒǉ�
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

            // MIDI�f�o�C�X�̑I��
  
            
                midiIn = new MidiIn(1);
                midiIn.MessageReceived += MidiIn_MessageReceived;
                midiIn.ErrorReceived += MidiIn_ErrorReceived;
                midiIn.Start();
            
        



        }


        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // MIDI���b�Z�[�W��TextBox�ɕ\��
            Invoke(new Action(() =>
            {
                // MIDI�C�x���g��K�؂Ȍ`���ŕ\��
                textBox1.AppendText($"MIDI Message Received: {e.MidiEvent}\r\n");
                // MIDI���b�Z�[�W�̃f�[�^�� �R���\�[���ɕ\��
                Debug.WriteLine($"MIDI Message Received: {e.MidiEvent}\r\n");
            }));
        }

        private void MidiIn_ErrorReceived(object sender, MidiInMessageEventArgs e)
        {
            // �G���[���b�Z�[�W��TextBox�ɕ\��
            Invoke(new Action(() =>
            {
                textBox1.AppendText($"MIDI Error Received: {e.MidiEvent}\r\n");
            }));
        }
    }
}
