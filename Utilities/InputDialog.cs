using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDialog : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

//using UnityEngine;
//using UnityEditor;

//public class ShowPopupExample : EditorWindow
//{
//    [MenuItem("Example/ShowPopup Example")]
//    static void Init()
//    {
//        ShowPopupExample window = ScriptableObject.CreateInstance<ShowPopupExample>();
//        window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
//        window.ShowPopup();
//    }

//    void OnGUI()
//    {
//        EditorGUILayout.LabelField("This is an example of EditorWindow.ShowPopup", EditorStyles.wordWrappedLabel);
//        GUILayout.Space(70);
//        if(GUILayout.Button("Agree!")) this.Close();
//    }
//}