using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    public static class RPC {
        public static void Success(CCEffectInstance instance) {
            Send(instance, "success");
        }

        public static void FailTemporarly(CCEffectInstance instance) {
            Send(instance, "failTemporary");
        }

        public static void FailPermanently(CCEffectInstance instance) {
            Send(instance, "failPermanent");
        }

        public static void TimedBegin(CCEffectInstanceTimed instance) {
            Send(instance, "timedBegin");
        }

        public static void TimedPause(CCEffectInstanceTimed instance) {
            Send(instance, "timedPause");
        }

        public static void TimedResume(CCEffectInstanceTimed instance) {
            Send(instance, "timedResume");
        }

        public static void TimedEnd(CCEffectInstanceTimed instance) {
            Send(instance, "timedEnd");
        }

        private static void Send(CCEffectInstance instance, string command) {
            JSONRpc rpc = new JSONRpc(CrowdControl.instance.CurrentUserHash, instance, command);
            CrowdControl.instance.SendJSON(new JSONData("rpc", JsonConvert.SerializeObject(rpc)));
        }
    }
}
