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
using System;

public class Launch : MonoBehaviour
{

    [Header("设置资源的加载位置")]
    public KSwordKit.Core.ResourcesManagement.ResourcesLoadingLocation ResourcesLoadingLocation;

    [Header("UI 根节点")]
    public GameObject UIRoot;

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
                                Instantiate(obj, UIRoot.transform).name = obj.name;
                            }
                            else
                                Debug.LogError("加载预制体 loadSceneButton 失败：" + _error);
                        }
                        else
                        {
                            Debug.Log("正在加载预制体 loadSceneButton ：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                        }
                    });
                    management.LoadAssetAsync("Assets/Examples/ExamplesResourcesManagement/Resources/texture/背景/背景光效.png", (_management, isdone, progress, _error, obj) =>{
                        if (isdone)
                        {
                            if (string.IsNullOrEmpty(_error))
                            {
                                Debug.Log("加载 背景图片 成功：" + obj.name);
                                var t = obj as Texture2D;
                                TestLoadAssetsPanel.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(t, new Rect(0,0,t.width,t.height), Vector2.zero);
                                // TestLoadAssetsPanel.transform.GetChild(0).GetComponent<Image>().sprite = t;
                            }
                            else
                                Debug.LogError("加载 背景图片 失败：" + _error);
                        }
                        else
                        {
                            Debug.Log("正在加载 背景图片 ：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                        }
                    });
                    // 加载第一组资源，并赋值
                    management.LoadAssetAsync(new string[] {
                         "Assets/Examples/ExamplesResourcesManagement/Resources/texture/个人信息/个人信息图标.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/个人信息/个人信息修改名字.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/1.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/2.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/5.png"
                    }, (_management, isdone, progress, _error, objs) => {
                        if(isdone)
                        {
                            if (!string.IsNullOrEmpty(_error))
                            {
                                Debug.LogError("加载第一组资源 失败：" + _error);
                                return;
                            }

                            Debug.Log("第一组资源全部加载成功");

                            for(var i =0; i < objs.Length; i++)
                            {
                                var t = objs[i] as Texture2D;
                                var image = TestLoadAssetsPanel.transform.GetChild(i + 1).GetComponent<Image>();
                                image.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), image.rectTransform.pivot);
                                image.SetNativeSize();
                            }
                        }
                        else
                        {
                            Debug.Log("正在加载第一组资源：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                        }
                    });
                    // 使用泛型加载资源
                    management.LoadAssetAsync<Sprite>("Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/6.png", (_management, isdone, progress, _error, obj) =>
                    {
                        if (isdone)
                        {
                            if (string.IsNullOrEmpty(_error))
                            {
                                Debug.Log("加载 按钮 成功：" + obj.name);
                                TestLoadAssetsPanel.transform.GetChild(6).GetComponent<Image>().sprite = obj;
                                TestLoadAssetsPanel.transform.GetChild(6).GetComponent<Image>().SetNativeSize();
                            }
                            else
                                Debug.LogError("加载 按钮 失败：" + _error);
                        }
                        else
                        {
                            Debug.Log("正在加载 按钮 ：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                        }
                    });
                    // 加载第二组资源
                    management.LoadAssetAsync<Sprite>(new string[] {
                         "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/视屏.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/图标/8.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/按钮/项目升级按钮动画.png",
                        "Assets/Examples/ExamplesResourcesManagement/Resources/texture/个人信息/个人信息图标.png"
                    }, (_management, isdone, progress, _error, objs) => {
                        if (isdone)
                        {
                            if (!string.IsNullOrEmpty(_error))
                            {
                                Debug.LogError("加载第二组资源 失败：" + _error);
                                return;
                            }

                            Debug.Log("第二组资源全部加载成功");

                            for (var i = 0; i < objs.Length; i++)
                            {
                                var image = TestLoadAssetsPanel.transform.GetChild(i + 7).GetComponent<Image>();
                                image.sprite = objs[i];
                                image.SetNativeSize();
                            }
                        }
                        else
                        {
                            Debug.Log("正在加载第二组资源：" + progress);
                            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                        }
                    });
                }
                else
                {
                    Debug.LogError("发生错误：" + error);
                }
            });
    }
}
