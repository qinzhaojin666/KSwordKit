/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ExamplesAssetManagementTest.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 6/7/2020
 *  File Description:
 *    Ignore.
 *************************************************************************/

using KSwordKit.Core;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class Launch : MonoBehaviour
{
    [Header("资源加载位置")]
    public KSwordKit.Core.ResourcesManagement.ResourcesLoadingLocation ResourcesLoadingLocation;


    void Start()
    {
        KSwordKit.Core.ResourcesManagement.ResourcesManagement.Init(ResourcesLoadingLocation)
            .OnInitializing((management, progress) =>
            {
                Debug.Log("正在初始化：进度" + progress);
            })
            .OnInitCompleted((management, error) =>
            {
                Debug.Log("初始化完成：error = " + (string.IsNullOrEmpty(error) ? "null" : error));
                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log("当前资源CRC：" + management.ResourcePackage.CRC);
                    foreach (var r in management.ResourcePackage.AssetBundleInfos)
                    {
                        Debug.Log("资源包：" + r.AssetBundleName + "\n路径：" + r.AssetBundlePath);
                        foreach (var d in r.Dependencies)
                        {
                            Debug.Log("\t依赖：" + d);
                        }
                        foreach (var item in r.ResourceObjects)
                        {
                            Debug.Log("\t内部的资源对象：" + item.ObjectName + "\n\t\t路径：" + item.ResourcePath);
                        }
                    }
                }
            });
    }
}
