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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExamplesAssetManagementTest : MonoBehaviour
{   
    public Image[] TestImages;
    public Button LoadSceneButton;
    void Start()
    {
        Debug.Log(Application.platform.ToString());
        //Debug.Log("资源路径："+ KSwordKit.Core.ResourcesManagement.ResourcePath);
        
        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync("", (r)=>{
            
        //}, (error)=>{
        //    Debug.LogError(error);
        //}, (progress)=>{
        //    Debug.Log("场景加载中..."+ progress);
        //});

        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Texture2D>(
        //    "Assets/Resources/Textures/Loading/log.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+ r.name);
        //        TestImages[0].sprite = Sprite.Create(r, new Rect(0,0, r.width,r.height), Vector2.zero);
        //        // TestImage.SetNativeSize();
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    },
        //    (progress)=>{
        //        Debug.Log("log.png 进度：" + progress * 100 +"%");
        //    }
        //);

        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Texture2D>(
        //    "Assets/Resources/Textures/投资/投资按钮亮.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+ r.name);
        //        TestImages[1].sprite = Sprite.Create(r, new Rect(0,0, r.width,r.height), Vector2.zero);
        //        // TestImages[0].SetNativeSize();
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    },
        //    (progress)=>{
        //        Debug.Log("投资按钮亮.png 进度：" + progress * 100 +"%");
        //    }
        //);

        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Texture2D>(
        //    "Assets/Resources/Textures/投资/投资图标.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+ r.name);
        //        TestImages[2].sprite = Sprite.Create(r, new Rect(0,0, r.width,r.height), Vector2.zero);
        //        //  TestImages[1].SetNativeSize();
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    },
        //    (progress)=>{
        //        Debug.Log("投资图标.png 进度：" + (progress * 100) +"%");
        //    }
        //);

        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync(
        //    "Assets/Resources/Textures/投资/投资标题.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+r.name);
        //        var t = r as Texture2D;
        //        TestImages[3].sprite = Sprite.Create(t, new Rect(0,0, t.width,t.height), Vector2.zero);
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    }
        //);
        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Sprite>(
        //    "Assets/Resources/Textures/投资/投资按钮灰.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+r.name);
        //        TestImages[4].sprite = r;
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    }
        //);
        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Sprite>(
        //    "Assets/Resources/Textures/投资/投资界面.png",
        //    (r)=>{
        //        Debug.Log("加载成功！name="+r.name);
        //        TestImages[5].sprite = r;
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    }
        //);

        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Sprite>(
        //    new string[]{
        //        "Assets/Resources/Textures/败家之路/败.png",
        //        "Assets/Resources/Textures/败家之路/败家之路图标.png",
        //        "Assets/Resources/Textures/通用/4room.png"
        //    },
        //    (rarr)=>{

        //        for(var i = 0; i < rarr.Length; i++)
        //        {
        //            var r = rarr[i];
        //            Debug.Log("加载成功！name="+r.name);
        //            TestImages[i+6].sprite = r;
        //        }
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    },
        //    (progress)=>{
        //        Debug.Log("加载 一组资源 6 - 9， 进度：" + progress);
        //    }
        //);
        //KSwordKit.Core.ResourcesManagement.LoadAssetAsync<Sprite>(
        //    new string[]{
        //        "Assets/Resources/Textures/通用/红点.png",
        //        "Assets/Resources/Textures/通用/视屏.png",
        //        "Assets/Resources/Textures/通用/4字广告.png",
        //        "Assets/Resources/Textures/通用/gb.png",
        //        "Assets/Resources/Textures/通用/room5.png"
        //    },
        //    (rarr)=>{

        //        for(var i = 0; i < rarr.Length; i++)
        //        {
        //            var r = rarr[i];
        //            Debug.Log("加载成功！name="+r.name);
        //            TestImages[i+9].sprite = r;
        //        }
        //    },
        //    (error)=>{
        //        Debug.LogError(error);
        //    },
        //    (progress)=>{
        //        Debug.Log("加载 一组资源 9 - 12， 进度：" + progress);
        //    }
        //);
    }
}
