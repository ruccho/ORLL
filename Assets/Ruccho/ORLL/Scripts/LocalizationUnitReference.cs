using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MessagePack;
using System;
using System.Linq;

namespace ORLL
{
    public class LocalizationUnitReference
    {
        public string UnitName { get; }
        private string UnitPath { get; }

        private List<LocalizationUnitHandle> Handles { get; } = new List<LocalizationUnitHandle>();

        private LocalizationUnit Unit { get; set; }

        public LocalizationGroup Group { get; private set; }

        public bool IsValid
        {
            get
            {
                RefreshUnitReference();
                return Unit != null;
            }
        }

        public LocalizationUnitReference(LocalizationGroup group, string unitPath, bool createIfNeeded = false)
        {
            Group = group;
            UnitPath = unitPath;
            UnitName = Path.GetFileNameWithoutExtension(unitPath);

            if (createIfNeeded && !File.Exists(UnitPath))
            {
                Create();
            }
        }

        public void Create()
        {
            Debug.Log("Create Unit" + UnitName + " to " + UnitPath);
            var u = new LocalizationUnit();
            byte[] raw = MessagePackSerializer.Serialize(u, ORLLResolver.Instance);
            File.WriteAllBytes(UnitPath, raw);
        }

        public bool TryGetText(string key, out string value)
        {
            value = null;
            if (Unit == null || Unit.Values == null) return false;

            if (Unit.Values.ContainsKey(key))
            {
                value = Unit.Values[key];
                return true;
            }
            else
            {
                return false;
            }

        }

        public string GetText(string key, string defaultValue = "")
        {
            if (Unit == null)
            {
                Debug.LogError("ORLL: LocalizationUnitReference: The unit is not loaded.: " + UnitName);
                return defaultValue;
            }
            else
            {
                if (Unit.Values.ContainsKey(key))
                {
                    return Unit.Values[key];
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public void SetText(string key, string value)
        {
            if (Unit == null)
            {
                Debug.LogError("ORLL: LocalizationUnitReference: The unit is not loaded." + UnitName);
                
            }
            else
            {
                if (Unit.Values.ContainsKey(key))
                {
                    Unit.Values[key] = value;
                }
                else
                {
                    
                }
            }
        }

        public LocalizationUnitHandle RequestHandle()
        {
            var handle = new LocalizationUnitHandle();
            RegisterHandle(handle);
            return handle;
        }

        internal void RegisterHandle(LocalizationUnitHandle handle)
        {
            Handles.Add(handle);
            RefreshUnitReference();
            handle.BindReference(this);
        }

        internal void UnbindHandle(LocalizationUnitHandle handle)
        {
            if (Handles.Contains(handle))
            {
                Handles.Remove(handle);
            }
            RefreshUnitReference();
        }

        internal void Migrate(LocalizationGroup newGroup)
        {
            var newReference = newGroup.RequestReference(UnitName);

            int i = 0;
            LocalizationUnitHandle prevHandle = null;
            while (i < Handles.Count)
            {
                prevHandle = Handles[i];

                newReference.RegisterHandle(Handles[i]);

                if (i < Handles.Count && prevHandle == Handles[i])
                {
                    i++;
                }
            }

        }

        private void RefreshUnitReference()
        {
            if (Handles.Count > 0)
            {
                if (Unit == null)
                {
                    if (File.Exists(UnitPath))
                    {
                        byte[] raw = File.ReadAllBytes(UnitPath);
                        var unit = MessagePackSerializer.Deserialize<LocalizationUnit>(raw, ORLLResolver.Instance);
                        Unit = unit;
                        //Debug.Log("ORLL: LocalizationUnitReference: An unit has been loaded from file " + UnitPath);
                    }
                    else
                    {
                        //Debug.LogWarning("ORLL: LocalizationUnitReference: An unit has not been loaded from file " + UnitPath);
                    }
                }
            }
            else
            {
                //Debug.Log($"ORLL: LocalizationUnitReference: An unit has been released because there is no reference. \"{UnitPath}\" (Note: actual instance of the unit might not be released until GC collects it.)");
                Unit = null;
            }
        }

        public bool ContainsKey(string key)
        {
            if (Unit != null)
            {
                return Unit.Values.ContainsKey(key);
            }
            return false;
        }

        public void AddKey(string key)
        {
            if(Unit != null)
            {
                Unit.Values.Add(key, "");
            }
        }

        public void RemoveKey(string key)
        {
            if (Unit != null)
            {
                Unit.Values.Remove(key);
            }
        }

        public void SaveUnit()
        {
            if (Unit == null) return;
            byte[] raw = MessagePackSerializer.Serialize(Unit, ORLLResolver.Instance);
            File.WriteAllBytes(UnitPath, raw);
        }

        public string[] GetKeys()
        {
            if (Unit == null) return null;
            return Unit.Values.Keys.ToArray();
        }


    }
}