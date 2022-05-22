using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;

namespace CrossUp;

public static unsafe class NodeEdit
{
    public struct PropertySet
    {
        public bool? Visible { get; init; }
        public float? X { get; init; }
        public float? Y { get; init; }
        public float? Scale { get; init; }
        public ushort? Width { get; init; }
        public ushort? Height { get; init; }
        public float? Alpha { get; init; }
        public int? OrigX { get; init; }
        public int? OrigY { get; init; }
        public Vector3? Color { get; init; }
    }
    public static void SetVis(AtkResNode* node,bool show)
    {
        if (show) node->Flags |=  0x10;
        else      node->Flags &= ~0x10;

        node->Flags_2 |= 0xD;
    }
    public static void SetPos(AtkResNode* node,float x=0F,float y=0F)
    {
        node->X = x;
        node->Y = y;
        node->Flags_2 |= 0xD;
    }
    public static void SetScale(AtkResNode* node, float scale)
    {
        node->ScaleX = scale;
        node->ScaleY = scale;
        node->Flags_2 |= 0xD;
    }
    public static void SetOrigin(AtkResNode* node, int x, int y)
    {
        node->OriginX = x;
        node->OriginY = y;
        node->Flags_2 |= 0xD;
    }
    public static void SetSize(AtkResNode* node, ushort w, ushort h)
    {
        node->Width = w;
        node->Height = h;
        node->Flags_2 |= 0xD;
    }
    public static void SetSize(AtkResNode* node, Vector2 size) => SetSize(node, (ushort)size.X, (ushort)size.Y);
    public static void SetColor(AtkResNode* node, Vector3 color)
    {
        node->Color.R = (byte)(color.X * 255f);
        node->Color.G = (byte)(color.Y * 255f);
        node->Color.B = (byte)(color.Z * 255f);
        node->Flags_2 |= 0xD;
    }
    public static void SetTextColor(AtkResNode* node, Vector3 color, Vector3 glow)
    {
        var tnode = node->GetAsAtkTextNode();
        tnode->EdgeColor.R = (byte)(color.X * 255f);
        tnode->EdgeColor.G = (byte)(color.Y * 255f);
        tnode->EdgeColor.B = (byte)(color.Z * 255f);
        tnode->TextColor.R = (byte)(glow.X * 255f);
        tnode->TextColor.G = (byte)(glow.Y * 255f);
        tnode->TextColor.B = (byte)(glow.Z * 255f);
        node->Flags_2 |= 0xD;
    }
    public static void SetAlpha(AtkResNode* node, float a)
    {
        node->Color.A = (byte)(a * 255f);
        node->Flags_2 |= 0xD;
    }
    public static void SetVarious(AtkResNode* node, PropertySet props)
    {
        if (props.X != null) { node->X = (float)props.X; }
        if (props.Y != null) { node->Y = (float)props.Y; }
        if (props.Width != null) { node->Width = (ushort)props.Width; }
        if (props.Height != null) { node->Height = (ushort)props.Height; }
        if (props.Scale != null) { SetScale(node, (float)props.Scale); }
        if (props.Visible != null) { SetVis(node, (bool)props.Visible); }
        if (props.Color != null) { SetColor(node, (Vector3)props.Color); }
        if (props.Alpha != null) { SetAlpha(node, (float)props.Alpha); }
        if (props.OrigX != null && props.OrigY != null) { SetOrigin(node, (int)props.OrigX, (int)props.OrigY); }
        node->Flags_2 |= 0xD;
    }

         // overloads for stuff from BarReference.cs
    public static void SetVis(CrossUp.NodeRef nodeRef, bool show) => SetVis(nodeRef.Node, show);
    public static void SetPos(CrossUp.NodeRef nodeRef, float? x = null, float? y = null)
    {
        x ??= nodeRef.Position.X;
        y ??= nodeRef.Position.Y;
        SetPos(nodeRef.Node, (float)x, (float)y);
    }
    public static void RelativePos(CrossUp.NodeRef nodeRef, float x, float y) => SetPos(nodeRef.Node, nodeRef.Position.X + x, nodeRef.Position.Y + y);
    public static void SetSize(CrossUp.NodeRef nodeRef, ushort? width = null, ushort? height = null)
    {
        width ??= (ushort)nodeRef.Size.X;
        height ??= (ushort)nodeRef.Size.Y;
        SetSize(nodeRef.Node, (ushort)width, (ushort)height);
    }
    public static void SetSize(CrossUp.NodeRef nodeRef, Vector2 size) => SetSize(nodeRef.Node, (ushort)size.X, (ushort)size.Y);
    public static void SetColor(CrossUp.NodeRef nodeRef, Vector3 color) => SetColor(nodeRef.Node, color);
    public static void SetAlpha(CrossUp.NodeRef nodeRef, float a) => SetAlpha(nodeRef.Node, a);
    public static void SetVarious(CrossUp.NodeRef nodeRef, PropertySet props) => SetVarious(nodeRef.Node, props);
}