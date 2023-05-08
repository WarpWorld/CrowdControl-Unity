using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace WarpWorld.CrowdControl {
    public abstract class CCGeneric : MonoBehaviour {
        [SerializeField]
        protected string genericName = "Generic";
        [SerializeField]
        protected string [] keys;
        [SerializeField]
        protected string [] values;

        public virtual string Name { get { return genericName; } }

        protected IEnumerator RegisterGeneric() {
            while (CrowdControl.instance == null)
                yield return new WaitForSeconds(1.0f);

            CrowdControl.instance.RegisterGeneric(this);
        }

        /// <summary> Ran when new generic information is assigned. </summary>
        protected internal abstract void OnTrigger(KeyValuePair<string, string> [] param);

        private void Awake() {
            StartCoroutine(RegisterGeneric());
            values = new string[keys.Length];
        }

        /// <summary> Applies the received generic message to the generic's keys and values </summary>
        public void Apply(CCMessageGeneric messageGeneric) {
            foreach (KeyValuePair<string, string> keyValue in messageGeneric.parameters) {
                bool applied = false;
                for (int i = 0; i < keys.Length; i++) {
                    if (string.Equals(keys[i], keyValue.Key)) {
                        values[i] = keyValue.Value;
                        applied = true;
                        break;
                    }
                }

                if (!applied) {
                    Array.Resize(ref keys, keys.Length + 1);
                    keys[keys.Length - 1] = keyValue.Key;

                    Array.Resize(ref values, values.Length + 1);
                    values[values.Length - 1] = keyValue.Value;
                }
            }

            OnTrigger(messageGeneric.parameters.ToArray());
        }

        /// <summary> List of all data as key / value pairs </summary>
        public KeyValuePair<string, string> [] Data() {
            KeyValuePair<string, string>[] keyValues = new KeyValuePair<string, string>[keys.Length];

            for (int i = 0; i < keys.Length; i++)  {
                keyValues[i] = new KeyValuePair<string, string>(keys[i], values[i]);
            }

            return keyValues;
        }
    }
}
