using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class IntProperty : BlackboardProperty<int>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.ComparisonOperators; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            var property = other as BlackboardProperty<int>;
            return this.value.CompareTo(property!.value);
        }
    }

    [Serializable]
    public class FloatProperty : BlackboardProperty<float>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.ComparisonOperators; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            var property = other as BlackboardProperty<float>;

            if (Mathf.Approximately(this.value, property!.value))
            {
                return 0;
            }

            return this.value > property.value ? 1 : -1;
        }
    }

    [Serializable]
    public class BoolProperty : BlackboardProperty<bool>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            if (other is BlackboardProperty<bool> property)
            {
                return this.value == property.value ? 0 : -1;
            }

            return -1;
        }
    }

    [Serializable]
    public class TriggerProperty : BlackboardProperty<bool>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Trigger; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            if (this.value)
            {
                this.value = false;
                return 0;
            }

            return -1;
        }
    }

    [Serializable]
    public class Vector3Property : BlackboardProperty<Vector3>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal | EConditionType.NotEqual; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            BlackboardProperty<Vector3> otherValue = other as BlackboardProperty<Vector3>;

            if (Mathf.Approximately(this.value.sqrMagnitude, otherValue.value.sqrMagnitude))
            {
                return 0;
            }

            return -1;
        }
    }

    [Serializable]
    public class Vector2Property : BlackboardProperty<Vector2>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal | EConditionType.NotEqual; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            BlackboardProperty<Vector2> otherValue = other as BlackboardProperty<Vector2>;

            if (Mathf.Approximately(this.value.sqrMagnitude, otherValue.value.sqrMagnitude))
            {
                return 0;
            }

            return -1;
        }
    }

    [Serializable]
    public class QuaternionProperty : BlackboardProperty<Quaternion>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal | EConditionType.NotEqual; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            BlackboardProperty<Quaternion> otherValue = other as BlackboardProperty<Quaternion>;

            if (Mathf.Approximately(this.value.eulerAngles.sqrMagnitude, otherValue.value.eulerAngles.sqrMagnitude))
            {
                return 0;
            }

            return -1;
        }
    }

    [Serializable]
    public class LayerMaskProperty : BlackboardProperty<LayerMask>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal | EConditionType.NotEqual; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            if (other is BlackboardProperty<LayerMask> layerMask)
            {
                return (this.value & layerMask.value) > 0 ? 0 : -1;
            }

            return -1;
        }
    }

    [Serializable]
    public abstract class GenericListProperty<T> : BlackboardProperty<List<T>> { }
    
    [Serializable]
    public class NavMeshAgentProperty : BlackboardProperty<NavMeshAgent> { }
    
    [Serializable]
    public class TransformProperty : BlackboardProperty<Transform> { }

    [Serializable]
    public class StringProperty : BlackboardProperty<string> { }

    [Serializable]
    public class RigidbodyProperty : BlackboardProperty<Rigidbody> { }

    [Serializable]
    public class AnimatorProperty : BlackboardProperty<Animator> { }

    [Serializable]
    public class ColliderProperty : BlackboardProperty<Collider> { }

    [Serializable]
    public class GameObjectProperty : BlackboardProperty<GameObject> { }

    [Serializable]
    public class CharacterControllerProperty : BlackboardProperty<CharacterController> { }
}