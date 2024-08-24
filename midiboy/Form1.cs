using System;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio.Midi;

namespace midiboy
{
    public partial class Form1 : Form
    {
        private MidiIn? midiIn;
        private ComboBox comboBoxMidi;
        private DataGridView dataGridView;
        private Button saveButton;
        private Button loadButton;
        private Dictionary<int, Keys> noteToKeyMap;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
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

            // �t�B�[���h�̏�����
            noteToKeyMap = new Dictionary<int, Keys>();
            dataGridView = new DataGridView();
            saveButton = new Button();
            loadButton = new Button();
        }

        private void InitializeNoteToKeyMap()
        {
            noteToKeyMap = new Dictionary<int, Keys>();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["MidiNote"].Value != null && row.Cells["Key"].Value != null)
                {
                    int noteNumber = (int)row.Cells["MidiNote"].Value;
                    Keys key = (Keys)row.Cells["Key"].Value;
                    noteToKeyMap[noteNumber] = key;
                }
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

        private void ComboBoxMidi_SelectedIndexChanged(object? sender, EventArgs e)
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

        private void MidiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent is NoteOnEvent noteOnEvent)
            {
                // Note On �C�x���g������
                HandleNoteOn(noteOnEvent);
                // �m�[�g�ԍ���textBox1�ɕ\��
                Invoke(new Action(() =>
                {
                    textBox1.Text = $"Note On: {noteOnEvent.NoteNumber}\r\n";
                }));
            }
            else if (e.MidiEvent is NoteEvent noteEvent && noteEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                // Note Off �C�x���g������
                HandleNoteOff(noteEvent);
                // �m�[�g�ԍ���textBox1�ɕ\��
                Invoke(new Action(() =>
                {
                    textBox1.Text = $"Note Off: {noteEvent.NoteNumber}\r\n";
                }));
            }
        }

        private void HandleNoteOn(NoteOnEvent noteOnEvent)
        {
            // �m�[�g�ԍ����L�[�R�[�h�ɕϊ�
            Keys key = ConvertNoteToKey(noteOnEvent.NoteNumber);
            // �L�[�̉������V�~�����[�g
            if (key != Keys.None)
            {
                SendKeys.SendWait(key.ToString());
            }
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
            if (noteToKeyMap.TryGetValue(noteNumber, out Keys key))
            {
                return key;
            }
            return Keys.None; // �Ή�����L�[��������Ȃ��ꍇ
        }

        private void MidiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
            // �G���[���b�Z�[�W��TextBox�ɕ\��
            Invoke(new Action(() =>
            {
                textBox1.AppendText($"MIDI Error Received: {e.MidiEvent}\r\n");
            }));
        }

        private void InitializeCustomComponents()
        {
            dataGridView = dataGridView1;

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MIDI Note", DataPropertyName = "MidiNote" });
            dataGridView.Columns.Add(new DataGridViewComboBoxColumn { HeaderText = "Key", DataPropertyName = "Key", DataSource = Enum.GetValues(typeof(Keys)) });

            saveButton = new Button { Text = "Save", Dock = DockStyle.Bottom, Height = 60 };
            saveButton.Click += SaveButton_Click;

            loadButton = new Button { Text = "Load", Dock = DockStyle.Bottom, Height = 60 };
            loadButton.Click += LoadButton_Click;

            Controls.Add(dataGridView);
            Controls.Add(saveButton);
            Controls.Add(loadButton);
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            SaveMappings();
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            LoadMappings();
        }

        private void SaveMappings()
        {
            // Save mappings logic here
        }

        private void LoadMappings()
        {
            // Load mappings logic here
        }
    }
}
