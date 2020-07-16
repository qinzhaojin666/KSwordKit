/*************************************************************************
 *  Copyright (C), 2020-2021. All rights reserved.
 *
 *  FileName: ComponentsEditor.cs
 *  Author: ks   
 *  Version: 1.0.0   
 *  CreateDate: 2020-7-9
 *  File Description: Ignore.
 *************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KSwordKit.Contents.Editor
{
    public class ContentsEditor
    {
        public const string Update_Assets = "Assets/KSwordKit/框架/更新框架";
        public const string Update = "KSwordKit/框架/更新框架 _%#U";

        public const string ImportChild_Assets = "Assets/KSwordKit/框架/导入部件";
        public const string ImportChild = "KSwordKit/框架/导入部件 _%#I";
        public const string ImportWindowTitle = "导入部件";
        public const string ImportConfigFileName = "ImportConfig.json";

        public const string DeleteChild_AlreadyImport_Assets = "Assets/KSwordKit/框架/删除已导入部件";
        public const string DeleteChild_AlreadyImport = "KSwordKit/框架/删除已导入部件 _%#D";
        public const string DeleteImportWindowTitle = "导入部件";

        public const string MakeNew_Assets = "Assets/KSwordKit/框架/制作新部件";
        public const string MakeNew = "KSwordKit/框架/制作新部件 _%&N";
        public const string MakeNewWindowTitle = "制作新部件";

        public const string About_Assets = "Assets/KSwordKit/框架/关于作者";
        public const string AboutUs = "KSwordKit/框架/关于作者 _%&M";
        public const string AboutUsWindowTitle = "关于作者";


        [MenuItem(Update_Assets, false, 0)]
        [MenuItem(Update, false, 0)]
        public static void UpdateFunction()
        {
            Application.OpenURL("https://github.com/keenlovelife/KSwordKit.git");
        }

        [MenuItem(ImportChild_Assets, false,20)]
        [MenuItem(ImportChild, false, 20)]
        public static void ImportChildFunction()
        {
            ImportChildWindow.Open();
        }
        
        [MenuItem(DeleteChild_AlreadyImport_Assets, false, 21)]
        [MenuItem(DeleteChild_AlreadyImport, false, 21)]
        public static void DeleteChildFunction()
        {
            ImportChildWindow.Open();
        }

        [MenuItem(MakeNew_Assets, false, 40)]
        [MenuItem(MakeNew, false, 40)]
        public static void MakeNewFunction()
        {
            MakeNewComponentEditorWindow.Open();
        }

        [MenuItem(About_Assets, false, 1000)]
        [MenuItem(AboutUs, false, 1000)]
        public static void AboutFunction()
        {
            Application.OpenURL("https://github.com/keenlovelife");
        }
    }
}
