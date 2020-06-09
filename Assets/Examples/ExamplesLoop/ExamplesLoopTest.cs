/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ExamplesLoopTest.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-7
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExamplesLoopTest : MonoBehaviour
{

    void Start()
    {
        // 每帧while循环一次
        var i = 0;
        KSwordKit.Core.Loop.While(()=> i < 10, ()=>{
            Debug.Log("这是 while 第"+ i++ +"次While 循环(每帧)。");
        });

        // 每一秒执行一次while循环
        var j = 0;
        KSwordKit.Core.Loop.While(()=> j < 10, ()=>{
            Debug.Log("这是 while 第"+ j++ +"次While 循环(每一秒)。");
        }, 1);

        // 每帧执行一次 do while 循环
        var k = 0;
        KSwordKit.Core.Loop.Do___While(()=>{
            Debug.Log("这是 do while 第" + k++ +"次循环(每帧)。");
        }, ()=> k > 10);

        // 每秒执行一次 do while 循环
        var t = 0;
        KSwordKit.Core.Loop.Do___While(()=>{
            Debug.Log("这是 do while 第" + t++ +"次循环(每妙)。");
        }, ()=> t < 10, 1);
    }

    void Update()
    {
        
    }
}
