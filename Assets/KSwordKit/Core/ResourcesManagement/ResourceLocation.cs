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

public enum ResourceLoadLocationType
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

public interface ResourceLocation
{
	
}
