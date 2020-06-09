/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: Resource.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-10
 *  File Description: Ignore.
 *************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Core.ResourcesManagement
{
    /// <summary>
    /// 关于资源加载位置的枚举
    /// </summary>
    public enum LoadLocationType
    {
        /// <summary>
        /// 本地资源，使用 Unity 默认的 Resources.Load 加载
        /// </summary>
        Resources,
        /// <summary>
        /// 本地资源，位于 StreamingAssets 中
        /// </summary>
        StreamingAssets,
        /// <summary>
        /// 远程资源，使用前，需要通过网络下载到本地，然后加载
        /// </summary>
        Remote
    }

    /// <summary>
    /// 资源位置的接口
    /// </summary>
    public interface IResourceLocation
    {
        /// <summary>
        /// 获取资源位置的方法
        /// </summary>
        /// <param name="loadLocationType">LoadLocationType的枚举项</param>
        /// <returns></returns>
        string GetResourceLocation(LoadLocationType loadLocationType);
        /// <summary>
        /// 当资源位于远程服务器时，设置获取远程资源的办法
        /// </summary>
        /// <param name="url">请求远程服务器的 url</param>
        /// <param name="method">请求方法 get 或 post</param>
        /// <param name="param">请求参数</param>
        /// <returns></returns>
        //public void SetRemoteResourceLocation(string url, string method, string param);
    }
}

