using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public interface IBehaviourIterable
    {
        public int childCount { get; }
        
        public IEnumerable<NodeBase> GetChildren();
    }
}