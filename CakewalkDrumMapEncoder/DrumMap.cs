using System.Collections.ObjectModel;
using System.IO;

namespace DrumMapEncoder
{
    class DrumMap
    {
        // ==================================================
        // コンストラクタ
        // ==================================================
        public DrumMap(string targetPath)
        {
            // ----------------------------------------
            // ノート、出力ポートの個数を数える
            // ----------------------------------------
            NoteCount = 0;
            OutputPortCount = 0;
            foreach (string line in File.ReadLines(targetPath))
            {
                if (line[0] == '#') OutputPortCount++;
                else NoteCount++;
            }

            // ----------------------------------------
            // ノート、出力ポートの情報を読み込む
            // ----------------------------------------
            NoteDatas = new NoteData[NoteCount];
            OutputPortDatas = new OutputPortData[OutputPortCount];
            int noteCount = 0;
            int outputPortCount = 0;
            foreach (string line in File.ReadLines(targetPath))
            {
                // 読み込んだ行をタブで分解
                string[] splitedDatas = line.Split('\t');

                // 行頭に # がついている場合、出力ポート情報の読み込み
                if (line[0] == '#')
                {
                    string tmpOutputPortName;
                    uint tmpDefaultFlag;
                    uint tmpDefaultPacking;
                    uint tmpBankNumber;
                    uint tmpPatchNumber;

                    // 出力ポート名がデフォルトの場合
                    if (InputData.Instance().OutputPortNames[InputData.Instance().SelectedOutputPortNameIndex] == "デフォルト")
                    {
                        tmpOutputPortName = null;
                        tmpDefaultFlag = DefaultPortTrue;
                        tmpDefaultPacking = Packing_F;
                    }
                    else
                    {
                        tmpOutputPortName = InputData.Instance().OutputPortNames[InputData.Instance().SelectedOutputPortNameIndex];
                        tmpDefaultFlag = DefaultPortFalse;
                        tmpDefaultPacking = Packing_0;
                    }

                    // バンク番号がデフォルトの場合
                    if (splitedDatas[3] == "-") tmpBankNumber = Packing_F;
                    else tmpBankNumber = uint.Parse(splitedDatas[3]);

                    // パッチ番号がデフォルトの場合
                    if (splitedDatas[4] == "-") tmpPatchNumber = Packing_F;
                    else tmpPatchNumber = uint.Parse(splitedDatas[4]);

                    // 出力ポート情報をまとめる
                    OutputPortDatas[outputPortCount] = new OutputPortData(
                        channelNumber:    uint.Parse(splitedDatas[1]),
                        outputPortNumber: uint.Parse(splitedDatas[2]),
                        outputPortName:   tmpOutputPortName,
                        defaultFlag:      tmpDefaultFlag,
                        defaultPacking:   tmpDefaultPacking,
                        bankNumber:       tmpBankNumber,
                        patchNumber:      tmpPatchNumber
                        );
                    outputPortCount++;
                }

                // 行頭に # がついていない場合、ノート情報の読み込み
                else
                {
                    uint tmpNoteEndFlag;

                    // 最後のノートの場合
                    if (noteCount == NoteCount - 1) tmpNoteEndFlag = NoteEndTrue;
                    else tmpNoteEndFlag = NoteEndFalse;

                    // ノート情報をまとめる
                    NoteDatas[noteCount] = new NoteData(
                        inputNoteNumber: uint.Parse(splitedDatas[0]),
                        outputNoteNumber: uint.Parse(splitedDatas[1]),
                        noteName: splitedDatas[2],
                        channelNumber: uint.Parse(splitedDatas[3]),
                        outputPortNumber: uint.Parse(splitedDatas[4]),
                        velocityOffset: int.Parse(splitedDatas[5]),
                        velocityScale: float.Parse(splitedDatas[6]) / 100,
                        tmpNoteEndFlag
                        );
                    noteCount++;
                }
            }

            //読み込んだ情報からファイルサイズと各セクションのサイズを算出
            NoteDataSize = 16 + 116 * NoteCount;
            OutputPortDataSize = 16 + 20 * OutputPortCount;
            OutputPortDataSize2 = 16 + 24 * OutputPortCount;
            OthrerDataSize = 16 + 40 * NoteCount;
            BankPatchDataSize = 16 + 36 * OutputPortCount;
            FileSize = 16 + NoteDataSize + 174 + OutputPortDataSize + OutputPortDataSize2 + OthrerDataSize;
        }

