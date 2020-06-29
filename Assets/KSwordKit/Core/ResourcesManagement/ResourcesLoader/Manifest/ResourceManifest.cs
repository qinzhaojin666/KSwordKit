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
        /// <summary>
        /// 资源包引用计数
        /// </summary>
        [NonSerialized]
        int AssetBundleCount;
        /// <summary>
        /// 异步加载时设置超时时间
        /// </summary>
        [NonSerialized]
        public int AsyncLoadTimeout;
        [NonSerialized]
        string error;
        [NonSerialized]
        bool isDone;
        [NonSerialized]
        bool asyncloaded;
        /// <summary>
        /// 加载状态事件
        /// </summary>
        event System.Action<bool, float, string, AssetBundle> LoadingStatusEvent;
        /// <summary>
        /// 加载资源包方法
        /// </summary>
        /// <param name="action">回调动作</param>
        /// <returns>协程</returns>
        public IEnumerator AsyncLoad(System.Action<bool, float, string, AssetBundle> action)
        {
            if(asyncloaded)
            {
                if (isDone)
                {
                    action(false, 1, null, null);
                    ResourcesManagement.NextFrame(() => action(isDone, 1, error, AssetBundle));
                }
                else
                    LoadingStatusEvent += action;

                yield break;
            }

            if (!asyncloaded)
                asyncloaded = true;
            LoadingStatusEvent += action;


            float dc = Dependencies.Count;
            float cc = 0;
            for (var i = 0; i < dc; i++)
            {
                yield return ResourcesManagement.Instance.AssetbundleName_AssetBundlePathDic[Dependencies[i]].AsyncLoad((isdone, progress, _error, obj) => {

                    if (!string.IsNullOrEmpty(error))
                    {
                        cc++;
                        return;
                    }
                    if(isdone)
                    {   
                        error = _error;
                        cc++;
                        return;
                    }
                    LoadingStatusEvent(false, 0.5f * progress, null, null);
                });
            }


            while (cc != dc)
                yield return null;

            if (!string.IsNullOrEmpty(error))
            {
                LoadingStatusEvent(false, 1, error, null);
                yield break;
            }

            var rootDir = System.IO.Path.Combine(ResourcesManagement.GetResourcesFileRootDirectory(), ResourcesManagement.ResourceRootDirectoryName);
            var unityWebRequest = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle("file://" + System.IO.Path.Combine(rootDir, AssetBundlePath));
            unityWebRequest.timeout = AsyncLoadTimeout;
            var op = unityWebRequest.SendWebRequest();
            while (!op.isDone)
            {
                isDone = op.isDone;
                LoadingStatusEvent(false, op.progress, null, null);
                yield return null;
            }

            isDone = op.isDone;
            if (op.progress != 1f)
                LoadingStatusEvent(false, 1, null, null);

            yield return null;

            if (string.IsNullOrEmpty(unityWebRequest.error))
            {
                try
                {
                    AssetBundle ab = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(unityWebRequest);

                    if (ab != null)
                    {
                        AssetBundle = ab;
                        LoadingStatusEvent(true, 1, null, ab);
                    }
                    else
                    {
                        error = ResourcesManagement.KSwordKitName + ": 获取资源包失败！\nassetBunbleName=" + AssetBundleName + "\nAssetBundlePath=" + AssetBundlePath + "\nurl=" + unityWebRequest.url;
                        LoadingStatusEvent(true, 1, error, null);
                    }
            }
                catch (System.Exception e)
            {
                error = ResourcesManagement.KSwordKitName + ": 获取资源包失败！" + e.Message + "\nassetBunbleName=" + AssetBundleName + "\nAssetBundlePath=" + AssetBundlePath + "\nurl=" + unityWebRequest.url;
                LoadingStatusEvent(true, 1, error, null);
            }
        }
            else
            {
                error = ResourcesManagement.KSwordKitName + ": 获取资源包失败！" + unityWebRequest.error + "\nassetBunbleName=" + AssetBundleName + "\nAssetBundlePath=" + AssetBundlePath + "\nurl=" + unityWebRequest.url;
                LoadingStatusEvent(true, 1, error, null);
            }
        }
    }
}
