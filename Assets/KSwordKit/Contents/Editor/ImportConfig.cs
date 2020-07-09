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
        public string Path;
        public string ImportPath;
        public string ExamplePath;
    }
    [Serializable]
    public class ImportConfig
    {
        public string Name;
        public List<ImportFileSetting> FileSettingList;
    }
#endif
}
