using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class PropertyDrawerSample : MonoBehaviour
{
    [SerializeField] private PD p;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("BBB");   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class PD
{
    [SerializeField] private string text;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(PD))]
public class PDDrawer : PropertyDrawer
{
    /*
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();
        root.Add(new Label("ABC"));
        return root;
    }
*/
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        
    }
}
#endif