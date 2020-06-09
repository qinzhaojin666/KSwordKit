/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: Loop.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-6-7
 *  File Description: Ignore.
 *************************************************************************/
namespace KSwordKit.Core
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loop : MonoBehaviour
{
    const string NAME = "Timer";
    static Loop _instance;
    static Loop instance{
        get{
            if(_instance == null)
                _instance = new GameObject(NAME).AddComponent<Loop>();
            return _instance;
        }
    }

    public static void While(System.Func<bool> conditionFunc, System.Action action, float timeIntervalOfWaitNextExecution = 0)
    {
        if(conditionFunc == null || action == null)
        {
            Debug.LogError("循环条件和执行内容不能为空！");
            return;
        }
        if(conditionFunc())
            instance.StartCoroutine(_while(conditionFunc, action,timeIntervalOfWaitNextExecution));
    }

    static IEnumerator _while(System.Func<bool> conditionFunc, System.Action action, float timeIntervalOfWaitNextExecution = 0){
        if(timeIntervalOfWaitNextExecution <= 0)
            yield return new WaitForEndOfFrame();
        else
            yield return new WaitForSecondsRealtime(timeIntervalOfWaitNextExecution);
        action();
        if(conditionFunc())
            instance.StartCoroutine(_while(conditionFunc, action,timeIntervalOfWaitNextExecution));
    }

    public static void Do___While(System.Action doAction, System.Func<bool> conditionFunc, float timeIntervalOfWaitNextExecution = 0)
    {
        if(conditionFunc == null || doAction == null)
        {
            Debug.LogError("循环条件和执行内容不能为空！");
            return;
        }
       instance.StartCoroutine(do___while(doAction, conditionFunc,timeIntervalOfWaitNextExecution));
    }

    static IEnumerator do___while(System.Action doAction, System.Func<bool> conditionFunc, float timeIntervalOfWaitNextExecution = 0)
    {
        if(timeIntervalOfWaitNextExecution <= 0)
            yield return new WaitForEndOfFrame();
        else
            yield return new WaitForSecondsRealtime(timeIntervalOfWaitNextExecution);
        doAction();
        if(conditionFunc())
            instance.StartCoroutine(do___while(doAction, conditionFunc,timeIntervalOfWaitNextExecution));
    }
}

}