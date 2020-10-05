using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DrumMapEncoder
{
    class InputData
    {
        // ==================================================
        // シングルトンコンストラクタ
        // ==================================================
        private InputData() { ErrorLog = "変換後のドラムマップは、変換元のテキストファイルと同じディレクトリに生成されます。"; }
        private static readonly InputData instance = new InputData();
        public static InputData Instance() => instance;

        // ==================================================
        // プロパティ変更通知イベント
        // ==================================================
        public event PropertyChangedEventHandler DrumMapDataPropertyChanged;

        // ==================================================
        // VM <- M プロパティ
        // ==================================================
        private string pTargetPath;
        public string TargetPath
        {
            get => pTargetPath;
            set 
            { 
                pTargetPath = value;
                EncodedFileName = Path.GetFileNameWithoutExtension(value) + ".map";
                DrumMapDataPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetPath)));
            }
        }
        private string pEncodedFileName;
        public string EncodedFileName
        {
            get => pEncodedFileName;
            set { pEncodedFileName = value; DrumMapDataPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncodedFileName))); }
        }
        private string pErrorLog;
        public string ErrorLog
        {
            get => pErrorLog;
            set { pErrorLog = value; DrumMapDataPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorLog))); }
        }
        public ObservableCollection<string> OutputPortNames { get; set; } = new ObservableCollection<string>();
        private int pSelectedOutputPortNameIndex = 0;
        public int SelectedOutputPortNameIndex
        {
            get => pSelectedOutputPortNameIndex;
            set { pSelectedOutputPortNameIndex = value; DrumMapDataPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOutputPortNameIndex))); }
        }

        // ==================================================
        // 出力ポート名の保存・読み込みメソッド
        // ==================================================
        public void SaveOutputPortNames()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
            using (StreamWriter streamWriter = new StreamWriter("drummapencoder.xml", false, Encoding.UTF8))
            {
                serializer.Serialize(streamWriter, OutputPortNames);
            }
        }
        public void LoadOutputPortNames()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
                var xmlSettings = new System.Xml.XmlReaderSettings() { CheckCharacters = false };
                using (var streamReader = new StreamReader("drummapencoder.xml", Encoding.UTF8))
                using (var xmlReader = System.Xml.XmlReader.Create(streamReader, xmlSettings))
                {
                    var tmp = (ObservableCollection<string>)serializer.Deserialize(xmlReader);
                    foreach (var x in tmp) OutputPortNames.Add(x);
                }
                if (OutputPortNames.Count == 0) { throw new Exception(); }
            }
            catch (Exception e)
            {
                ErrorLog += $"\n例外 : {e.Message}\ndrummapencoder.xml が見つからないため、デフォルトデータを読み込みます。";
                OutputPortNames.Add("デフォルト");
            }
        }
    }
}
