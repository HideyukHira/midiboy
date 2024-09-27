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
            dataGridView = dataGridView1; // ここで初期化
            InitializeCustomComponents();
            // comboBoxMidi へ comboBox1 を代入
            comboBoxMidi = comboBox1;
            comboBoxMidi.SelectedIndexChanged += ComboBoxMidi_SelectedIndexChanged;
            LoadMidiDevices();

            // 確認用のデバッグ出力
            Debug.WriteLine("Form1 constructor executed.");
            // デバッグ出力にMIDIデバイスのリストを出力
            Debug.WriteLine("Available MIDI Input Devices:");
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                Debug.WriteLine($"{device}: {MidiIn.DeviceInfo(device).ProductName}");
            }

            // フィールドの初期化
            noteToKeyMap = new Dictionary<int, Keys>();
            saveButton = new Button();
            loadButton = new Button();

            // 初期化時にマッピングをロード
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

        private void ComboBoxMidi_SelectedIndexChanged(object? sender, EventArgs e)
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

        private void MidiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent is NoteOnEvent noteOnEvent)
            {
                // Note On イベントを処理
                HandleNoteOn(noteOnEvent);
                // ノート番号をtextBox1に表示
                Invoke(new Action(() =>
                {
                    textBox1.Text = $"Note On: {noteOnEvent.NoteNumber}\r\n";
                }));
            }
            else if (e.MidiEvent is NoteEvent noteEvent && noteEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                // Note Off イベントを処理
                HandleNoteOff(noteEvent);
                // ノート番号をtextBox1に表示
                Invoke(new Action(() =>
                {
                    textBox1.Text = $"Note Off: {noteEvent.NoteNumber}\r\n";
                }));
            }
        }

        private void HandleNoteOn(NoteOnEvent noteOnEvent)
        {
            // ノート番号をキーコードに変換
            Keys key = ConvertNoteToKey(noteOnEvent.NoteNumber);
            // キーの押下をシミュレート
            if (key != Keys.None)
            {
                SendKeys.SendWait(key.ToString());
            }
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
            if (noteToKeyMap.TryGetValue(noteNumber, out Keys key))
            {
                return key;
            }
            return Keys.None; // 対応するキーが見つからない場合
        }

        private void MidiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
            // エラーメッセージをTextBoxに表示
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
                Name = "MidiNote" // ここで列名を設定
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Key",
                DataPropertyName = "Key",
                Name = "Key" // ここで列名を設定
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
                        // デバッグ出力で行の情報を確認
                        Debug.WriteLine($"Row Index: {row.Index}");

                        if (row.Cells["MidiNote"].Value != null && row.Cells["Key"].Value != null)
                        {
                            if (int.TryParse(row.Cells["MidiNote"].Value.ToString(), out int noteNumber))
                            {
                                string? keyString = row.Cells["Key"].Value?.ToString();

                                // デバッグ出力でセルの値を確認
                                Debug.WriteLine($"Note Number: {noteNumber}, Key String: {keyString}");

                                // キー文字列を Keys 列挙型に変換（大文字小文字を区別しない）
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
                // 例外の詳細をデバッグ出力
                Debug.WriteLine($"Exception: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // 例外の詳細をメッセージボックスに表示
                MessageBox.Show($"An error occurred while saving mappings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMappings()
        {
            string filePath = "mappings.txt";

            if (!File.Exists(filePath))
            {
                // ファイルが存在しない場合は初期値を書き込む
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // 初期値の例
                    writer.WriteLine("36,E"); // MIDIノート60をキーAにマッピング
                    writer.WriteLine("62,B"); // MIDIノート62をキーBにマッピング
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
