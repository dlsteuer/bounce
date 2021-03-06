using System.Collections.Generic;

namespace LegacyBounce.Framework {
    public class RemoteBounce {
        private ITargetsParser TargetsParser;
        private List<object> RemoteTargets;

        public RemoteBounce(ITargetsParser targetsParser) {
            TargetsParser = targetsParser;
            RemoteTargets = new List<object>();
        }

        public RemoteBounce() : this(new TargetsParser()) { }

        public object WithRemoteTargets(object targetsObject) {
            var targets = TargetsParser.ParseTargetsFromObject(targetsObject);

            foreach (var remoteTarget in RemoteTargets) {
                foreach (KeyValuePair<string, IObsoleteTask> target in TargetsParser.ParseTargetsFromObject(remoteTarget)) {
                    targets.Add(target.Key, target.Value);
                }
            }

            return targets;
        }

        public RemoteBounceArguments ArgumentsForTargets(object targets)
        {
            RemoteTargets.Add(targets);
            return new RemoteBounceArguments { Targets = TargetsParser.ParseTargetsFromObject(targets).Keys };
        }
    }
}