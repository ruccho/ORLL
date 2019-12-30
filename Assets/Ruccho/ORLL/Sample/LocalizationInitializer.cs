using ORLL.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace ORLL.Sample
{
    public class LocalizationInitializer : MonoBehaviour
    {
        void Awake()
        {
            if (Localizer.Instance != null) return;
            Localizer.Initialize(Application.streamingAssetsPath.TrimEnd('/', '\\') + "/Localization", null);
        }

        public void SwitchPack(string guid)
        {
            Localizer.Instance.SwitchPack(Localizer.Instance.Packs.FirstOrDefault(p => p.Guid == guid));
        }
    }
}