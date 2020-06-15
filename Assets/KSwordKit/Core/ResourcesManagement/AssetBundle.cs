/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourceAssetBundle.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-15
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Core.ResourcesManagement
{
    /// <summary>
    /// 资源包数据结构
    /// </summary>
    public class AssetBundle
    {
        /// <summary>
        /// 资源文件序列化版本号
        /// </summary>
        public string AssetFileHashSerializedVersion = null;
        /// <summary>
        /// 资源文件哈希值
        /// </summary>
        public string AssetFileHash = null;
        /// <summary>
        /// 类型树序列化版本号
        /// </summary>
        public string TypeTreeHashSerializedVersion = null;
        /// <summary>
        /// 类型树哈希值
        /// </summary>
        public string TypeTreeHash = null;
        /// <summary>
        /// 资源包的名字
        /// <para>程序依据该值加载AssetBundle</para>
        /// </summary>
        public string AssetBundleName;
        /// <summary>
        /// 资源包内包含的所有资源项
        /// <para>查看<see cref="ResourceItem"/>了解资源项的数据结构。</para>
        /// </summary>
        public ResourceItem[] ResourceItems = null;
        /// <summary>
        /// 该资源包依赖的其他资源包
        /// </summary>
        public AssetBundle[] Dependencies = null;
        /// <summary>
        /// 资源包对象
        /// </summary>
        public UnityEngine.AssetBundle ABObject = null;
    }
}
