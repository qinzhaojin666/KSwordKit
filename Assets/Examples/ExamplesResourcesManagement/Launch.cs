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

    [Header("进度条")]
    public GameObject ProgressPanel;
    public RectTransform ProgressParentRT;
    public Image ProgressImage;
    public Text ProgressText;

    [Header("测试加载资源面板")]
    public GameObject TestLoadAssetsPanel;

    void Start()
    {
        ProgressImage.rectTransform.sizeDelta = new Vector2(0, ProgressImage.rectTransform.sizeDelta.y);
        // 初始化
        KSwordKit.Core.ResourcesManagement.ResourcesManagement.Init(ResourcesLoadingLocation)
            .OnInitializing
            ((management, progress) =>
            {
                Debug.Log("正在初始化：进度 -> " + progress);
                ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
            })
            .OnInitCompleted
            ((management, error) =>
            {
                Debug.Log("初始化完成：error = " + (string.IsNullOrEmpty(error) ? "null" : error));
                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log("当前资源CRC：" + management.ResourcePackage.CRC);
                    foreach (var r in management.ResourcePackage.AssetBundleInfos)
                    {
                        Debug.Log("资源包：" + r.AssetBundleName + "，路径：" + r.AssetBundlePath);
                        foreach (var d in r.Dependencies)
                        {
                            Debug.Log("\t依赖包：" + d);
                        }
                        foreach (var item in r.ResourceObjects)
                        {
                            Debug.Log("\t内部的资源对象：" + item.ObjectName + "\n\t\t路径：" + item.ResourcePath);
                        }
                    }

                    ProgressImage.rectTransform.sizeDelta = new Vector2(0, ProgressImage.rectTransform.sizeDelta.y);
                    // 加载资源并实例化
                    management.LoadAssetAsync("Assets/Examples/ExamplesResourcesManagement/Resources/prefabs/loadSceneButton.prefab", (_management, isdone, progress, _error, obj) =>
                    {
                        if (isdone)
                        {
                            if (string.IsNullOrEmpty(_error))
                            {
                                Debug.Log("加载预制体 loadSceneButton 成功：" + obj.name);
                                Instantiate(obj, GameObject.Find("Canvas").transform).name = obj.name;
                            }
                            else
                                Debug.LogError("加载预制体 loadSceneButton 失败：" + _error);
                        }
                        else
                        {
                            Debug.Log("正在加载预制体 loadSceneButton ：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress*100).ToString("f2") + "%";
                        }
                    });
                    // 加载一组资源，并赋值
                    management.LoadAssetAsync<Sprite>(new string[] {
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/个人信息/个人信息图标.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/个人信息/个人信息修改名字.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/1.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/2.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/5.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/背景/背景光效.png"
                    }, (_management, isdone, progress, _error, objs) =>
                        {
                            if (isdone)
                            {
                                if (string.IsNullOrEmpty(_error))
                                    Debug.Log("加载一组资源全部成功：" + objs.Length);
                                else
                                    Debug.LogError("加载一组资源 失败：" + _error);

                                for(var i = 0; i < TestLoadAssetsPanel.transform.childCount;i++)
                                {
                                    TestLoadAssetsPanel.transform.GetChild(i).GetComponent<Image>().sprite = objs[i] as Sprite;
                                }
                            }
                            else
                            {
                                Debug.Log("正在加载一组资源：" + progress);
                                ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                                ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                            }
                        });
                }
            });
    }
}
