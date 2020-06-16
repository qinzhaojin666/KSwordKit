/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourceItem.cs
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
    /// 一项资源的数据结构
    /// </summary>
    [Serializable]
    public class ResourceItem
    {
        /// <summary>
        /// 是否是场景资源
        /// </summary>
        public bool IsScene = false;
        /// <summary>
        /// AssetBundle名称
        /// <para>指示该资源位于哪个资源包内</para>
        /// </summary>
        public string AssetBundleName = null;
        /// <summary>
        /// 资源路径
        /// <para>该资源在项目中的相对路径</para>
        /// </summary>
        public string ResourcePath = null;
        /// <summary>
        /// 资源的文件扩张名
        /// <para>不同类型的资源加载方式不同</para>
        /// </summary>
        public string FileExtensionName = null;
        /// <summary>
        /// 对象名称
        /// <para>用于在AssetBundle中加载资源，在使用诸如 ab.LoadAsset 时当做参数.</para>
        /// <para>当<see cref="IsScene"/> 为true时，改名字为场景名称，可用于方法 <see cref="UnityEngine.SceneManagement.SceneManager.LoadScene(string)"/>中作为参数，加载场景。</para>
        /// </summary>
        public string ObjectName = null;
        /// <summary>
        /// 资源对象
        /// </summary>
        [NonSerialized]
        public UnityEngine.Object ResourceObject = null;

    }
}
