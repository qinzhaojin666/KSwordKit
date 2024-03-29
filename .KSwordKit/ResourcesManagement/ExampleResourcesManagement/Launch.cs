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

using KSwordKit.Contents;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using System;
using KSwordKit.Contents.ResourcesManagement.GeneratedResourcePath;

public class Launch : MonoBehaviour
{

    [Header("设置资源的加载位置")]
    public KSwordKit.Contents.ResourcesManagement.ResourcesLoadingLocation ResourcesLoadingLocation;

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

        // 使用资源管理器默认位置的资源清单内容进行初始化
        // 默认位置有枚举参数 ResourcesLoadingLocation 指定具体位置
        // 如果没有设置资源加载位置的值，默认是使用 Resources 的方式进行加载。
        var rmi = KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.Init(ResourcesLoadingLocation);

        //// 使用自定义位置(本地某位置)的资源清单文件内容进行初始化
        //var rmi = KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance;
        //var path = rmi.GetResourceListFilePath();
        //if (ResourcesLoadingLocation != KSwordKit.Contents.ResourcesManagement.ResourcesLoadingLocation.StreamingAssetsPath)
        //{
        //    string text = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
        //    rmi.Init(text);
        //}
        //else
        //{
        //    path = Application.dataPath.Replace("Assets", path);
        //    if (rmi.IsNeedAddLocalFilePathPrefix())
        //        path = "file://" + path;
        //    StartCoroutine(getfile(path, (text) =>
        //    {
        //        rmi.Init(text);
        //    }));
        //}
        //rmi.ResourcesLoadingLocation = ResourcesLoadingLocation;

        ////使用自定义位置(网络位置)的资源清单内容进行初始化（实例代码是使用本地文件模拟）
        //var rmi = KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance;
        //var path = rmi.GetResourceListFilePath();
        //path = Application.dataPath.Replace("Assets", path);
        //if (rmi.IsNeedAddLocalFilePathPrefix())
        //    path = "file://" + path;
        //var www = UnityEngine.Networking.UnityWebRequest.Get(path);
        //rmi.Init(www);
        //rmi.ResourcesLoadingLocation = ResourcesLoadingLocation;

        doSomething(rmi);

    }

    void doSomething(KSwordKit.Contents.ResourcesManagement.ResourcesManager rmi)
    {
        rmi.OnInitializedSuccessfully(() => {
            Debug.Log("初始化成功！当前资源CRC：" + rmi.ResourcePackage.CRC);
            foreach (var r in KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.ResourcePackage.AssetBundleInfos)
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

            KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync<GameObject>(
                Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.prefabs.loadSceneButton_prefab.Path, (isdone, progress, _error, obj) =>
            {
                if (isdone)
                {
                    if (string.IsNullOrEmpty(_error))
                    {

                        Debug.Log("加载预制体 loadSceneButton 成功, 名称：" + obj.name);
                        var go = Instantiate(obj, UIRoot.transform);
                        go.name = obj.name;
                        go.GetComponentInChildren<Text>().text = "加载Test场景";

                        string sceneName = null;
                        go.GetComponent<Button>().onClick.AddListener(() =>
                        {

                            rmi.LoadSceneAsync(Assets.KSwordKit.Examples.ExampleResourcesManagement.Test_unity.Path, (_sceneName) =>
                            {
                                sceneName = _sceneName;
                                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_sceneName);

                            }, (_isdone, _progress, __error, sceneinfo) =>
                            {
                                if (_isdone)
                                {
                                    Debug.Log("场景" + sceneName + "加载完成：" + (__error == null ? "NULL" : __error));
                                    return;
                                }

                                Debug.Log("正在加载场景 ：" + progress);
                                ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                                ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                            });
                        });
                    }
                    else
                        Debug.LogError("加载预制体 loadSceneButton 失败！\n" + _error);
                }
                else
                {
                    Debug.Log("正在加载预制体 loadSceneButton ：" + progress);
                    ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
                    ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
                }
            });

            KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync(Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.背景.背景光效_png.Path, (isdone, progress, _error, obj) =>
            {
                if (isdone)
                {
                    if (string.IsNullOrEmpty(_error))
                    {
                        Debug.Log("加载 背景图片 成功：" + obj.name);
                        var t = obj as Texture2D;
                        TestLoadAssetsPanel.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);

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
            KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync(new string[] {
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.个人信息.个人信息图标_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.个人信息.个人信息修改名字_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.图标._1_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.图标._2_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.图标._5_png.Path
                    }, (isdone, progress, _error, objs) =>
                    {
                        if (isdone)
                        {
                            if (!string.IsNullOrEmpty(_error))
                            {
                                Debug.LogError("加载第一组资源 失败：" + _error);
                                return;
                            }

                            Debug.Log("第一组资源全部加载成功");

                            for (var i = 0; i < objs.Length; i++)
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

            KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync<Sprite>(Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.图标._6_png.Path, (isdone, progress, _error, obj) =>
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
            KSwordKit.Contents.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync<Sprite>(new string[] {
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.视屏_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.图标._8_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.按钮.项目升级按钮动画_png.Path,
                        Assets.KSwordKit.Examples.ExampleResourcesManagement.Resources.texture.个人信息.个人信息图标_png.Path
                    }, (isdone, progress, _error, objs) =>
                    {
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

        });
        rmi.OnInitializationFailed((error) => {
            Debug.LogError("Launch -> 初始化时发生错误 -> " + error);
        });
        rmi.OnInitializing((progress) => {
            Debug.Log("正在初始化：进度 -> " + progress);
            ProgressImage.rectTransform.sizeDelta = new Vector2(progress * ProgressParentRT.rect.width, ProgressImage.rectTransform.sizeDelta.y);
            ProgressText.text = "加载进度: " + (progress * 100).ToString("f2") + "%";
        });
    }

    static IEnumerator getfile(string path, System.Action<string> action)
    {
        var www = UnityEngine.Networking.UnityWebRequest.Get(path);
        var op = www.SendWebRequest();
        while (!op.isDone)
        {
            yield return null;
        }

        if (string.IsNullOrEmpty(www.error))
        {
            var text = www.downloadHandler.text;
            action(text);
        }
        else
        {
            Debug.LogError("加载失败："+ www.error+"\nurl="+www.url);
        }
    }
}
