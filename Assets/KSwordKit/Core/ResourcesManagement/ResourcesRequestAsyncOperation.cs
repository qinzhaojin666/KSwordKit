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
    public class ResourcesRequestAsyncOperation : CustomYieldInstruction
    {
        public ResourcesRequestAsyncOperation(string path)
        {
            
        }

        public override bool keepWaiting
        {
            get
            {
                return true;
            }
        }
    }


    public class ResourcesRequestAsyncOperation<T> : CustomYieldInstruction where T: UnityEngine.Object
    {
        public ResourcesRequestAsyncOperation(string path)
        {

        }

        public override bool keepWaiting
        {
            get
            {
                return true;
            }
        }
    }

}

