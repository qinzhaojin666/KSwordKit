/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ExamplesCustomYieldInstruction.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-10
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class CustomYield : CustomYieldInstruction
{
    float m_time;
    public CustomYield(float time)
    {
        m_time = Time.realtimeSinceStartup + time;
    }

    public override bool keepWaiting
    {
        get
        {
            return Time.realtimeSinceStartup < m_time;
        }
    }
}


public class ExamplesCustomYieldInstruction : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(testCustomYield());
    }

    IEnumerator testCustomYield()
    {
        yield return new CustomYield(2);
        Debug.Log("使用自定义协程类:" + Time.realtimeSinceStartup);
        StartCoroutine(testCustomYield());
    }


    void Update()
    {
        
    }
}
