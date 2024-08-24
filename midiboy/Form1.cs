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
            // comboBoxMidi �� comboBox1 ����
            comboBoxMidi = comboBox1;
            comboBoxMidi.SelectedIndexChanged += ComboBoxMidi_SelectedIndexChanged;
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
        }

        private void ComboBoxMidi_SelectedIndexChanged(object sender, EventArgs e)
        {
            // �I�����ꂽMIDI�f�o�C�X�̃C���f�b�N�X���擾
            int selectedIndex = comboBoxMidi.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < MidiIn.NumberOfDevices)
            {
                // ������MIDI�f�o�C�X���~���Ĕj��
                if (midiIn != null)
                {
                    midiIn.Stop();
                    midiIn.Dispose();
                }

                // �V����MIDI�f�o�C�X��I��
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
                // Note On �C�x���g������
                HandleNoteOn(noteOnEvent);
            }
            else if (e.MidiEvent is NoteEvent noteEvent && noteEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                // Note Off �C�x���g������
                HandleNoteOff(noteEvent);
            }
        }

        private void HandleNoteOn(NoteOnEvent noteOnEvent)
        {
            // �m�[�g�ԍ����L�[�R�[�h�ɕϊ�
            Keys key = ConvertNoteToKey(noteOnEvent.NoteNumber);
            // �L�[�̉������V�~�����[�g
            SendKeys.SendWait(key.ToString());
        }

        private void HandleNoteOff(NoteEvent noteEvent)
        {
            // �m�[�g�ԍ����L�[�R�[�h�ɕϊ�
            Keys key = ConvertNoteToKey(noteEvent.NoteNumber);
            // �L�[�̃����[�X���V�~�����[�g�i�K�v�ɉ����Ď����j
            // SendKeys.SendWait("{UP " + key.ToString() + "}");
        }

        private Keys ConvertNoteToKey(int noteNumber)
        {
            // �m�[�g�ԍ����L�[�R�[�h�ɕϊ����郍�W�b�N������
            // ��: C4 (�m�[�g�ԍ�60) �� A �L�[�Ƀ}�b�s���O
            switch (noteNumber)
            {
                case 60: return Keys.A;
                case 61: return Keys.W;
                case 62: return Keys.S;
                // ���̃m�[�g�ԍ������l�Ƀ}�b�s���O
                default: return Keys.None;
            }
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