using NAudio.Midi;
using System.Diagnostics;

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
            dataGridView = dataGridView1; // �����ŏ�����
            InitializeCustomComponents();
            // comboBoxMidi �� comboBox1 ����
            comboBoxMidi = comboBox1;
            comboBoxMidi.SelectedIndexChanged += ComboBoxMidi_SelectedIndexChanged;
            LoadMidiDevices();

            // �m�F�p�̃f�o�b�O�o��
            Debug.WriteLine("Form1 constructor executed.");
            // �f�o�b�O�o�͂�MIDI�f�o�C�X�̃��X�g���o��
            Debug.WriteLine("Available MIDI Input Devices:");
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                Debug.WriteLine($"{device}: {MidiIn.DeviceInfo(device).ProductName}");
            }

            // �t�B�[���h�̏�����
            noteToKeyMap = new Dictionary<int, Keys>();
            saveButton = new Button();
            loadButton = new Button();

            // ���������Ƀ}�b�s���O�����[�h
            LoadMappings();
        }

        private void InitializeNoteToKeyMap()
        {
            noteToKeyMap = new Dictionary<int, Keys>();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["MidiNote"].Value != null && row.Cells["Key"].Value != null)
                {
                    if (int.TryParse(row.Cells["MidiNote"].Value.ToString(), out int noteNumber) &&
                        Enum.TryParse(row.Cells["Key"].Value.ToString(), out Keys key))
                    {
                        noteToKeyMap[noteNumber] = key;
                    }
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

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "MIDI Note",
                DataPropertyName = "MidiNote",
                Name = "MidiNote" // �����ŗ񖼂�ݒ�
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Key",
                DataPropertyName = "Key",
                Name = "Key" // �����ŗ񖼂�ݒ�
            });

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
            string filePath = "mappings.txt";

            try
            {
                Debug.WriteLine("Starting SaveMappings method.");

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    Debug.WriteLine("StreamWriter initialized.");

                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        // �f�o�b�O�o�͂ōs�̏����m�F
                        Debug.WriteLine($"Row Index: {row.Index}");

                        if (row.Cells["MidiNote"].Value != null && row.Cells["Key"].Value != null)
                        {
                            if (int.TryParse(row.Cells["MidiNote"].Value.ToString(), out int noteNumber))
                            {
                                string? keyString = row.Cells["Key"].Value?.ToString();

                                // �f�o�b�O�o�͂ŃZ���̒l���m�F
                                Debug.WriteLine($"Note Number: {noteNumber}, Key String: {keyString}");

                                // �L�[������� Keys �񋓌^�ɕϊ��i�啶������������ʂ��Ȃ��j
                                if (keyString != null && Enum.TryParse(keyString, true, out Keys key))
                                {
                                    writer.WriteLine($"{noteNumber},{key}");
                                    Debug.WriteLine($"Written to file: {noteNumber},{key}");
                                }
                                else
                                {
                                    Debug.WriteLine($"Failed to parse key: {keyString}");
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine("Mappings saved successfully.");
                MessageBox.Show("Mappings saved successfully.", "Save Mappings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // ��O�̏ڍׂ��f�o�b�O�o��
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // ��O�̏ڍׂ����b�Z�[�W�{�b�N�X�ɕ\��
                MessageBox.Show($"An error occurred while saving mappings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMappings()
        {
            string filePath = "mappings.txt";

            if (!File.Exists(filePath))
            {
                // �t�@�C�������݂��Ȃ��ꍇ�͏����l����������
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // �����l�̗�
                    writer.WriteLine("36,E"); // MIDI�m�[�g60���L�[A�Ƀ}�b�s���O
                    writer.WriteLine("62,B"); // MIDI�m�[�g62���L�[B�Ƀ}�b�s���O
                }

                MessageBox.Show("Mapping file not found. A new file with default mappings has been created.", "Load Mappings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                dataGridView.Rows.Clear();
                noteToKeyMap.Clear();

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int noteNumber) && Enum.TryParse(parts[1], out Keys key))
                    {
                        dataGridView.Rows.Add(noteNumber, key);
                        noteToKeyMap[noteNumber] = key;
                    }
                }
            }

            MessageBox.Show("Mappings loaded successfully.", "Load Mappings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
