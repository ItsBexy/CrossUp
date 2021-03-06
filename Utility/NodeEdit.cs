using System.Numerics;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    /// <summary>A wrapper that points to an AtkResNode*, alongside optional defaults for that node's properties, and chainable methods for editing the node</summary>
    public sealed class NodeWrapper
    {
        public Vector2 DefaultPos;
        public Vector2 DefaultSize;
        public AtkResNode* Node;

        public NodeWrapper()
        {
        }

        public NodeWrapper(AtkResNode* node, Vector2? defaultPos)
        {
            Node = node;
            if (defaultPos !=null) { DefaultPos = (Vector2)defaultPos; }
        }

        public static implicit operator AtkResNode*(NodeWrapper nr) => nr.Node;
        public static implicit operator NodeWrapper(AtkResNode* n) => new() { Node = n };

        /// <summary>A set of properties that can be written to a node all at once</summary>
        public struct PropertySet
        {
            public bool? Visible { get; init; }
            public float? X { get; init; }
            public float? Y { get; init; }
            public float? Scale { get; init; }
            public ushort? Width { get; init; }
            public ushort? Height { get; init; }
            public float? Alpha { get; init; }
            public float? OrigX { get; init; }
            public float? OrigY { get; init; }
            public Vector3? Color { get; init; }
        }

        public int ChildCount() => Node == null ? 0 : Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;

        public NodeWrapper SetVis(bool show)
        {
            if (Node == null) return this;
            if (show) Node->Flags |= 0x10;
            else Node->Flags &= ~0x10;
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper ChildVis(bool show)
        {
            if (Node == null) return this;
            var count = (int)Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;
            for (var i = 0; i < count; i++) ChildNode(i).SetVis(show);
            return this;
        }

        public NodeWrapper SetScale(float scale = 1f)
        {
            if (Node == null) return this;
            Node->ScaleX = scale;
            Node->ScaleY = scale;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeWrapper SetSize(Vector2? size)
        {
            size ??= DefaultSize;
            return SetSize((Vector2)size);
        }

        public NodeWrapper SetSize(Vector2 size) => SetSize((ushort)size.X, (ushort)size.Y);

        public NodeWrapper SetSize(ushort? w, ushort? h)
        {
            w ??= (ushort)DefaultSize.X;
            h ??= (ushort)DefaultSize.Y;
            return SetSize((ushort)w, (ushort)h);
        }
        public NodeWrapper SetSize(ushort w, ushort h)
        {
            if (Node != null)
            {
                Node->SetWidth(w);
                Node->SetHeight(h);
            }
            return this;
        }
        public NodeWrapper SetSize() => SetSize(DefaultSize);

        public NodeWrapper SetColor(Vector3 color)
        {
            if (Node == null) return this;
            Node->Color.R = (byte)(color.X * 255f);
            Node->Color.G = (byte)(color.Y * 255f);
            Node->Color.B = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper SetMultiply(Vector3 color)
        {
            if (Node == null) return this;
            Node->MultiplyRed = (byte)(color.X * 255f);
            Node->MultiplyGreen = (byte)(color.Y * 255f);
            Node->MultiplyBlue = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper SetAlpha(byte a)
        {
            if (Node == null) return this;
            Node->Color.A = a;
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper SetBlend(uint b)
        {
            if (Node != null)
            {
                var nineGrid = Node->GetAsAtkNineGridNode();
                if (nineGrid != null) nineGrid->BlendMode = b;
            }
            return this;
        }

        public NodeWrapper SetProps(PropertySet props)
        {
            if (Node == null) return this;
            if (props.X != null) Node->X = (float)props.X;
            if (props.Y != null) Node->Y = (float)props.Y;
            if (props.Width != null) Node->Width = (ushort)props.Width;
            if (props.Height != null) Node->Height = (ushort)props.Height;
            if (props.Scale != null) SetScale((float)props.Scale);
            if (props.Visible != null) SetVis((bool)props.Visible);
            if (props.Color != null) SetColor((Vector3)props.Color);
            if (props.Alpha != null) SetAlpha((byte)props.Alpha);
            if (props.OrigX != null && props.OrigY != null)
            {
                float x = (int)props.OrigX;
                float y = (int)props.OrigY;
                Node->OriginX = x;
                Node->OriginY = y;
                Node->Flags_2 |= 0xD;
            }
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper SetOrigin(float x, float y)
        {
            if (Node == null) return this;

            Node->OriginX = x;
            Node->OriginY = y;
            Node->Flags_2 |= 0xD;
            return this;
        }

        public NodeWrapper SetPos(Vector2 pos)
        {
            if (Node == null) return this;
            Node->SetPositionFloat(pos.X, pos.Y);
            return this;
        }

        public NodeWrapper SetPos(Vector2? pos)
        {
            pos ??= DefaultSize;
            return SetPos((Vector2)pos);
        }

        public NodeWrapper SetPos(float x, float y)
        {
            if (Node != null) Node->SetPositionFloat(x, y);
            return this;
        }

        public NodeWrapper SetRelativePos(float x = 0f, float y = 0f)
        {
            if (Node != null) Node->SetPositionFloat(DefaultPos.X + x, DefaultPos.Y + y);
            return this;
        }

        public NodeWrapper ChildNode(params int[] path)
        {
            if (Node == null)
            {
                PluginLog.LogWarning($"NodeWrapper is null and has no child nodes \n{new System.Diagnostics.StackTrace()}");
                return this;
            }
            
            var node = Node;
            foreach (var i in path)
            {
                var comp = node->GetAsAtkComponentNode();
                if (comp == null || comp->Component->UldManager.NodeListSize < i)
                {
                    PluginLog.LogWarning($"No Child node found for NodeWrapper at index {i}\n{new System.Diagnostics.StackTrace()}");
                    return this;
                }
                node = comp->Component->UldManager.NodeList[i];
            }
            return node;
        }

        public NodeWrapper SetTextColor(Vector3 color,Vector3 glow)
        {
            if (Node == null) return this;
            var tnode = Node->GetAsAtkTextNode();
            tnode->EdgeColor.R = (byte)(glow.X * 255f);
            tnode->EdgeColor.G = (byte)(glow.Y * 255f);
            tnode->EdgeColor.B = (byte)(glow.Z * 255f);
            tnode->TextColor.R = (byte)(color.X * 255f);
            tnode->TextColor.G = (byte)(color.Y * 255f);
            tnode->TextColor.B = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
            return this;
        }
    }
}