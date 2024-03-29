﻿/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: NewBehaviourScript.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-8
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSwordKit.Contents.ResourcesManagement.GeneratedResourcePath
{
    public class GeneratedResourcePathObject
    {
        public int IndentModulus = 1;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 子路径对象们
        /// </summary>
        public List<GeneratedResourcePathObject> Objects = new List<GeneratedResourcePathObject>();

        string classStr = @"
{缩进}/// <summary>
{缩进}/// 此处的真实资源路径为: {路径}
{缩进}/// <para>使用 API：<see cref=""{类型名}.Path""/> 获取该路径的字符串</para>
{缩进}/// </summary>
{缩进}public class {类型名}
{缩进}{
{缩进}    /// <summary>
{缩进}    /// 真实资源路径：{路径}
{缩进}    /// </summary>
{缩进}    public const string Path = ""{路径}"";
{缩进}    {字符串替换位置}
{缩进}}
";

        /// <summary>
        /// 
        /// </summary>
        public string ClassImplementation
        {
            get
            {
                var str = classStr.Replace("{类型名}", Name);
                var indentStr = "";
                for (var i = 0; i < IndentModulus; i++)
                    indentStr += "\t";
                str = str.Replace("{缩进}", indentStr);
                str = str.Replace("{路径}", Path);

                var objectsStrs = "";
                foreach (var o in Objects)
                {
                    o.IndentModulus += IndentModulus;
                    objectsStrs += o.ClassImplementation;
                }

                str = str.Replace("{字符串替换位置}", objectsStrs);
                return str;
            }
        }

    }
}