        // ==================================================
        // 読み取り専用メンバー
        // ==================================================
        public uint Packing_0 { get; } = 0x00000000;
        public uint Packing_F { get; } = 0xFFFFFFFF;
        public uint Constant_1017_0 { get; } = 0x10170000;
        public uint Constant_1017_1 { get; } = 0x10170001;
        public uint Constant_1017_2 { get; } = 0x10170002;
        public uint Constant_1017_3 { get; } = 0x10170003;
        public uint Constant_1017_4 { get; } = 0x10170004;
        public uint Constant_1018_0 { get; } = 0x10180000;
        public uint NoteEndTrue { get; } = 0x000000B2;
        public uint NoteEndFalse { get; } = 0x00000000;
        public uint DefaultPortTrue { get; } = 0x00000000;
        public uint DefaultPortFalse { get; } = 0x141AC902;
        public ReadOnlyCollection<byte> UniqueNoteConstant = new ReadOnlyCollection<byte>(new byte[]
        {
            0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x3A, 0x10, 0x0C, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3A, 0x10,
            0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x3A, 0x10, 0x00, 0x00, 0x00, 0x00
        });

        // ==================================================
        // ファイル・セクションサイズプロパティ
        // ==================================================
        public int FileSize { get; set; }
        public int NoteDataSize { get; set; }
        public int OutputPortDataSize { get; set; }
        public int OutputPortDataSize2 { get; set; }
        public int OthrerDataSize { get; set; }
        public int BankPatchDataSize { get; set; }

        // ==================================================
        // 読み込むプロパティ
        // ==================================================
        public int NoteCount { get; set; }
        public int OutputPortCount { get; set; }
        public string DrumMapName { get; set; } = "DrumMapEncoderTest"; // 現時点ではデフォルト
        public NoteData[] NoteDatas { get; set; }
        public OutputPortData[] OutputPortDatas { get; set; }

        // ==================================================
        // 内部クラス
        // ==================================================
        public class NoteData
        {
            public NoteData(uint inputNoteNumber, uint outputNoteNumber, string noteName, uint channelNumber, uint outputPortNumber, int velocityOffset, float velocityScale, uint noteEndFlag)
            {
                InputNoteNumber = inputNoteNumber;
                OutputNoteNumber = outputNoteNumber;
                NoteName = noteName;
                ChannelNumber = channelNumber;
                OutputPortNumber = outputPortNumber;
                VelocityOffset = velocityOffset;
                VelocityScale = velocityScale;
                NoteEndFlag = noteEndFlag;
            }
            public uint InputNoteNumber { get; }
            public uint OutputNoteNumber { get; }
            public string NoteName { get; }
            public uint ChannelNumber { get; }
            public uint OutputPortNumber { get; }
            public int VelocityOffset { get; }
            public float VelocityScale { get; }
            public uint NoteEndFlag { get; }
        }
        public class OutputPortData
        {
            public OutputPortData(uint channelNumber, uint outputPortNumber, string outputPortName, uint defaultFlag, uint defaultPacking, uint bankNumber, uint patchNumber)
            {
                ChannelNumber = channelNumber;
                OutputPortNumber = outputPortNumber;
                OutputPortName = outputPortName;
                DefaultFlag = defaultFlag;
                DefaultPacking = defaultPacking;
                BankNumber = bankNumber;
                PatchNumber = patchNumber;
            }
            public uint ChannelNumber { get; }
            public uint OutputPortNumber { get; }
            public string OutputPortName { get; }
            public uint DefaultFlag { get; }
            public uint DefaultPacking { get; }
            public uint BankNumber { get; }
            public uint PatchNumber { get; }
        }
    }
}