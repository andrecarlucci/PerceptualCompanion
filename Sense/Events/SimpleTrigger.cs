using System;

namespace Sense.Events {
    public class SimpleTrigger {
        public event Action Fired;

        public virtual void OnFired() {
            Action handler = Fired;
            if (handler != null) handler();
        }
    }
}