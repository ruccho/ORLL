using ORLL.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ORLL.Unity
{
    public class LocalizedTextAttacher : MonoBehaviour
    {
        [SerializeField]
        private LocalizedString Text;
        [SerializeField]
        private Text Target;

        private void Start()
        {
            if (!Target) Target = GetComponent<Text>();
            if (Target) Text.defaultText = Target.text;
            Text.Initialize((s) =>
            {
                if (!Target) Target = GetComponent<Text>();
                if (Target)
                    Target.text = s;
            });
        }

        private void OnDestroy()
        {
            Text.Dispose();
        }
    }
}