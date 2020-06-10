/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: IResourcesSyncLoader.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-11
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Core.ResourcesManagement
{
	/// <summary>
	/// 定义同步加载资源的几个接口
	/// </summary>
	public interface IResourcesSyncLoader
	{
		/// <summary>
		/// 根据 assetPath 加载资源，resultAction中将加载结果异步回调给用户
		/// </summary>
		/// <param name="assetPaths">资源路径</param>
		/// <returns>资源加载结果； 加载成功，返回资源对象； 如果加载出错，返回 null</returns>
		UnityEngine.Object Load(params string[] assetPaths);
		T Load<T>(params string[] assetPaths) where T : UnityEngine.Object;

		UnityEngine.Object Load(string[] assetPaths, ref string error);
		T Load<T>(string[] assetPaths, ref string error) where T : UnityEngine.Object;

		UnityEngine.Object Load(string assetPaths, ref string error);
		T Load<T>(string assetPaths, ref string error) where T : UnityEngine.Object;
	}
}
