using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DrumMapEncoder
{
    class DrumMapCreater
    {
        // ==================================================
        // シングルトンコンストラクタ
        // ==================================================
        private DrumMapCreater() { }
        private static readonly DrumMapCreater instance = new DrumMapCreater();
        public static DrumMapCreater Instance() => instance;

        // ==================================================
        // M プロパティ
        // ==================================================
        public DrumMap LoadedDrumMapData { get; set; }

        // ==================================================
        // フォーマット検証コマンド
        // ==================================================
        public bool CheckFormat()
        {
            // ----------------------------------------
            // エラーログをリセット
            // ----------------------------------------
            InputData.Instance().ErrorLog = "";

            // ----------------------------------------
            // 読み込み前にフォーマットを検証する
            // ----------------------------------------
            Regex noteRegex = new Regex(@"^\d+\t\d+\t.+?\t\d+\t\d+\t\d+\t\d+$");
            Regex portRegex = new Regex(@"^#\t\d+\t\d+\t.+?\t.+?$");
            bool collectFormat = true;
            bool portDataExist = false;
            int count = 1;
            foreach (string line in File.ReadLines(InputData.Instance().TargetPath))
            {
                // 出力ポートの情報が存在するかの判定
                if (line[0] == '#') portDataExist = true;

                // 行のフォーマットがノート情報または出力ポート情報に一致するかの判定
                if (!noteRegex.IsMatch(line)&&!portRegex.IsMatch(line))
                {
                    collectFormat = false;
                    InputData.Instance().ErrorLog += $"{count}行目のフォーマットが間違っています。\n";
                }
                count++;
            }
            if (!portDataExist) InputData.Instance().ErrorLog += "ポート情報がありません。\n";
            return collectFormat && portDataExist;
        }

        // ==================================================
        // 読み込みメソッド
        // ==================================================
        public void ReadDrumMapData() { LoadedDrumMapData = new DrumMap(InputData.Instance().TargetPath); }

        // ==================================================
        // 書き込みメソッド
        // ==================================================
        public void WriteDrumMapData()
        {
            // ----------------------------------------
            // データを書き込むドラムマップファイルを生成
            // ----------------------------------------
            string encodedFilePath = $@"{Path.GetDirectoryName(InputData.Instance().TargetPath)}\{InputData.Instance().EncodedFileName}";
            if (File.Exists(encodedFilePath)) File.Delete(encodedFilePath);

            // ----------------------------------------
            // データをドラムマップファイルに書き込み
            // ----------------------------------------
            using (FileStream fileStream = new FileStream(encodedFilePath, FileMode.Create))
            {
                BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                binaryWriter.Write(LoadedDrumMapData.FileSize);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_4);
                binaryWriter.Write(LoadedDrumMapData.NoteDataSize);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_0);
                binaryWriter.Write(LoadedDrumMapData.NoteCount);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                foreach(DrumMap.NoteData noteData in LoadedDrumMapData.NoteDatas)
                {
                    binaryWriter.Write(noteData.InputNoteNumber);
                    for (int i = 0; i < 6; i++) binaryWriter.Write(LoadedDrumMapData.Packing_0);
                    binaryWriter.Write(noteData.VelocityScale);
                    binaryWriter.Write(noteData.ChannelNumber);
                    binaryWriter.Write(noteData.OutputNoteNumber);
                    binaryWriter.Write(noteData.OutputPortNumber);
                    binaryWriter.Write(noteData.VelocityOffset);
                    binaryWriter.Write(Encoding.GetEncoding("utf-16").GetBytes(noteData.NoteName));
                    for (int i = 0; i < 64 - Encoding.GetEncoding("utf-16").GetBytes(noteData.NoteName).Length; i++) binaryWriter.Write((byte)0x00);
                    binaryWriter.Write(noteData.NoteEndFlag);
                }
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_1);
                binaryWriter.Write(Encoding.GetEncoding("utf-16").GetBytes(LoadedDrumMapData.DrumMapName));
                for (int i = 0; i < 166 - Encoding.GetEncoding("utf-16").GetBytes(LoadedDrumMapData.DrumMapName).Length; i++) binaryWriter.Write((byte)0x00);
                binaryWriter.Write(LoadedDrumMapData.OutputPortDataSize);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_2);
                binaryWriter.Write(LoadedDrumMapData.OutputPortCount);
                foreach(DrumMap.OutputPortData outputPortData in LoadedDrumMapData.OutputPortDatas)
                {
                    binaryWriter.Write(outputPortData.OutputPortNumber);
                    binaryWriter.Write(outputPortData.DefaultFlag);
                    if (outputPortData.OutputPortName == null) for (int i = 0; i < 12; i++) binaryWriter.Write((byte)0x00);
                    else binaryWriter.Write(Encoding.ASCII.GetBytes(outputPortData.OutputPortName));
                }
                binaryWriter.Write(LoadedDrumMapData.OutputPortDataSize2);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_3);
                binaryWriter.Write(LoadedDrumMapData.OutputPortCount);
                foreach (DrumMap.OutputPortData outputPortData in LoadedDrumMapData.OutputPortDatas)
                {
                    binaryWriter.Write(outputPortData.OutputPortNumber);
                    binaryWriter.Write(outputPortData.DefaultFlag);
                    if (outputPortData.OutputPortName == null) for (int i = 0; i < 12; i++) binaryWriter.Write((byte)0x00);
                    else binaryWriter.Write(Encoding.ASCII.GetBytes(outputPortData.OutputPortName));
                    binaryWriter.Write(outputPortData.DefaultPacking);
                }
                binaryWriter.Write(LoadedDrumMapData.OthrerDataSize);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1017_4);
                binaryWriter.Write(LoadedDrumMapData.NoteCount);
                for (int i = 0; i < LoadedDrumMapData.NoteCount; i++)
                {
                    foreach(byte b in LoadedDrumMapData.UniqueNoteConstant)
                    {
                        binaryWriter.Write(b);
                    }
                }
                binaryWriter.Write(LoadedDrumMapData.BankPatchDataSize);
                binaryWriter.Write(LoadedDrumMapData.Packing_0);
                binaryWriter.Write(LoadedDrumMapData.Constant_1018_0);
                binaryWriter.Write(LoadedDrumMapData.OutputPortCount);
                foreach(DrumMap.OutputPortData outputPortData in LoadedDrumMapData.OutputPortDatas)
                {
                    binaryWriter.Write(outputPortData.ChannelNumber);
                    binaryWriter.Write(outputPortData.OutputPortNumber);
                    binaryWriter.Write(outputPortData.BankNumber);
                    binaryWriter.Write(LoadedDrumMapData.Packing_0);
                    binaryWriter.Write(outputPortData.PatchNumber);
                    for (int i = 0; i < 4; i++) binaryWriter.Write(LoadedDrumMapData.Packing_0);
                }
            }
        }

        // ==================================================
        // 出力ポート名読み込みメソッド
        // ==================================================
        public void ReadOutputPortName() 
        {
            using (FileStream fileStream = new FileStream(InputData.Instance().TargetPath, FileMode.Open))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                binaryReader.BaseStream.Seek(12, SeekOrigin.Begin);
                int tmpNoteDataSize = binaryReader.ReadInt32();
                binaryReader.BaseStream.Seek(tmpNoteDataSize + 198, SeekOrigin.Current);
                byte[] tmpOutputPortName = new byte[12]; // 出力ポート名はASCII12文字
                binaryReader.Read(tmpOutputPortName, 0, 12);
                InputData.Instance().OutputPortNames.Add(Encoding.ASCII.GetString(tmpOutputPortName));
            }
        }
    }
}
