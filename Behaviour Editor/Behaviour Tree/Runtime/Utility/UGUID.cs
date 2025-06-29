using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    /// <summary> Serializable Unity GUID </summary>
    [StructLayout(LayoutKind.Explicit), Serializable]
    public struct UGUID : IComparable, IComparable<UGUID>, IEquatable<UGUID>
    {
        public UGUID(Guid guid = default)
        {
            this._value0 = 0;
            this._value1 = 0;
            this._value2 = 0;
            this._value3 = 0;
            this.guid = guid;
        }

        [NonSerialized, FieldOffset(0)]
        public Guid guid;

        [SerializeField, FieldOffset(0)]
        private uint _value0;

        [SerializeField, FieldOffset(4)]
        private uint _value1;

        [SerializeField, FieldOffset(8)]
        private uint _value2;

        [SerializeField, FieldOffset(12)]
        private uint _value3;



        public static UGUID Create()
        {
            UGUID uguid = new UGUID();
            uguid.guid = Guid.NewGuid();
            return uguid;
        }


        public override bool Equals(object other)
        {
            if (other is null || other is not UGUID uguid)
            {
                return false;
            }

            return this.Equals(uguid);
        }


        public bool Equals(UGUID other)
        {
            return this == other;
        }


        public int CompareTo(object obj)
        {
            if (obj is null)
            {
                return 1;
            }

            return this.CompareTo((UGUID)obj);
        }


        public int CompareTo(UGUID rhs)
        {
            if (this < rhs)
            {
                return -1;
            }

            return this > rhs ? 1 : 0;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(((this._value0 * 397 ^ this._value1) * 397 ^ this._value2) * 397 ^ this._value3);
            }
        }


        public override string ToString()
        {
            return this.guid.ToString();
        }


        public bool IsEmpty()
        {
            return this._value0 == 0U && this._value1 == 0U && this._value2 == 0U && this._value3 == 0U;
        }


        public static bool operator ==(UGUID x, UGUID y)
        {
            return (x._value0 == y._value0) && (x._value1 == y._value1) && (x._value2 == y._value2) && (x._value3 == y._value3);
        }


        public static bool operator !=(UGUID x, UGUID y)
        {
            return (x._value0 != y._value0) || (x._value1 != y._value1) || (x._value2 != y._value2) || (x._value3 != y._value3);
        }


        public static bool operator <(UGUID x, UGUID y)
        {
            if (x._value0 != y._value0)
            {
                return x._value0 < y._value0;
            }

            if (x._value1 != y._value1)
            {
                return x._value1 < y._value1;
            }

            return x._value2 != y._value2 ? x._value2 < y._value2 : x._value3 < y._value3;
        }


        public static bool operator >(UGUID x, UGUID y)
        {
            if (x._value0 != y._value0)
            {
                return x._value0 > y._value0;
            }

            if (x._value1 != y._value1)
            {
                return x._value1 > y._value1;
            }

            return x._value2 != y._value2 ? x._value2 > y._value2 : x._value3 > y._value3;
        }
    }



#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UGUID))]
    public class UGUIDPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.TextField(position, label, ((UGUID)property.boxedValue).ToString());
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}