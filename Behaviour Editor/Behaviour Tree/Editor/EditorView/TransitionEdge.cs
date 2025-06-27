using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    // Referenced: https://github.com/FBast/unity-graphtools-fsm/blob/main/Editor/Edges/TransitionEdge.cs
    // Copyright (c) 2021 Original Author
    // Licensed under the MIT License. See LICENSE file in the root for details.
    public class TransitionEdge : Edge
    {
        public TransitionEdge()
        {
            isGhostEdge = !BehaviourSystemEditor.IsLoadingTreeToView;

            generateVisualContent += this.DrawArrow;
        }

        private const float _ARROW_WIDTH = 12f;

        private readonly Color _activeColor = Color.green;


        public bool active
        {
            get;
            set;
        }


        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (base.ContainsPoint(localPoint))
            {
                return true;
            }

            Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
            Vector2 mid = (start + end) / 2;

            return (localPoint - mid).sqrMagnitude <= (_ARROW_WIDTH * _ARROW_WIDTH);
        }


        public override bool UpdateEdgeControl()
        {
            base.UpdateEdgeControl();

            if (PointsAndTangents is null || PointsAndTangents.Length < 4)
            {
                return false;
            }

            // output 또는 input 포트 중 하나라도 유효한 노드가 있어야 함.
            Node outputNode = output?.node;
            Node inputNode = input?.node;

            if (outputNode is null && inputNode is null)
            {
                return false;
            }

            Vector2 from;
            Vector2 to;

            if (isGhostEdge)
            {
                //드래그 중 (고스트 엣지) 인 경우.
                if (outputNode is not null)
                {
                    from = outputNode.GetPosition().center;
                    to = edgeControl.to;
                }
                else
                {
                    from = edgeControl.from;
                    to = inputNode.GetPosition().center;
                }
            }
            else
            {
                // 완성된 엣지인 경우
                if (outputNode != null && inputNode != null)
                {
                    from = outputNode.GetPosition().center;
                    to = inputNode.GetPosition().center;
                }
                else
                {
                    return false;
                }
            }

            Vector2 tangent = Vector2.zero;

            if (isGhostEdge == false && outputNode is not null && inputNode is not null)
            {
                Vector2 dir = (to - from).normalized;
                float distance = Vector2.Distance(from, to);
                Vector2 perpendicular = new Vector2(-dir.y, dir.x);
                float curveStrength = Mathf.Min(5f, distance * 0.3f);
                tangent = perpendicular * curveStrength;
            }

            PointsAndTangents[0] = from;
            PointsAndTangents[1] = from + tangent;
            PointsAndTangents[2] = to + tangent;
            PointsAndTangents[3] = to;

            this.MarkDirtyRepaint();
            return true;
        }


        private void DrawArrow(MeshGenerationContext context)
        {
            Color arrowColor = GetColor();

            Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
            Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
            Vector2 mid = (start + end) * 0.5f;
            Vector2 direction = end - start;

            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            direction.Normalize();

            float distanceFromMid = _ARROW_WIDTH * Mathf.Sqrt(3) / 4;
            float perpendicularLength = _ARROW_WIDTH * 0.5f;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * perpendicularLength;

            var mesh = context.Allocate(3, 3);
            Vertex[] vertices = new Vertex[3];
            ushort[] indices = new ushort[3];

            vertices[0].position = mid + direction * distanceFromMid;
            vertices[1].position = mid - direction * distanceFromMid + perpendicular;
            vertices[2].position = mid - direction * distanceFromMid - perpendicular;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].position += Vector3.forward * Vertex.nearZ;
                vertices[i].tint = arrowColor;
                indices[i] = (ushort)i;
            }

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }


        private Color GetColor()
        {
            if (isGhostEdge)
            {
                return ghostColor;
            }

            if (selected)
            {
                return selectedColor;
            }

            if (active)
            {
                return _activeColor;
            }

            if (output != null)
            {
                return output.portColor;
            }

            return defaultColor;
        }


        public void UpdateEdgeColor()
        {
            Color color = this.GetColor();
            edgeControl.inputColor = color;
            edgeControl.outputColor = color;
            this.MarkDirtyRepaint();
        }
    }
}