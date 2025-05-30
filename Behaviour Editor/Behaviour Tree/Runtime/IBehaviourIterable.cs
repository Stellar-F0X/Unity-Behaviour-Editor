using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public interface IBehaviourIterable
    {
        public int childCount { get; }
        
        public List<NodeBase> GetChildren();
    }
}