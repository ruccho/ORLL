using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#else
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using System;
using System.Linq;

namespace ORLL.Unity.Editor
{
    internal class LocalizedTextField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<LocalizedTextField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Path =
                new UxmlStringAttributeDescription {name = "path", defaultValue = " "};

            UxmlStringAttributeDescription m_Key =
                new UxmlStringAttributeDescription {name = "key", defaultValue = " "};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var path = m_Path.GetValueFromBag(bag, cc);
                var key = m_Key.GetValueFromBag(bag, cc);
                ((LocalizedTextField) ve).Init(path, key);
            }
        }

        private List<LocalizationUnitHandle> Handles { get; set; } = new List<LocalizationUnitHandle>();
        private List<LocalizationPack> UnaddedPacks { get; set; } = new List<LocalizationPack>();


        public int HandleCount => Handles.Count;

        private Button refreshButton;

        public bool preventAutoGUI { get; set; }

        public string UnitPath { get; private set; }
        public string Key { get; private set; }

        public LocalizedTextField()
        {
            string styleSheetPath = "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizedTextField/LocalizedTextField.uss";
            #if UNITY_2018
            AddStyleSheetPath(styleSheetPath);
            #else
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                styleSheetPath);
            styleSheets.Add(ss);
#endif
            
        }

        public LocalizedTextField(string unitPath, string key, bool preventAutoGUI = false) : this()
        {
            this.preventAutoGUI = preventAutoGUI;
            Init(unitPath, key);
        }

        public void Init(string unitPath, string key)
        {
            UnitPath = unitPath;
            Key = key;
            Refresh();
        }

        private void RefreshButton_Clicked(EventBase eb)
        {
            Refresh();
        }

        private void AddLocalizationElement(LocalizationUnitHandle handle, string key)
        {
            var container = this.Q<Box>("localizations");
            var box = new Box();
            box.name = "localization-element";

            var label = new Label(handle.Pack.Name);
            label.name = "localization-label";

            var field = new TextField();
            field.name = "localization-field";

            field.value = handle.GetText(key);

            field.RegisterCallback<ChangeEvent<string>>((s) => { handle.SetText(key, s.newValue); });

            field.RegisterCallback<FocusInEvent>((e) => { field.SetValueWithoutNotify(handle.GetText(key)); });

            field.RegisterCallback<FocusOutEvent>((e) => { handle.SaveUnit(); });


            var removeButton = new Button();
            field.multiline = true;

            removeButton.clickable.clicked += () =>
            {
                handle.RemoveKey(key);
                Refresh();
            };

            removeButton.name = "localization-expand";
            removeButton.text = "Remove";

            box.Add(label);
            box.Add(field);
            box.Add(removeButton);
            container.Add(box);
        }

        private IMGUIContainer ImguiContainer;

        private void Refresh()
        {
            if (!preventAutoGUI)
            {
                Clear();

                if (Localizer.Instance == null)
                {
                    Add(new Label("Localizer is not loaded. Try refresh."));
                    return;
                }
            }

            foreach (var handle in Handles)
            {
                handle.Dispose();
            }

            Handles.Clear();
            UnaddedPacks.Clear();

            if (!string.IsNullOrEmpty(UnitPath) && !string.IsNullOrEmpty(Key))
                foreach (var pack in Localizer.Instance.Packs)
                {
                    var handle = pack.RequestUnitHandle(UnitPath);
                    if (!handle.IsEmpty())
                    {
                        if (handle.TryGetText(Key, out string value))
                        {
                            Handles.Add(handle);
                            continue;
                        }
                    }

                    UnaddedPacks.Add(pack);
                    handle.Dispose();
                }

            if (!preventAutoGUI)
            {
                ImguiContainer = new IMGUIContainer(() =>
                {
                    var r = this.ImguiContainer.contentRect;
                    r.width -= 20f;
                    r.x += 20f;
                    OnGUI(r);
                    this.ImguiContainer.style.height = GetPropertyHeight();
                });
                Add(ImguiContainer);
            }

            return;

            //以下、UIElements用コード
            /*

            var visualTree =
                AssetDatabase.LoadAssetAtPath(
                    "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizedTextField/LocalizedTextField.uxml",
                    typeof(VisualTreeAsset)) as VisualTreeAsset;
            VisualElement uxml = visualTree.CloneTree();
            this.Add(uxml);

            foreach (var handle in Handles)
            {
                AddLocalizationElement(handle, Key);
            }


            refreshButton = this.Q<Button>("refresh");
            refreshButton.clickable.clickedWithEventInfo += RefreshButton_Clicked;

            var addButton = this.Q<Button>("add");
            var m = new ContextualMenuManipulator((e) =>
            {
                if (!string.IsNullOrEmpty(UnitPath) && !string.IsNullOrEmpty(Key))
                    foreach (var pack in Localizer.Instance.Packs)
                    {
                        var handle = pack.RequestUnitHandle(UnitPath);
                        if (!handle.IsEmpty())
                        {
                            if (handle.TryGetText(Key, out string value))
                            {
                                handle.Dispose();
                                continue;
                            }
                        }

                        handle.Dispose();
                        e.menu.AppendAction($"{pack.Name} ({pack.Guid})", a =>
                            {
                                pack.RequestUnitHandle(UnitPath, true).AddKey(Key);

                                Refresh();
                            },
                            a => DropdownMenuAction.AlwaysEnabled(a));
                    }
            });
            m.target = addButton;
            m.activators.Clear();

            m.activators.Add(new ManipulatorActivationFilter() {button = MouseButton.LeftMouse, clickCount = 1});
            */
        }
        
        public void OnGUI(Rect r)
        {
            var line = EditorGUIUtility.singleLineHeight + 2;
            var cHeight = EditorGUIUtility.singleLineHeight;
            float b = r.y + 5;
            int i = 0;
            LocalizationUnitHandle handleToRemove = null;
            foreach (var h in Handles)
            {
                GUI.Label(new Rect(r.x, b + line * i * 2, 150f, cHeight), h.Pack.Name);

                var oldValue = h.GetText(Key);
                var newValue =
                    EditorGUI.TextArea(new Rect(r.x + 150f, b + line * i * 2, r.width - 150f - 60f, cHeight * 2), oldValue);
                if (oldValue != newValue)
                {
                    h.SetText(Key, newValue);
                    h.SaveUnit();
                }

                if (GUI.Button(new Rect(r.x + r.width - 60f, b + line * i * 2, 60f, cHeight * 2), "Remove"))
                {
                    handleToRemove = h;
                }

                i++;
            }
            
            
            if (handleToRemove != null)
            {
                handleToRemove.RemoveKey(Key);
                handleToRemove.SaveUnit();
                Refresh();
            }

            int selected = EditorGUI.Popup(new Rect(r.x, b + line * i * 2, 100f, cHeight), 0,
                    new string[] {"Add localization"}.Concat(
                        UnaddedPacks.Select(_ => $"{_.Name} ({_.Guid})")).ToArray())
                ;
            if (selected != 0)
            {
                var packToAdd = UnaddedPacks[selected - 1];

                packToAdd.RequestUnitHandle(UnitPath, true).AddKey(Key);

                Refresh();
            }
        }

        public float GetPropertyHeight()
        {
            var line = EditorGUIUtility.singleLineHeight + 2;
            return line * (1 + Handles.Count * 2) + 10f;
        }
    }
}