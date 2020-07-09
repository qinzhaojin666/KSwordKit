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

        public const string ImportChild_Assets = "Assets/KSwordKit/框架开发/导入部件";
        public const string ImportChild = "KSwordKit/框架开发/导入部件 _%#I";
        public const string ImportWindowTitle = "导入部件";
        public const string ImportConfigFileName = "ImportConfig.json";
        public const string Import_Tag_ProjectRootDirectory = "{ProjectRootDirectory}";

        public const string DeleteChild_AlreadyImport_Assets = "Assets/KSwordKit/框架开发/删除已导入部件";
        public const string DeleteChild_AlreadyImport = "KSwordKit/框架开发/删除已导入部件 _%#D";
        public const string DeleteImportWindowTitle = "导入部件";

        public const string ExportChild_Assets = "Assets/KSwordKit/框架开发/导出部件";
        public const string ExportChild = "KSwordKit/框架开发/导出部件 _%&O";
        public const string ExportWindowTitle = "导出部件";

        public const string DeleteChild_AlreadyExport_Assets = "Assets/KSwordKit/框架开发/删除已导出部件";
        public const string DeleteChild_AlreadyExport = "KSwordKit/框架开发/删除已导出部件 _%&D";
        public const string DeleteExportWindowTitle = "删除已导出部件";


        [MenuItem(ImportChild_Assets, false,0)]
        [MenuItem(ImportChild, false, 0)]
        public static void ImportChildFunction()
        {
            ImportChildWindow.Open();
        }
        
        [MenuItem(DeleteChild_AlreadyImport_Assets, false, 1)]
        [MenuItem(DeleteChild_AlreadyImport, false, 1)]
        public static void DeleteChildFunction()
        {
            ImportChildWindow.Open();
        }

        [MenuItem(ExportChild_Assets, false, 2)]
        [MenuItem(ExportChild, false, 2)]
        public static void ExportChildFunction()
        {
            ImportChildWindow.Open();

        }

        [MenuItem(DeleteChild_AlreadyExport_Assets, false, 3)]
        [MenuItem(DeleteChild_AlreadyExport, false, 3)]
        public static void DeleteExportChildFunction()
        {
            ImportChildWindow.Open();

        }

    }
}
