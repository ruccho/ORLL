using System;
using System.Collections;
using System.Collections.Generic;
using ORLL;
using UnityEngine;


namespace ORLL.Unity
{
    [Serializable]
    public class LocalizedString
    {
        [SerializeField, Multiline] public string defaultText = "";

        [SerializeField] public string path;

        [SerializeField] private bool loadByDefault;

        private LocalizationUnitHandle Handle { get; set; }

        private string Key { get; set; }


        public bool IsPrepared => Handle != null;

        public string Text
        {
            get
            {
                if (!IsPrepared)
                {
                    Debug.LogError("ORLL: LocalizedString: This LocalizedString is not initialized.");
                    return "";
                }

                return Handle.GetText(Key);

            }
        }

        public LocalizedString() { }
        public LocalizedString(string path, string defaultText = "")
        {
            this.path = path;
            this.defaultText = defaultText;
        }

        public void Initialize(Action<string> onLocalizationChanged)
        {
            if (IsPrepared)
            {
                Debug.LogError("LocalizedString is already initialized.");
                return;
            }

            if (Localizer.Instance == null)
            {
                Debug.LogError("ORLL: LocalizedString: Localizer is not initialized.");
            }

            string[] splitted = path.Split('/');

            if (splitted.Length != 3)
            {

                return;
            }

            Handle = Localizer.Instance.RequestUnitHandle(splitted[0] + '/' + splitted[1]);

            if (Handle == null)
            {
                return;
            }

            Key = splitted[2];

            Handle.Register((h) => { Debug.Log("Changed!"); onLocalizationChanged(h.GetText(Key, defaultText)); });
            
        }

        public void Dispose()
        {
            if (!IsPrepared) return;

            Handle.Dispose();
            Handle = null;
        }

        public LocalizedString Clone()
        {
            return new LocalizedString(path, defaultText);
        }
    }
}