using BF1GamePatch.Helper;

namespace BF1GamePatch;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    private static Mutex AppMainMutex;
    private readonly string AppName = ResourceAssembly.GetName().Name;

    protected override void OnStartup(StartupEventArgs e)
    {
        AppMainMutex = new Mutex(true, AppName, out var createdNew);
        if (!createdNew)
        {
            MsgBoxHelper.Warning($"请不要重复打开，程序已经运行\n如果一直提示，请到\"任务管理器-详细信息（win7为进程）\"里\n强制结束 \"{AppName}.exe\" 程序");
            Current.Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}
