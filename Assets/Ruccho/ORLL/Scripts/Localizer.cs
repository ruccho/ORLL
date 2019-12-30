using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace ORLL
{
    public class Localizer
    {

        public static Localizer Instance
        {
            get;
            private set;
        }

        public LocalizationPack CurrentPack { get; private set; }
        private List<LocalizationPack> packs;

        public IReadOnlyList<LocalizationPack> Packs
        {
            get { return packs; }
        }

        public string RootPath { get; private set; }

        /// <summary>
        /// Initialize localizer singleton instance.
        /// </summary>
        /// <param name="path">path of directory containing localization packs.</param>
        /// <returns></returns>
        public static Localizer Initialize(string path, string defaultPackGuid)
        {
            var instance = new Localizer();
#if UNITY_EDITOR
            instance.LoadPacks(path, defaultPackGuid, true);
#else
            instance.LoadPacks(path, defaultPackGuid);
#endif
            Instance = instance;
            return Instance;
        }

        internal void SetPacksEditable(string path, string defaultPackGuid)
        {
            foreach (var pack in Packs)
            {
                pack.SetEditable();
            }
        }

        private void LoadPacks(string path, string defaultPackGuid, bool allowEditDisk = false)
        {
            RootPath = path;
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
                return;
            }

            if (packs != null)
                packs.Clear();
            else
            {
                packs = new List<LocalizationPack>();
            }

            try
            {

                string[] packDirs = Directory.GetDirectories(path);
                foreach (var packDir in packDirs)
                {
                    string[] packFiles = Directory.GetFiles(packDir, "*.lcpack");

                    foreach (var packFile in packFiles)
                    {
                        byte[] packRaw = File.ReadAllBytes(packFile);
                        var pack = MessagePackSerializer.Deserialize<LocalizationPack>(packRaw, ORLLResolver.Instance);
                        pack.Initialize(packFile, allowEditDisk);
                        packs.Add(pack);
                    }
                }
            }
            catch (IOException e)
            {
                //Debug.LogError(e);(e);
                throw;
            }

            //Check for duplicate
            List<LocalizationPack> packsToRemove = new List<LocalizationPack>();

            for (int i = 0; i < packs.Count; i++)
            {
                string guid = packs[i].Guid;
                for (int j = 0; j < i; j++)
                {
                    if (guid == packs[j].Guid)
                    {
                        Debug.LogError($"ORLL Localizer: GUID of localization pack \"{packs[i].Name}\" ({packs[i].PackPath}) conflicts with \"{packs[j].Name}\" ({packs[j].PackPath}). \"{packs[i].Name}\" was not loaded.");
                        packsToRemove.Add(packs[i]);
                        break;
                    }
                }
            }

            Debug.Log($"ORLL Localizer: {packs.Count.ToString()} packs were loaded.");

            foreach (var packToRemove in packsToRemove)
            {
                packs.Remove(packToRemove);
            }

            //Find default pack
            var defaultPack = packs.FirstOrDefault(pack => pack.Guid == defaultPackGuid);

            if (defaultPack == null && packs.Count > 0)
            {
                defaultPack = packs[0];
            }

            if (defaultPack != null)
            {
                Debug.Log($"ORLL Localizer: {defaultPack.Name} was set as default pack.");
            }

            CurrentPack = defaultPack;
        }

        public LocalizationPack CreatePack(string folderName, string name)
        {
            var pack = new LocalizationPack(Guid.NewGuid().ToString(), name, name, "", "", "");
            var folderPath = Path.GetDirectoryName(RootPath.TrimEnd('/', '\\') + Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + folderName + pack.Guid;
            var packPath = folderPath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar + folderName + ".lcpack";
            Directory.CreateDirectory(folderPath);
            pack.Initialize(packPath, true);
            packs.Add(pack);
            return pack;
        }

        public void RemovePack(LocalizationPack pack)
        {
            if (!packs.Contains(pack)) return;
            string packPath = pack.PackPath;
            Debug.Log(Path.GetDirectoryName(packPath));
            Directory.Delete(Path.GetDirectoryName(pack.PackPath), true);
            packs.Remove(pack);
        }

        public void RemovePack(string packPath)
        {
            var pack = packs.FirstOrDefault(p => p.PackPath == packPath);
            RemovePack(pack);
        }

        public void SwitchPack(LocalizationPack newPack)
        {
            if (!Packs.Contains(newPack))
            {
                Debug.LogError($"ORLL: Localizer: Specified pack \"{newPack.Name}\" is not loaded on current localizer instance.");
                return;
            }

            var old = CurrentPack;

            if (old == newPack) return;

            CurrentPack = newPack;

            old.SwitchPack(newPack);

            Debug.Log($"ORLL: Localizer: \"{newPack.Name}\" was set as default pack.");

        }

        public LocalizationUnitHandle RequestUnitHandle(string path)
        {
            if (CurrentPack == null) return LocalizationUnitHandle.Default;
            return CurrentPack.RequestUnitHandle(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group">The name of the group.</param>
        /// <param name="unit">The name of the unit.</param>
        /// <returns>Matched handle. If no handles are matched, it returns empty handle. </returns>
        public LocalizationUnitHandle RequestUnitHandle(string group, string unit)
        {
            if (CurrentPack == null) return LocalizationUnitHandle.Default;
            return CurrentPack.RequestUnitHandle(group, unit);
        }

    }
}