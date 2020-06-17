/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourceAssetBundle.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-15
 *  File Description: Ignore.
 *************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Core.ResourcesManagement
{

    /// <summary>
    /// 一个资源包的数据结构
    /// <para>特指打包的具体资源包，通过解析 .manifest 文件得到的数据</para>
    /// </summary>
    [Serializable]
    public class ResourceManifest
    {
        /// <summary>
        /// .manifest 文件版本号
        /// </summary>
        public string ManifestFileVersion = null;
        /// <summary>
        /// CRC校验码
        /// </summary>
        public string CRC = null;
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
        /// 附加哈希值
        /// </summary>
        public string HashAppended = null;
        /// <summary>
        /// 资源包的名字
        /// <para>程序依据该值加载AssetBundle</para>
        /// </summary>
        public string AssetBundleName = null;
        /// <summary>
        /// 资源包的相对路径
        /// </summary>
        public string AssetBundlePath = null;
        /// <summary>
        /// 资源包对象
        /// </summary>
        [NonSerialized]
        public AssetBundle AssetBundle = null;
        /// <summary>
        /// 资源包内包含的所有资源项
        /// <para>查看<see cref="ResourceObject"/>了解资源项的数据结构。</para>
        /// </summary>
        public List<ResourceObject> ResourceObjects = null;
        /// <summary>
        /// 该资源包依赖的其他资源包
        /// <para>链表项表示依赖的包名，可在主包<see cref="AssetBundleManifest"/>中链表<see cref="AssetBundleManifest.AssetBundleInfos"/>中查找。</para>
        /// </summary>
        public List<string> Dependencies = null;
    }
}
