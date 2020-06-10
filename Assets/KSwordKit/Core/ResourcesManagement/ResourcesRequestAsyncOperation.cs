/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: IResourcesRequest.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-10
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace KSwordKit.Core.ResourcesManagement
{
    public class ResourcesRequestAsyncOperation
    {
        protected string _resourcePath;
        /// <summary>
        /// 加载的资源路径
        /// </summary>
        public string resourcePath { get { return _resourcePath; } }
        protected bool _isdone;
        /// <summary>
        /// 指示异步操作是否完成
        /// </summary>
        public bool isDone { get { return _isdone; } }
        protected float _progress;
        /// <summary>
        /// 指示异步操作的进度
        /// </summary>
        public float progress { get { return _progress; } }
        protected bool _allowSceneActivation;
        /// <summary>
        /// 当加载的资源是场景时，指示是否在场景加载完毕后激活场景
        /// </summary>
        public bool allowSceneActivation { get; set; }
        protected string _error = null;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get { return _error; } }
        protected UnityEngine.Object _asset;
        /// <summary>
        /// 加载到的资源
        /// </summary>
        public UnityEngine.Object asset;
        protected UnityEngine.Object[] _assets;
        /// <summary>
        /// 加载到的所有资源
        /// </summary>
        public UnityEngine.Object[] assets;

    }


    public class ResourcesRequestAsyncOperation<T> where T: UnityEngine.Object
    {
        protected string _resourcePath;
        /// <summary>
        /// 加载的资源路径
        /// </summary>
        public string resourcePath { get { return _resourcePath; } }
        protected bool _isdone;
        /// <summary>
        /// 指示异步操作是否完成
        /// </summary>
        public bool isDone { get { return _isdone; } }
        protected float _progress;
        /// <summary>
        /// 指示异步操作的进度
        /// </summary>
        public float progress { get { return _progress; } }
        protected bool _allowSceneActivation;
        /// <summary>
        /// 当加载的资源是场景时，指示是否在场景加载完毕后激活场景
        /// </summary>
        public bool allowSceneActivation { get; set; }
        protected string _error = null;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get { return _error; } }
        protected T _asset;
        /// <summary>
        /// 加载到的资源
        /// </summary>
        public T asset;
        protected T[] _assets;
        /// <summary>
        /// 加载到的所有资源
        /// </summary>
        public T[] assets;
    }
}

