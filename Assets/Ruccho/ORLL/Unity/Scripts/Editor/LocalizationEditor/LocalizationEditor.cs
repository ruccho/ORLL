using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#else
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace ORLL.Unity.Editor
{
    public class LocalizationEditor : EditorWindow
    {
        private List<string> GroupNames = new List<string>();
        private List<string> UnitNames = new List<string>();
        private List<string> KeyNames = new List<string>();

        private string SelectedGroupName;
        private string SelectedUnitName;

#if UNITY_2018
        private VisualElement rootVisualElement => this.GetRootVisualContainer();
#endif

        [MenuItem("Window/ORLL/Localization Editor")]
        public static void ShowExample()
        {
            LocalizationEditor wnd = GetWindow<LocalizationEditor>();
            wnd.titleContent = new GUIContent("Localization Editor");
        }

        public void OnEnable()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (Localizer.Instance == null)
                Localizer.Initialize(Application.streamingAssetsPath.TrimEnd('/', '\\') + "/Localization", "");

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            root.Clear();

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath(
                    "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationEditor/LocalizationEditor.uxml",
                    typeof(VisualTreeAsset)) as VisualTreeAsset;
            VisualElement uxml;
#if UNITY_2018
            uxml = visualTree.CloneTree(null);
#else
                uxml = visualTree.CloneTree();
#endif

            string styleSheetPath =
                "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationEditor/LocalizationEditor_style.uss";
#if UNITY_2018
            uxml.AddStyleSheetPath(styleSheetPath);
#else
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            uxml.styleSheets.Add(ss);
#endif
            root.Add(uxml);

            root.Q<Button>("group_add_button").clickable.clicked += () =>
            {
                var newName = root.Q<TextField>("group_add_name").value;
                if (!GroupNames.Contains(newName))
                {
                    GroupNames.Add(newName);
                }

                root.Q<ListView>("groupList").Refresh();
            };

            root.Q<Button>("unit_add_button").clickable.clicked += () =>
            {
                var newName = root.Q<TextField>("unit_add_name").value;
                if (!UnitNames.Contains(newName))
                {
                    UnitNames.Add(newName);
                }

                root.Q<ListView>("unitList").Refresh();
            };

            root.Q<Button>("key_add_button").clickable.clicked += () =>
            {
                var newName = root.Q<TextField>("key_add_name").value;
                var keysContainer = rootVisualElement.Q<Box>("keys");

                keysContainer.Add(new Label(newName));
                var element = new LocalizedTextField(SelectedGroupName + "/" + SelectedUnitName, newName);
                keysContainer.Add(element);
            };

#if UNITY_2018
            root.Q<ScrollView>("keys-scroll").stretchContentWidth = true;

#endif


            UpdateGroupList();

            var groupList = root.Q<ListView>("groupList");
            var groupListParent = groupList.parent;
            groupListParent.Remove(groupList);

            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = GroupNames[i];

            const int itemHeight = 16;

            groupList = new ListView(GroupNames, itemHeight, makeItem, bindItem);

            groupList.selectionType = SelectionType.Single;

            groupList.onSelectionChanged += (o) =>
            {
                SelectedGroupName = groupList.selectedItem as string;
                UpdateUnitList();
            };

            groupList.style.flexGrow = 1.0f;
            groupList.name = "groupList";

            groupListParent.Add(groupList);


            var refreshButton = new Button(() => { InitializeUI(); });

            refreshButton.text = "Refresh";
            refreshButton.name = "refresh-button";

            styleSheetPath =
                "Assets/Ruccho/ORLL/Unity/Scripts/Editor/LocalizationManager/LocalizationManager_style.uss";
#if UNITY_2018
            refreshButton.AddStyleSheetPath(styleSheetPath);
#else
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            refreshButton.styleSheets.Add(ss);
#endif

            root.Add(refreshButton);

            root.Q<Button>("group_export_csv").clickable.clicked += () => { ExportToCsv(); };
            root.Q<Button>("group_import_csv").clickable.clicked += () => { ImportFromCsv(); };
        }

        private void ExportToCsv()
        {
            if (string.IsNullOrEmpty(SelectedGroupName))
            {
                EditorUtility.DisplayDialog("", "Select 1 group to export.", "OK");
                return;
            }

            string groupName = SelectedGroupName;

            StringBuilder csv = new StringBuilder();

            string header = "\"Path\"";

            foreach (var pack in Localizer.Instance.Packs)
            {
                header += ",\"" + pack.Name + "\"";
            }

            csv.AppendLine(header);

            foreach (var unitName in UnitNames)
            {
                SelectedUnitName = unitName;
                UpdateKeys();
                foreach (var key in KeyNames)
                {
                    csv.Append($"\"{groupName}/{unitName}/{key}\"");

                    foreach (var pack in Localizer.Instance.Packs)
                    {
                        csv.Append(",\"");
                        var h = pack.RequestUnitHandle(groupName, unitName);
                        csv.Append(h.GetText(key));
                        h.Dispose();
                        csv.Append("\"");
                    }

                    csv.AppendLine();
                }
            }

            string s = EditorUtility.SaveFilePanel("Export csv...", "", groupName, "csv");

            if (!string.IsNullOrEmpty(s))
            {
                System.IO.File.WriteAllText(s, csv.ToString(), Encoding.GetEncoding("Shift_JIS"));
            }
        }

        private void ImportFromCsv()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Import csv...", "", new string[] { "CSV", "csv", "All Files", "*" });

            if (!EditorUtility.DisplayDialog("Import CSV", "All packs included in the CSV will be possibly modified. If you want to prevent unexpected modifications, remove non-target columns in csv before you import csv. ", "Continue", "Cancel"))
            {
                return;
            }

            string csv = System.IO.File.ReadAllText(path, Encoding.GetEncoding("Shift_JIS"));

            csv = csv.Replace("\r\n", "\n");

            int c = 0;
            int lineCount = 0;
            bool isInLiteral = false;
            while(c < csv.Length)
            {
                int index = csv.IndexOfAny(new char[] { '\n', '\"' }, c);
                if (index == -1) break;
                if(csv[index] == '\"')
                {
                    isInLiteral = !isInLiteral;
                }else if(!isInLiteral && csv[index] == '\n')
                {
                    lineCount++;
                }
                c = index + 1;
            }

            string[] lines = new string[lineCount];
            isInLiteral = false;
            c = 0;
            int lastReturn = -1;
            int l = 0;
            while (c < csv.Length)
            {
                int index = csv.IndexOfAny(new char[] { '\n', '\"' }, c);
                if (index == -1) break;
                if (csv[index] == '\"')
                {
                    isInLiteral = !isInLiteral;
                }
                else if (!isInLiteral && csv[index] == '\n')
                {
                    lines[l] = csv.Substring(lastReturn + 1, index - lastReturn - 1);
                    l++;
                    lastReturn = index;
                        
                }
                c = index + 1;
            }

            string[][] items = new string[lines.Length - 1][];

            LocalizationUnitHandle[] handleCache = new LocalizationUnitHandle[lines.Length - 1];

            string[] header = ParseCsvLine(lines[0]);

            for (int i = 1; i < lines.Length; i++)
            {
                items[i - 1] = ParseCsvLine(lines[i]);
            }

            LocalizationPack[] packs = new LocalizationPack[header.Length - 1];
            

            for (int i = 1; i < header.Length; i++)
            {
                var pack = Localizer.Instance.Packs.FirstOrDefault(_ => _.Name == header[i]);
                packs[i - 1] = pack;
            }

            foreach (var item in items)
            {
                if (item.Length <= 1) continue;

                string keyPath = item[0];
                string[] keyPathSplitted = keyPath.Split('/');
                if (keyPathSplitted.Length != 3) continue;

                string group = keyPathSplitted[0];
                string unit = keyPathSplitted[1];
                string key = keyPathSplitted[2];

                for (int packIndex = 0; packIndex < packs.Length; packIndex++)
                {
                    if (packs[packIndex] == null) continue;



                    var handle = packs[packIndex].RequestUnitHandle(group, unit, true);
                    string text;
                    if (packIndex + 1 >= item.Length)
                    {
                        text = "";
                    }
                    else
                    {
                        text = item[packIndex + 1];
                    }
                    if (!handle.ContainsKey(key))
                    {
                        handle.AddKey(key);
                    }
                    handle.SetText(key, text);

                    var old = handleCache[packIndex];

                    if (old != null && old.UnitName != handle.UnitName) old.SaveUnit();

                    handleCache[packIndex] = handle;
                    if (old != null) old.Dispose();

                }

            }

            for (int packIndex = 0; packIndex < packs.Length; packIndex++)
            {
                handleCache[packIndex]?.SaveUnit();
                handleCache[packIndex]?.Dispose();
            }

        }

        private string[] ParseCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return new string[] { "" };

            List<string> result = new List<string>();
            int i = 0;
            bool isInLiteral = false;
            int start = 0;

            string temp = "";

            char[] detect = { ',', '\"' };
            while (i < line.Length)
            {
                int p = line.IndexOfAny(detect, i);

                if (p == -1)
                {
                    result.Add(line.Substring(start));
                    break;
                }
                else if (line[p] == ',')
                {
                    if (isInLiteral)
                    {
                        //ignore comma
                        i = p + 1;
                    }
                    else
                    {
                        temp += line.Substring(start, p - start);
                        result.Add(temp);
                        temp = "";

                        start = p + 1;
                        i = p + 1;
                    }

                }
                else if (line[p] == '\"')
                {
                    if (isInLiteral)
                    {
                        if (p + 1 < line.Length && line[p + 1] == '\"')
                        {
                            //escape for "
                            temp += line.Substring(start, p - start) + "\"";
                            start = p + 2;
                            i = p + 2;
                        }
                        else if (p + 1 >= line.Length || (p + 1 < line.Length && line[p + 1] == ','))
                        {
                            //valid close
                            temp += line.Substring(start, p - start);
                            start = p + 2;
                            i = p + 2;
                            isInLiteral = false;


                            result.Add(temp);
                            temp = "";
                        }
                        else
                        {
                            //invalid close to ignore
                            i = p + 1;
                        }
                    }
                    else
                    {
                        isInLiteral = true;
                        i = p + 1;
                        start = p + 1;
                    }
                }
            }
            return result.ToArray();
        }


        private void UpdateGroupList()
        {
            GroupNames.Clear();
            foreach (var pack in Localizer.Instance.Packs)
            {
                foreach (var g in pack.GroupNames)
                {
                    if (!GroupNames.Contains(g))
                    {
                        GroupNames.Add(g);
                    }
                }
            }
        }

        private void UpdateUnitList()
        {
            UnitNames.Clear();
            foreach (var pack in Localizer.Instance.Packs)
            {
                var group = pack.Groups.FirstOrDefault(g => g.Name == SelectedGroupName);

                if (group != null)
                {
                    var units = group.GetUnitNames();
                    foreach (var unit in units)
                    {
                        if (!UnitNames.Contains(unit))
                        {
                            UnitNames.Add(unit);
                        }
                    }
                }
            }

            var unitList = rootVisualElement.Q<ListView>("unitList");
            var unitListParent = unitList.parent;
            unitListParent.Remove(unitList);

            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = UnitNames[i];

            const int itemHeight = 16;

            unitList = new ListView(UnitNames, itemHeight, makeItem, bindItem);

            unitList.selectionType = SelectionType.Single;

            unitList.onSelectionChanged += (o) =>
            {
                SelectedUnitName = unitList.selectedItem as string;
                UpdateKeys();
            };

            unitList.style.flexGrow = 1.0f;
            unitList.name = "unitList";

            unitListParent.Add(unitList);
        }

        private void UpdateKeys()
        {
            var keysContainer = rootVisualElement.Q<Box>("keys");

            keysContainer.Clear();

            KeyNames.Clear();

            foreach (var pack in Localizer.Instance.Packs)
            {
                var group = pack.Groups.FirstOrDefault(g => g.Name == SelectedGroupName);

                if (group != null)
                {
                    var h = group.RequestUnitHandle(SelectedUnitName);

                    if (!h.IsEmpty())
                    {
                        var keysInUnit = h.GetKeys();
                        if (keysInUnit != null)
                        {
                            foreach (var key in keysInUnit)
                            {
                                if (!KeyNames.Contains(key))
                                {
                                    keysContainer.Add(new Label(key));

                                    KeyNames.Add(key);
                                    var element = new LocalizedTextField(SelectedGroupName + "/" + SelectedUnitName,
                                        key);
                                    keysContainer.Add(element);
                                }
                            }
                        }
                    }

                    h.Dispose();
                }
            }
        }


        private void OnGUI()
        {
            var r = rootVisualElement.Q<Box>("root");
            if (r != null && (r.style.width != position.width || r.style.height != position.height))
            {
                r.style.width = position.width;
                r.style.height = position.height;
            }

        }
    }
}