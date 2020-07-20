/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ImportConfig.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-9
 *  File Description: Ignore.
 *************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Contents.Editor
{
#if UNITY_EDITOR
    [Serializable]
    public class ImportFileSetting
    {
        /// <summary>
        /// 用于指示 Path 值是文件还是文件目录
        /// </summary>
        public bool IsDir;
        /// <summary>
        /// 该文件在部件内的相对路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 该文件希望导入的目标位置
        /// </summary>
        public string ImportPath;
    }
    [Serializable]
    public class ImportConfig
    {
        /// <summary>
        /// 部件名称
        /// <para>名称在整个框架内有唯一性，否在不能导入。</para>
        /// </summary>
        public string Name;
        /// <summary>
        /// 该部件所依赖的框架内其他部件名称列表
        /// <para>根据 `独立无依赖原则`，请尽量保持该项为空。</para>
        /// </summary>
        public List<string> Dependencies;
        /// <summary>
        /// 该部件的示例代码路径列表：一个路径代表一个目录
        /// <para>默认示例代码是 `Example` 文件夹，也可以有多个，但最终他们都会被整合到 `Example` 里面。 </para>
        /// </summary>
        public List<string> ExampleFolderPaths;
        /// <summary>
        /// 该部件内特殊文件设置
        /// <para>默认情况下，部件导入项目中时，所有文件会导入到 `Assets/KSwordKit/Contents/List/{部件名称}` 文件夹内。</para>
        /// <para>如果有些特殊文件需要在其他路径下才能正常工作，可以使用该项单独设置。</para>
        /// </summary>
        public List<ImportFileSetting> FileSettings;
    }
#endif
}
