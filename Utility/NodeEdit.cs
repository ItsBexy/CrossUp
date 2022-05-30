using System.Diagnostics.CodeAnalysis;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;

namespace CrossUp;
public sealed unsafe partial class CrossUp
{
    public class NodeRef
    {
        public Vector2 DefaultPos;
        public Vector2 DefaultSize;
        public AtkResNode* Node;

        public static implicit operator AtkResNode*(NodeRef nr) => nr.Node;
        public static implicit operator NodeRef(AtkResNode* n) => new() { Node = n };
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
        public int ChildCount() => Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;
        public NodeRef SetVis(bool show)
        {
            if (show) Node->Flags |= 0x10;
            else Node->Flags &= ~0x10;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef ChildVis(bool show)
        {
            var count = (int)Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;
            for (var i = 0; i < count; i++) ChildNode(i).SetVis(show);
            return this;
        }
        public NodeRef SetScale(float scale = 1f)
        {
            Node->ScaleX = scale;
            Node->ScaleY = scale;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetSize(Vector2? size)
        {
            size ??= DefaultSize;
            return SetSize((Vector2)size);
        }
        public NodeRef SetSize(Vector2 size) => SetSize((ushort)size.X, (ushort)size.Y);
        public NodeRef SetSize(ushort? w, ushort? h)
        {
            w ??= (ushort)DefaultSize.X;
            h ??= (ushort)DefaultSize.Y;
            return SetSize((ushort)w, (ushort)h);
        }
        public NodeRef SetSize(ushort w, ushort h)
        {
            Node->Width = w;
            Node->Height = h;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetSize() => SetSize(DefaultSize);
        public NodeRef SetColor(Vector3 color)
        {
            Node->Color.R = (byte)(color.X * 255f);
            Node->Color.G = (byte)(color.Y * 255f);
            Node->Color.B = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetAlpha(byte a)
        {
            Node->Color.A = a;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetProps(PropertySet props)
        {
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
        public NodeRef SetOrigin(float x, float y)
        {
            Node->OriginX = x;
            Node->OriginY = y;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetPos(Vector2 pos)
        {
            Node->X = pos.X;
            Node->Y = pos.Y;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetPos(Vector2? pos)
        {
            pos ??= DefaultSize;
            return SetPos((Vector2)pos);
        }
        public NodeRef SetPos(float x, float y)
        {
            Node->X = x;
            Node->Y = y;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef SetRelativePos(float x = 0f, float y = 0f)
        {
            Node->X = DefaultPos.X + x;
            Node->Y = DefaultPos.Y + y;
            Node->Flags_2 |= 0xD;
            return this;
        }
        public NodeRef ChildNode(params int[] path)
        {
            var node = Node;
            foreach (var i in path) node = node->GetAsAtkComponentNode()->Component->UldManager.NodeList[i];
            return node;
        }
        public NodeRef SetTextColor(Vector3 color,Vector3 glow)
        {
            var tnode = Node->GetAsAtkTextNode();
            tnode->EdgeColor.R = (byte)(color.X * 255f);
            tnode->EdgeColor.G = (byte)(color.Y * 255f);
            tnode->EdgeColor.B = (byte)(color.Z * 255f);
            tnode->TextColor.R = (byte)(glow.X * 255f);
            tnode->TextColor.G = (byte)(glow.Y * 255f);
            tnode->TextColor.B = (byte)(glow.Z * 255f);
            Node->Flags_2 |= 0xD;
            return this;
        }
    }
}