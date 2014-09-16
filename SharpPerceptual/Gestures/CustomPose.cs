﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpPerceptual.Gestures {
    public class CustomPose {
        private Dictionary<int, bool> _flags = new Dictionary<int, bool>();
        private bool _active;

        public event Action<string> Begin;
        public event Action<string> End;

        public string Name { get; set; }

        public CustomPose(string name = "pose") {
            Name = name;
        }

        protected virtual void OnEnd() {
            Action<string> handler = End;
            if (handler != null) handler(Name);
        }

        protected virtual void OnBegin() {
            Action<string> handler = Begin;
            if (handler != null) handler(Name);
        }

        public int AddFlag() {
            int id = _flags.Count;
            _flags.Add(id, false);
            return id;
        }

        public void Flag(int id, bool state) {
            _flags[id] = state;
            Evaluate();
        }

        private void Evaluate() {
            Active = _flags.Values.All(x => x);
            var sb =  new StringBuilder();
            sb.AppendLine("CustomPose: "+Name);
            foreach (var flag in _flags) {
                sb.AppendLine("F: " + flag.Value);
            }
            Debug.WriteLine(sb);
        }

        public bool Active {
            get { return _active; }
            private set {
                if (_active == value) {
                    return;
                }
                _active = value;
                if (_active) {
                    Debug.WriteLine("Custom pose begin: " + Name);
                    OnBegin();
                }
                else {
                    Debug.WriteLine("Custom pose end: " + End);
                    OnEnd();
                }
            }
        }
    }
}