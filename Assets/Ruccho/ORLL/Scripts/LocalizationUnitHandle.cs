using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ORLL
{
    /// <summary>
    /// It provides an access to instance of LocalizationUnit and manages a resource.
    /// Call Dispose() to release resources. (actual release will be operated by GC.)
    /// </summary>
    public class LocalizationUnitHandle : IDisposable
    {
        public LocalizationPack Pack => Group.Pack;
        public LocalizationGroup Group =>  Reference.Group;

        public string UnitName => Reference.UnitName;

        /// <summary>
        /// New handle that has no reference of any unit.
        /// </summary>
        public static LocalizationUnitHandle Default => new LocalizationUnitHandle();

        /// <summary>
        /// Returns true if the handle has no reference of actual unit or has already disposed.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool IsEmpty(LocalizationUnitHandle handle)
        {
            return handle.Reference == null;
        }

        /// <summary>
        /// Returns true if the handle has no reference of actual unit or has already disposed.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return IsEmpty(this);
        }

        private LocalizationUnitReference Reference { get; set; }

        private Action<LocalizationUnitHandle> OnLocalizationChanged { get; set; }

        internal void BindReference(LocalizationUnitReference reference)
        {
            if (Reference != null)
            {
                Reference.UnbindHandle(this);
            }
            Reference = reference;
            OnLocalizationChanged?.Invoke(this);
        }

        /// <summary>
        /// It tries to get text. If text is missing, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetText(string key, out string value)
        {
            value = null;
            if (Reference == null) return false;
            return Reference.TryGetText(key, out value);
        }

        public string GetText(string key, string defaultValue = "")
        {
            if (Reference != null)
                return Reference.GetText(key, defaultValue);
            else
                return defaultValue;
        }

        public void SetText(string key, string value)
        {
            if(Reference != null)
            {
                Reference.SetText(key, value);
            }
        }

        /// <summary>
        /// Release resources of the unit that this handle refers.
        /// </summary>
        public void Dispose()
        {
            if (Reference != null)
            {
                Reference.UnbindHandle(this);
            }
            Reference = null;
        }

        public void Register(Action<LocalizationUnitHandle> onLocalizationChanged)
        {
            OnLocalizationChanged = onLocalizationChanged;
            OnLocalizationChanged?.Invoke(this);
        }

        public void Unregister()
        {
            OnLocalizationChanged = null;
        }

        public bool ContainsKey(string key)
        {
            if (Reference != null)
            {
                return Reference.ContainsKey(key);
            }
            return false;
        }

        public void AddKey(string key)
        {
            if(Reference != null)
            {
                Reference.AddKey(key);
            }
        }

        public void RemoveKey(string key)
        {
            if(Reference != null)
            {
                Reference.RemoveKey(key);
            }
        }

        public void SaveUnit()
        {
            if(Reference != null)
            {
                Reference.SaveUnit();
            }
        }

        public string[] GetKeys()
        {
            if (Reference == null) return null;
            return Reference.GetKeys();
        }

    }
}