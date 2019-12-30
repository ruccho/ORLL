using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;
using System.IO;
using System.Linq;
using Microsoft.Win32.SafeHandles;

namespace ORLL
{
	
	public class LocalizationGroup
	{
        public LocalizationPack Pack { get; private set; }

		private string PackPath { get; set; }
		
		public string Name { get; private set; }

		private string GroupPath
		{
			get { return Path.GetDirectoryName(PackPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + Name; }
		}

        private List<LocalizationUnitReference> References { get; set; } = new List<LocalizationUnitReference>();

        private string[] UnitNames { get; set; }

		public LocalizationGroup(LocalizationPack pack, string packPath, string name, bool allowCreateDirectoryIfNeeded = false)
		{
            Pack = pack;
			PackPath = packPath;
			Name = name;
			if (!Directory.Exists(GroupPath))
			{
				if (allowCreateDirectoryIfNeeded)
				{
					try
					{
						Directory.CreateDirectory(GroupPath);
					}
					catch (IOException e)
					{
						Debug.LogError("ORLL: LocalizationGroup: Failed to create the directory of the group.");
						throw;
					}
				}
				else
				{
					Debug.LogError($"ORLL: LocalizationGroup: Directory of the group \"{name}\" ({GroupPath}) was not found. You can create a directory automatically by specifying allowCreateDirectoryIfNeeded option.");
					throw new ArgumentException("Group was not found.");
				}
			}

            UnitNames = GetUnitNames();

        }

		public string[] GetUnitNames()
		{
			string[] paths = Directory.GetFiles(GroupPath, "*.lcunit");
			string[] result = new string[paths.Length];
			for (int i = 0; i < paths.Length; i++)
			{
				result[i] = Path.GetFileNameWithoutExtension(paths[i]);
			}

			return result;
		}

		public bool ExistsUnit(string unitName)
        {
            UnitNames = GetUnitNames();
            return UnitNames.Contains(unitName);
		}		

        public LocalizationUnitHandle RequestUnitHandle(string unitName, bool createIfNeeded = false)
        {
            var reference = RequestReference(unitName, createIfNeeded);
            if(reference == null)
            {
                return LocalizationUnitHandle.Default;
            }
            return reference.RequestHandle();
        }

        internal LocalizationUnitReference RequestReference(string unitName, bool createIfNeeded = false)
        {
            var ready = References.FirstOrDefault(r => r.UnitName == unitName);                     

            if(ready == null)
            {
                //if (!ExistsUnit(unitName) && !createIfNeeded) return new LocalizationUnitReference(this, GetUnitPath(unitName), false);

                ready = new LocalizationUnitReference(this, GetUnitPath(unitName), createIfNeeded);
                References.Add(ready);
            }else if(!ready.IsValid && createIfNeeded)
            {
                ready.Create();
            }

            return ready;
        }

        private string GetUnitPath(string unitName)
        {
            return GroupPath.TrimEnd('/', '\\') + Path.DirectorySeparatorChar + unitName + ".lcunit";
        }

        public void OnCurrentPackChanged(LocalizationPack newPack)
        {
            var newGroup = newPack.Groups.FirstOrDefault(p => p.Name == this.Name);

            if(newGroup != null)
            {
                foreach(var reference in References)
                {
                    reference.Migrate(newGroup);
                }
            }
        }


		
		
	}
}