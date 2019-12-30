using System.Collections;
using System.Collections.Generic;
using ORLL.Unity;
using UnityEditor;
using UnityEngine;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#else
using UnityEngine.UIElements;
#endif

namespace ORLL.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        /*
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Debug.Log("BBB");
            var root = new VisualElement();

            var pathProp = property.FindPropertyRelative("path");

            var pathElement = new TextField("path");

            pathElement.bindingPath = pathProp.propertyPath;


            var localizedTextField = new LocalizedTextField();

            var path = pathProp.stringValue;

            string[] splitted = path?.Split('/');

            if (splitted != null && splitted.Length == 3)
            {
                localizedTextField.Init(splitted[0] + '/' + splitted[1], splitted[2]);
            }

            pathElement.RegisterCallback<ChangeEvent<string>>((s) =>
            {
                string[] sp = s.newValue.Split('/');

                if (sp.Length == 3)
                {
                    localizedTextField.Init(splitted[0] + '/' + splitted[1], splitted[2]);
                }
            });

            root.Add(pathElement);
            root.Add(localizedTextField);

            return root;
        }
        */
        
        private LocalizedTextField element { get; set; }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect r = new Rect(position.x, position.y, position.width, position.height);
            EditorGUI.DrawRect(r, new Color(0.87f, 0.87f, 0.87f, 1.0f));

            
            float line = EditorGUIUtility.singleLineHeight;
            
            var pathProp = property.FindPropertyRelative("path");

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, line), pathProp, new GUIContent(property.displayName));
            
            var path = pathProp.stringValue;
            string[] splitted = path?.Split('/');

            if (splitted == null || splitted.Length != 3)
            {
                return;
            }

            var unitPath = splitted[0] + '/' + splitted[1];
            var key = splitted[2];

            if (element == null)
            {
                element = new LocalizedTextField(unitPath, key, true);
            }
            else
            {
                if (element.UnitPath != unitPath || element.Key != key)
                {
                    Debug.Log(unitPath + " " + key);
                    element.Init(unitPath, key);
                }
                element.OnGUI(new Rect(position.x + line, position.y + line, position.width - line, position.height - line));
            }
            
            //GUILayout.BeginArea(new Rect(position.x, position.y, position.width, position.height - line));            element.OnGUI(new Rect(position.x, position.y, position.width, position.height - line));

            //GUILayout.EndArea();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (element != null ? element.GetPropertyHeight() : 0);
        }
    }
}