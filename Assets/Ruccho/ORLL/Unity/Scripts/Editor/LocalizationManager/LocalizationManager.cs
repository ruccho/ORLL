using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#else
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace ORLL.Unity
{
    public class LocalizationManager : EditorWindow
    {
        private LocalizationPack SelectedPack;

#if UNITY_2018
        private VisualElement rootVisualElement => this.GetRootVisualContainer();
#endif

        [MenuItem("Window/ORLL/Localization Manager")]
        public static void ShowExample()
        {
            LocalizationManager wnd = GetWindow<LocalizationManager>();
            wnd.titleContent = new GUIContent("Localization Manager");
        }

        public void OnEnable()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();

            var refreshButton = new Button(() => { InitializeUI(); });

            refreshButton.text = "Refresh";
            refreshButton.name = "refresh-button";

            string styleSheetPath =
                "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationManager/LocalizationManager_style.uss";
#if UNITY_2018
            refreshButton.AddStyleSheetPath(styleSheetPath);
#else
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            refreshButton.styleSheets.Add(ss);
#endif

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath(
                    "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationManager/LocalizationManager.uxml",
                    typeof(VisualTreeAsset)) as VisualTreeAsset;
#if UNITY_2018
            VisualElement labelFromUXML = visualTree.CloneTree(null);
#else
            VisualElement labelFromUXML = visualTree.CloneTree();
#endif


            styleSheetPath =
                "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationManager/LocalizationManager_style.uss";
#if UNITY_2018
            labelFromUXML.AddStyleSheetPath(styleSheetPath);
#else
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            labelFromUXML.styleSheets.Add(ss);
#endif

            root.Add(labelFromUXML);

            var packListContainer = root.Query<Box>("packs-listContainer").First();

            root.Q<Button>("packs-add").clickable.clicked += () =>
            {
                var newPack = Localizer.Instance.CreatePack("New Localization Pack", "New Localization Pack");
                RefershPacks(packListContainer);
                var listView = packListContainer.Q<ListView>("packList");
                listView.selectedIndex = listView.itemsSource.Count - 1;
            };

            root.Q<Button>("packs-remove").clickable.clicked += () =>
            {
                var listView = packListContainer.Q<ListView>("packList");
                Localizer.Instance.RemovePack((listView.selectedItem as LocalizationPack).PackPath);
                RefershPacks(packListContainer);
            };

            root.Q<Button>("properties-save").clickable.clicked += () =>
            {
                SelectedPack.SetEditable();
                SelectedPack.Save();
            };


            //root.Add(refreshButton);

            RefershPacks(packListContainer);
        }

        private TextField AddTextField(string text, VisualElement container, EventCallback<ChangeEvent<string>> e,
            int lines = 1)
        {
            var row = new Box();
            row.AddToClassList("properties-item");

            var label = new Label(text);

            var field = new TextField();
            if (lines > 1)
            {
                field.multiline = true;
                field.AddToClassList("multiline");
            }

            field.RegisterCallback(e);

            row.Add(label);
            row.Add(field);

            container.Add(row);

            return field;
        }

        private void OnGUI()
        {
            //var container = rootVisualElement.Query<Box>("container").First();
            //container.StretchToParentSize();
        }

        private void RefershPacks(VisualElement packsContainer)
        {
            if (Localizer.Instance == null)
                Localizer.Initialize(Application.streamingAssetsPath.TrimEnd('/', '\\') + "/Localization", "");

            var packs = Localizer.Instance.Packs;
            var packsList = new List<LocalizationPack>(packs);

            var listView = packsContainer.Q<ListView>("packList");
            int listIndex = 0;
            if (listView != null)
            {
                listIndex = listView.selectedIndex;
                if (listIndex < 0) listIndex = 0;
            }


            packsContainer.Clear();

            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = packs[i].Name;

            const int itemHeight = 16;

            listView = new ListView(packsList, itemHeight, makeItem, bindItem);

            listView.selectionType = SelectionType.Multiple;

            listView.onSelectionChanged += (o) => { OnSelectedPackChanged(listView.selectedItem as LocalizationPack); };

            listView.style.flexGrow = 1.0f;
            listView.name = "packList";

            packsContainer.Add(listView);

            if (listView.itemsSource.Count > listIndex)
            {
                listView.selectedIndex = listIndex;
            }
            else if (listView.itemsSource.Count > 0)
            {
                listView.selectedIndex = listView.itemsSource.Count - 1;
            }
            else
            {
                OnSelectedPackChanged(null);
            }
        }

        private void OnSelectedPackChanged(LocalizationPack pack)
        {
            SelectedPack = pack;

            var propsContainer = rootVisualElement.Q<Box>("properties-container");
            propsContainer.Clear();
            if (SelectedPack != null)
            {
                AddTextField("GUID", propsContainer, (e) => { SelectedPack.Guid = e.newValue; })
                    .SetValueWithoutNotify(SelectedPack.Guid);

                AddTextField("Name", propsContainer, (e) =>
                {
                    var listView = rootVisualElement.Q<ListView>("packList");
                    SelectedPack.Name = e.newValue;
                    listView.Refresh();
                }).SetValueWithoutNotify(SelectedPack.Name);

                AddTextField("Display Name", propsContainer, (e) => { SelectedPack.DisplayName = e.newValue; })
                    .SetValueWithoutNotify(SelectedPack.DisplayName);

                AddTextField("Description", propsContainer, (e) => { SelectedPack.Description = e.newValue; }, 3)
                    .SetValueWithoutNotify(SelectedPack.Description);

                AddTextField("Author", propsContainer, (e) => { SelectedPack.Author = e.newValue; })
                    .SetValueWithoutNotify(SelectedPack.Author);

                AddTextField("Author Description", propsContainer,
                        (e) => { SelectedPack.AuthorDescription = e.newValue; }, 3)
                    .SetValueWithoutNotify(SelectedPack.AuthorDescription);
            }
        }
    }
}