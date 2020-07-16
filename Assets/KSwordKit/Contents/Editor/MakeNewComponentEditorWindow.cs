using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KSwordKit.Contents.Editor
{
    public class MakeNewComponentEditorWindow : EditorWindow
    {
        static MakeNewComponentEditorWindow window;

        public static void Open()
        {

            window = GetWindow<MakeNewComponentEditorWindow>(false, KSwordKitConst.KSwordKitName + ": " + ContentsEditor.MakeNewWindowTitle);

            window.Show();
        }
    }

}