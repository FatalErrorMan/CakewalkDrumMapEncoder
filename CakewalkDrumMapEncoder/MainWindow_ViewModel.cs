using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DrumMapEncoder
{
    class MainWindow_ViewModel : INotifyPropertyChanged
    {
        // ==================================================
        // コンストラクタ
        // ==================================================
        public MainWindow_ViewModel()
        {
            ReadDrumMapDataCommand = new DelegateCommand(CanExecuteReadDrumMapData, ExecuteReadDrumMapData);
            DeleteOutputPortNameCommand = new DelegateCommand(CanExecuteDeleteOutputPortName, ExecuteDeleteOutputPortName);
            InputData.Instance().DrumMapDataPropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(TargetPath):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetPath)));
                        break;
                    case nameof(EncodedFileName):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncodedFileName)));
                        break;
                    case nameof(ErrorLog):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorLog)));
                        break;
                    case nameof(SelectedOutputPortNameIndex):
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOutputPortNameIndex)));
                        break;
                    default:
                        break;
                }
            };
        }

        // ==================================================
        // プロパティ変更通知イベント
        // ==================================================
        public event PropertyChangedEventHandler PropertyChanged;

        // ==================================================
        // V <-> VM <-> M プロパティ
        // ==================================================
        public string TargetPath
        {
            get => InputData.Instance().TargetPath;
            set { if (TargetPath != value) InputData.Instance().TargetPath = value; }
        }
        public string EncodedFileName
        {
            get => InputData.Instance().EncodedFileName;
            set { if (EncodedFileName != value) InputData.Instance().EncodedFileName = value; }
        }
        public string ErrorLog
        {
            get => InputData.Instance().ErrorLog;
            set { if (ErrorLog != value) InputData.Instance().ErrorLog = value; }
        }
        public ObservableCollection<string> OutputPortNames
        {
            get => InputData.Instance().OutputPortNames;
            set { if (OutputPortNames != value) InputData.Instance().OutputPortNames = value; }
        }
        public int SelectedOutputPortNameIndex
        {
            get => InputData.Instance().SelectedOutputPortNameIndex;
            set { if (SelectedOutputPortNameIndex != value) InputData.Instance().SelectedOutputPortNameIndex = value; }
        }

        // ==================================================
        // V <-> VM プロパティ
        // ==================================================
        private bool pEnableLoadOutputPortNameMode;
        public bool EnableLoadOutputPortNameMode
        {
            get => pEnableLoadOutputPortNameMode;
            set { pEnableLoadOutputPortNameMode = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableLoadOutputPortNameMode))); }
        }

        // ==================================================
        // 読み込みコマンド
        // ==================================================
        public DelegateCommand ReadDrumMapDataCommand { get; set; }
        public bool CanExecuteReadDrumMapData(object parameter) => true;
        public void ExecuteReadDrumMapData(object parameter)
        {
            if (!EnableLoadOutputPortNameMode)
            {
                InputData.Instance().ErrorLog = "変換を開始しました。";
                if (File.Exists(TargetPath))
                {
                    bool collectFormat;
                    collectFormat = DrumMapCreater.Instance().CheckFormat();
                    if (collectFormat)
                    {
                        DrumMapCreater.Instance().ReadDrumMapData();
                        DrumMapCreater.Instance().WriteDrumMapData();
                        InputData.Instance().ErrorLog = "変換が終了しました。";
                    }
                }
                else InputData.Instance().ErrorLog = "エラー : 読み込むファイルがありません。";
            }
            else
            {
                InputData.Instance().ErrorLog = "出力ポート名を読み込みます。";
                if (File.Exists(TargetPath))
                {
                    DrumMapCreater.Instance().ReadOutputPortName();
                    InputData.Instance().ErrorLog = "出力ポート名をリストに追加しました。";
                }
                else InputData.Instance().ErrorLog = "エラー : 読み込むファイルがありません。";
            }
        }
        
        // ==================================================
        // 出力ポート名削除コマンド
        // ==================================================
        public DelegateCommand DeleteOutputPortNameCommand { get; set; }
        public bool CanExecuteDeleteOutputPortName(object parameter) => true;
        public void ExecuteDeleteOutputPortName(object parameter)
        {
            int tmpIndex = InputData.Instance().SelectedOutputPortNameIndex;
            if (tmpIndex != 0)
            {
                InputData.Instance().OutputPortNames.Remove(InputData.Instance().OutputPortNames[InputData.Instance().SelectedOutputPortNameIndex]);
                InputData.Instance().SelectedOutputPortNameIndex = tmpIndex - 1;
            }
        }

        // ==================================================
        // 出力ポート名の保存・読み込みメソッド
        // ==================================================
        public void SaveOutputPortNames() => InputData.Instance().SaveOutputPortNames();
        public void LoadOutputPortNames() => InputData.Instance().LoadOutputPortNames();
    }

    // ==================================================
    // リストのドラッグ＆ドロップ操作用ビヘイビア
    // ==================================================
    class ListViewDragDropBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewDragOver += TextBox_PreviewDragOver;
            AssociatedObject.Drop += TextBox_Drop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewDragOver -= TextBox_PreviewDragOver;
            AssociatedObject.Drop -= TextBox_Drop;
        }

        // ----------------------------------------
        // 各種ハンドラに登録するメソッド
        // ----------------------------------------
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = false;
            }
        }
        private void TextBox_Drop(object sender, DragEventArgs e)
        {
            InputData.Instance().TargetPath = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
        }
    }
}
