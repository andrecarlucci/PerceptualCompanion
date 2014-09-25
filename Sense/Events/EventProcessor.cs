using System;
using System.Collections.Generic;
using System.Linq;

namespace Sense.Events {
    public class EventProcessor {

        private Dictionary<string, bool> _flagsStates = new Dictionary<string, bool>();
        private Dictionary<string, SimpleTrigger> _triggers = new Dictionary<string, SimpleTrigger>(); 

        public void Flag(params string[] flags) {
            foreach (var flag in flags) {
                _flagsStates[flag] = true;                
            }
        }

        public void UnFlag(params string[] flags) {
            foreach (var flag in flags) {
                _flagsStates[flag] = false;
            }           
        }

        public void FlagOrUnflag(Func<bool> test, string flag) {
            if (test.Invoke()) {
                Flag(flag);
            }
            else {
                UnFlag(flag);
            }
        }

        public SimpleTrigger CreateTrigger(params string[] flags) {
            var trigger = new SimpleTrigger();
            _triggers[EncodeKey(flags)] = trigger;
            return trigger;
        }

        private string EncodeKey(params string[] flags) {
            return String.Join("|", flags.Select(x => x.ToUpper()));
        }
    }
}