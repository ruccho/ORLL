using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace ORLL
{
    [MessagePackObject]
    public class LocalizationPack
    {
        public LocalizationPack(string guid, string name, string displayName, string description, string author, string authorDescription)
        {
            Guid = guid;
            Name = name;
            DisplayName = displayName;
            Description = description;
            Author = author;
            AuthorDescription = authorDescription;
        }


        /// <summary>
        /// Path of lcpack file.
        /// </summary>
        [IgnoreMember]
        public string PackPath { get; set; }

        [IgnoreMember] public List<LocalizationGroup> Groups { get; set; }

        [IgnoreMember] private bool AllowEditPackFile { get; set; } = false;

        [Key(0)] public string Guid { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(7)] public string Version { get; set; }
        [Key(2)] public string DisplayName { get; set; }
        [Key(3)] public string Description { get; set; }
        [Key(4)] public string Author { get; set; }
        [Key(5)] public string AuthorDescription { get; set; }

        [Key(6)] public List<string> GroupNames { get; set; }

        public void Initialize(string path, bool allowEditPackFile = false)
        {
            PackPath = path;

            AllowEditPackFile = allowEditPackFile;

            if (AllowEditPackFile)
            {
                Save();
            }

            if (GroupNames == null)
            {
                GroupNames = new List<string>();
            }

            if (Groups != null)
            {
                Groups.Clear();
            }
            else
            {
                Groups = new List<LocalizationGroup>();
            }

            Debug.Log(PackPath);
            
            foreach (string groupName in GroupNames)
            {
                var groupDir =  Path.GetDirectoryName(PackPath).TrimEnd('/', '\\') + Path.DirectorySeparatorChar + groupName;

                if(!Directory.Exists(groupDir))
                {
                    Debug.LogWarning($"ORLL: LocalizationPack: Group \"childDir\" was npt found in this pack ({Name}).");
                    continue;
                }

                Groups.Add(new LocalizationGroup(this, PackPath, groupName));
                
            }
        }


        public void Save()
        {
            if (AllowEditPackFile)
            {
                byte[] raw = MessagePackSerializer.Serialize(this, ORLLResolver.Instance);
                Directory.CreateDirectory(Path.GetDirectoryName(PackPath));
                File.WriteAllBytes(PackPath, raw);
            }
            else
            {
                Debug.LogError("ORLL: LocalizationPack: This pack is not allowed to write to disk.");
            }
                
        }

        public void SetEditable()
        {
            AllowEditPackFile = true;
        }

        internal void SwitchPack(LocalizationPack newPack)
        {
            foreach (var group in Groups)
            {
                group.OnCurrentPackChanged(newPack);
            }
        }

        public LocalizationUnitHandle RequestUnitHandle(string path, bool createIfNeeded = false)
        {
            string[] splitted = path.Split('/');
            if (splitted.Length != 2)
            {
                Debug.LogError("ORLL: Localizer: Specified path is invalid. Path look like <group>/<unit>. (" + path + ")");
                return null;
            }

            return RequestUnitHandle(splitted[0], splitted[1], createIfNeeded);
        }

        public LocalizationUnitHandle RequestUnitHandle(string groupName, string unitName, bool createIfNeeded = false)
        {
            var group = Groups.FirstOrDefault(g => g.Name == groupName);
            if(group == null)
            {
                if (createIfNeeded)
                {
                    var g = CreateGroup(groupName);
                    if(g == null)
                    {
                        return LocalizationUnitHandle.Default;
                    }
                    return g.RequestUnitHandle(unitName, createIfNeeded);
                }
                else
                {
                    //Debug.LogWarning($"ORLL: LocalizationPack: Group {groupName} was not found in this pack.");
                    return LocalizationUnitHandle.Default;
                }
            }
            return group.RequestUnitHandle(unitName, createIfNeeded);

        }

        private LocalizationGroup CreateGroup(string name)
        {
            if (!AllowEditPackFile)
            {
                Debug.LogError("ORLL: LocalizationPack: This pack is not allowed to write to disk.");
                return null;
            }
            Debug.Log("Create group " + name);
            var g = new LocalizationGroup(this, PackPath, name, true);

            Groups.Add(g);
            GroupNames.Add(name);
            Save();


            return g;
        }

        
    }
}