﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public abstract class CCGeneric : MonoBehaviour {
        public string genericName = "Generic";
        public string [] keys;
        public string [] values;

        public virtual string Name { get { return genericName; } }

        protected IEnumerator RegisterGeneric() {
            while (CrowdControl.instance == null)
                yield return new WaitForSeconds(1.0f);

            CrowdControl.instance.RegisterGeneric(this);
        }

        protected internal abstract void OnTrigger(KeyValuePair<string, string> [] param);

        private void Awake() {
            StartCoroutine(RegisterGeneric());
            values = new string[keys.Length];
        }
    }
}
