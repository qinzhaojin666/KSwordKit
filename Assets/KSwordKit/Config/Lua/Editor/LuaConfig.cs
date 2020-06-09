

using UnityEngine;
using UnityEditor;

namespace KSwordKit.Config
{
    public class LuaConfig
    {
        /// <summary>
        /// xLua的生成代码路径修改
        /// </summary>
        public const string xLuaGenPath = "KSwordKit/Core/Lua/xLua/XLua/Gen/";

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod()
        {
            // 修改 xLua 默认的生成代码的位置
            CSObjectWrapEditor.GeneratorConfig.common_path = System.IO.Path.Combine(Application.dataPath, xLuaGenPath);
        }
    }
}

