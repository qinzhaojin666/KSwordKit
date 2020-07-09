

namespace KSwordKit
{
    /// <summary>
    /// KSwordKit中的所有常量
    /// <para>但不包括框架部件内的常量，框架各个部件内的常量由各个部件自己定义和使用。</para>
    /// <para>这里的常量请不要修改，以防止程序出错。</para>
    /// </summary>
    public class KSwordKitConst
    {
        /// <summary>
        /// KSwordKit框架名字
        /// </summary>
        public const string KSwordKitName = "KSwordKit";
        /// <summary>
        /// KSwordKit框架安装根目录
        /// <para>此目录指示了框架的工作目录</para>
        /// <para>该目录在项目 `Assets` 中，框架内导入的所有部件也会出现在该目录内，参与项目程序的最终编译。</para>
        /// </summary>
        public const string KSwordKitDirectory = "Assets/KSwordKit";
        /// <summary>
        /// KSwordKit框架导入部件的根目录
        /// <para>此目录指示了 `导入部件` 时，部件的内容安装的根目录。</para>
        /// </summary>
        public const string KSwordKitContentsDirectory = "Assets/KSwordKit/Contents/List";
        /// <summary>
        /// KSwordKit框架的部件安装所在根目录
        /// <para>此目录指示了在用户执行 `导入部件` 或 `导出部件` 时, 将进行存取目录。</para>
        /// <para>该目录在项目根目录下，但不会出现在 `Assets` 目录中。</para>
        /// </summary>
        public const string KSwordKitContentsSourceDiretory = ".KSwordKit";
        /// <summary>
        /// KSwordKit框架的示例代码所在根目录
        /// <para>该目录是框架内所有部件的示例代码存放位置</para>
        /// <para>如果某部件存在示例代码,　在　｀导入部件｀的同时会在此目录下创建一个名为 `Example{部件名称}` 的文件夹，并把部件内的示例代码存放其中。</para>
        /// </summary>
        public const string KSwordKitExamplesDirtory = "Assets/KSwordKit/Examples";
        /// <summary>
        /// KSwordKit框架更新位置
        /// </summary>
        public const string KSwordKitRemotePath = "";
    }
}
