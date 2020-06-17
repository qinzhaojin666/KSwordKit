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
    /// 一组资源包的数据结构
    /// </summary>
    [Serializable]
    public class ResourceBundleManifests
    {
        /// <summary>
        /// 一组资源包
        /// </summary>
        public List<ResourceBundleManifest> Bundles;
    }

    /// <summary>
    /// 一个资源包的数据结构
    /// <para>解析 .manifest 文件得到的数据</para>
    /// </summary>
    [Serializable]
    public class ResourceBundleManifest
    {
        /// <summary>
        /// 是否是主包
        /// <para>该包记录着项目打包的所有AssetBundle</para>
        /// <para>当值为false时，属性<see cref="ResourceObjects"/> 值为 null.</para>
        /// <para>当值为true时, 属性<see cref="AssetFileHashSerializedVersion"/>、<see cref="AssetFileHash"/>、<see cref="TypeTreeHashSerializedVersion"/>、<see cref="TypeTreeHash"/> 的值均为null</para>
        /// </summary>
        public bool IsMain;
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
        /// </summary>
        public List<ResourceBundleManifest> Dependencies = null;
        /// <summary>
        /// 当 <see cref="IsMain"/> 为true时，该字段值有效。
        /// 当 <see cref="IsMain"/> 为false时，该字段值为null。
        /// </summary>
        public List<ResourceBundleManifest> AssetBundleInfos = null;
    }
}
