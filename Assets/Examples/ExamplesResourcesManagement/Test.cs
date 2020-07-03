/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: Test.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-3
 *  File Description: Ignore.
 *************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Transform UIRootTransform;

    void Start()
    {
        KSwordKit.Core.ResourcesManagement.ResourcesManager.Instance.LoadAssetAsync<GameObject>("Assets/Examples/ExamplesResourcesManagement/Resources/prefabs/loadSceneButton.prefab", (isdone, progress, error, obj) => {
            if (isdone)
            {
                var o = Instantiate(obj, UIRootTransform);
                var btn = o.GetComponent<UnityEngine.UI.Button>();
                o.GetComponentInChildren<Text>().text = "加载Test2场景";
                btn.onClick.AddListener(() => {

                    KSwordKit.Core.ResourcesManagement.ResourcesManager.Instance.LoadSceneAsync("Assets/Examples/ExamplesResourcesManagement/Test2.unity", (Test2) => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(Test2), (_isdone, _progress, _error, sceneInfos) => {
                        if (_isdone)
                        {
                            Debug.Log("场景加载完毕！");
                            return;
                        }
                    });
                });
                return;
            }
        });
    }


    void Update()
    {
        
    }
}
