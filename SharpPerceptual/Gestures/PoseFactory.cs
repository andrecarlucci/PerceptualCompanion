using System;
using System.Collections.Generic;

namespace SharpPerceptual.Gestures {
    public class PoseFactory {
        private List<WhatTrigger> _items = new List<WhatTrigger>(); 

        public PoseFactory Combine(FlexiblePart what, State trigger) {
            _items.Add(new WhatTrigger(what, trigger));
            return this;
        }

        public CustomPose Build() {
            var gesture = new CustomPose();
            foreach (var itemState in _items) {
                var item = itemState.What;
                var state = itemState.Trigger;
                int id = gesture.AddFlag();
                switch (state) {
                    case State.Opened:
                        item.Opened += () => gesture.Flag(id, true);
                        item.Closed += () => gesture.Flag(id, false);
                        break;
                    case State.Closed:
                        item.Closed += () => gesture.Flag(id, true);
                        item.Opened += () => gesture.Flag(id, false);
                        break;
                    case State.Visible:
                        break;
                    case State.NotVisible:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _items.Clear();
            return gesture;
        }

        private class WhatTrigger {
            public FlexiblePart What { get; set; }
            public State Trigger { get; set; }

            public WhatTrigger(FlexiblePart what, State trigger) {
                What = what;
                Trigger = trigger;
            }
        }
    }
}