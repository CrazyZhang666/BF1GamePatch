using BF1GamePatch.Helper;

namespace BF1GamePatch;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private const string BF1MD5 = "254FEC4761D42EA95C6D5381F9C93518";
    private readonly string AppBaseDir = Environment.CurrentDirectory;

    private List<string> GameReplaceFiles;
    private List<string> GameDeleteFiles;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Main_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            try
            {
                // 读取嵌入文件
                GameReplaceFiles = FileHelper.GetEmbeddedResourceTextAllLine("Replace.txt");
                GameDeleteFiles = FileHelper.GetEmbeddedResourceTextAllLine("Delete.txt");

                foreach (var fileName in GameReplaceFiles)
                {
                    var tempPath = Path.Combine(AppBaseDir, "AppData", fileName);
                    if (!File.Exists(tempPath))
                    {
                        AppendLogger($"程序核心文件丢失 {tempPath}");
                        AppendLogger("程序初始化终止，发生严重错误！");
                        return;
                    }
                }

                AppendLogger("程序初始化完成");
                SetButtonEnable(true);
            }
            catch (Exception ex)
            {
                AppendLogger($"程序初始化发生异常 {ex.Message}");
                SetButtonEnable(false);
            }
        });
    }

    private void Window_Main_Closing(object sender, CancelEventArgs e)
    {

    }

    private void AppendLogger(string log)
    {
        this.Dispatcher.Invoke(() =>
        {
            TextBox_Logger.AppendText($"[{DateTime.Now:T}]  {log}");
            TextBox_Logger.AppendText(Environment.NewLine);
            TextBox_Logger.ScrollToEnd();
        });
    }

    private void SetButtonEnable(bool enable)
    {
        this.Dispatcher.Invoke(() =>
        {
            Button_RunGamePatch.IsEnabled = enable;
        });
    }

    private async void Button_RunGamePatch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetButtonEnable(false);

            var dialog = new OpenFileDialog
            {
                Title = "请选择战地1游戏主程序 bf1.exe 文件路径",
                FileName = "bf1.exe",
                DefaultExt = ".exe",
                Filter = "可执行文件 (.exe)|*.exe",
                Multiselect = false,
                RestoreDirectory = true,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == false)
            {
                AppendLogger("玩家选择文件操作取消");
                return;
            }

            AppendLogger($"当前选中的文件为 {dialog.FileName}");
            AppendLogger("正在校验 bf1.exe 文件MD5值中...");

            // 校验文件MD5
            var md5 = await FileHelper.GetFileMD5(dialog.FileName);
            if (BF1MD5 != md5)
            {
                AppendLogger("校验文件MD5失败，请选择正确的 bf1.exe 文件");
                return;
            }

            AppendLogger("校验 bf1.exe 文件MD5值成功");
            AppendLogger("开始执行战地1游戏文件降级操作...");

            var gameDir = Path.GetDirectoryName(dialog.FileName);

            // 第一步 删除多余文件夹
            await FileHelper.DeleteDirectoryAsync(Path.Combine(gameDir, "__Installer"));
            AppendLogger("删除 __Installer 文件夹成功");
            await FileHelper.DeleteDirectoryAsync(Path.Combine(gameDir, "__overlay"));
            AppendLogger("删除 __overlay 文件夹成功");
            await FileHelper.DeleteDirectoryAsync(Path.Combine(gameDir, "EAAntiCheat"));
            AppendLogger("删除 EAAntiCheat 文件夹成功");

            // 第二步 删除多余文件
            foreach (var fileName in GameDeleteFiles)
            {
                var fullPath = Path.Combine(gameDir, fileName);
                if (File.Exists(fullPath))
                {
                    await FileHelper.DeleteFileAsync(fullPath);
                    AppendLogger($"删除文件成功 {fullPath}");
                }
            }

            // 第三步 替换旧版游戏文件
            foreach (var fileName in GameReplaceFiles)
            {
                var tempPath = Path.Combine(AppBaseDir, "AppData", fileName);
                var newPath = Path.Combine(gameDir, fileName);

                await FileHelper.CopyFileAsync(tempPath, newPath);
                AppendLogger($"替换文件成功 {fileName}");
            }

            AppendLogger("恭喜，战地1游戏文件降级操作执行完毕");
        }
        catch (Exception ex)
        {
            AppendLogger($"执行降级操作发生异常 {ex.Message}");
        }
        finally
        {
            SetButtonEnable(true);
        }
    }
}