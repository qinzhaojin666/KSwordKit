/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: SceneInfo.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-2
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Core.ResourcesManagement
{
    /// <summary>
    /// 场景信息类
    /// </summary>
    public class SceneInfo : UnityEngine.Object
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; internal set; }
        /// <summary>
        /// 场景资源所在路径
        /// </summary>
        public string SceneAssetPath { get; internal set; }
        bool _allowSceneActivation = true;
        /// <summary>
        /// 一旦场景资源加载好了之后，是否允许激活场景
        /// <para>默认为true</para>
        /// </summary>
        public bool AllowSceneActivation {
            get { return _allowSceneActivation; } 
            set { _allowSceneActivation = value; }
        }
        /// <summary>
        /// 功能等同于 <see cref="UnityEngine.AsyncOperation.priority"/>
        /// </summary>
        public int Priority;
        /// <summary>
        /// 场景激活事件
        /// <para>回调参数的含义：是否激活完成、激活进度、激活过程中产生的错误信息</para>
        /// </summary>
        public event System.Action<bool, float, string> SceneActivationEvent;
        /// <summary>
        /// 场景激活状态
        /// </summary>
        /// <param name="isdone">是否完成</param>
        /// <param name="progress">进度</param>
        /// <param name="error">错误信息</param>
        internal void SceneActivationStatus(bool isdone, float progress, string error)
        {
            if (SceneActivationEvent != null)
                SceneActivationEvent(isdone, progress, error);
        }
    }

}
