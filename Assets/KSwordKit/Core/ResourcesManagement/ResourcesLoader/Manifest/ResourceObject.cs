/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ResourceObject.cs
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
    public class ResourceObject
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
        public UnityEngine.Object Object = null;

        bool asyncLoadAbr_isdone;
        string asyncLoadAbr_error;
        bool asyncloaded;
        event System.Action<bool, float, string, UnityEngine.Object> asyncLoadAbrEvent;
        AssetBundleRequest assetBundleRequest;

        public IEnumerator AsyncLoad(string path, AssetBundle assetBundle, System.Action<bool, float, string, UnityEngine.Object> asyncAction)
        {
            if (asyncloaded)
            {
                if (asyncLoadAbr_isdone)
                {
                    asyncAction(false, 1, null, null);
                    if (!string.IsNullOrEmpty(asyncLoadAbr_error))
                        ResourcesManagement.Instance.NextFrame(() => asyncAction(asyncLoadAbr_isdone, 1, asyncLoadAbr_error, null));
                    else
                        ResourcesManagement.Instance.NextFrame(() => asyncAction(asyncLoadAbr_isdone, 1, null, Object));
                }
                else
                    asyncLoadAbrEvent += asyncAction;
                yield break;
            }
            if (!asyncloaded)
                asyncloaded = true;

            asyncLoadAbrEvent += asyncAction;

            assetBundleRequest = assetBundle.LoadAssetAsync(ObjectName);
            while (!assetBundleRequest.isDone)
            {
                asyncLoadAbrEvent(false, assetBundleRequest.progress, null, null);
                yield return null;
            }
            if (assetBundleRequest.progress != 1)
                asyncLoadAbrEvent(false, 1, null, null);

            yield return null;

            if (assetBundleRequest.asset == null)
            {
                asyncLoadAbr_error = ResourcesManagement.KSwordKitName + ": 资源加载失败! 请检查参数 assetPath 是否正确, assetPath=" + path;
                asyncLoadAbrEvent(true, 1, asyncLoadAbr_error, null);
            }
            else
            {
                try
                {
                    if (assetBundleRequest.asset != null)
                    {
                        // 资源加载成功后，存入资源缓存中
                        Object = assetBundleRequest.asset;
                        asyncLoadAbrEvent(true, 1, null, Object);
                    }
                    else
                    {
                        asyncLoadAbr_error = ResourcesManagement.KSwordKitName + ": 资源加载失败! 请检查参数 assetPath 是否正确, assetPath=" + path;
                        asyncLoadAbrEvent(true, 1, asyncLoadAbr_error, null);
                    }
                }
                catch (System.Exception e)
                {
                    asyncLoadAbr_error = ResourcesManagement.KSwordKitName + ": 资源加载失败! 请检查参数 assetPath 是否正确, assetPath=" + path;
                    asyncLoadAbrEvent(true, 1, asyncLoadAbr_error, null);
                }
            }
        }
    }
}
