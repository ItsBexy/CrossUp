using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;

namespace CrossUp;

public static unsafe partial class NodeEdit
{
    public struct NodeProps
    {
        public bool? Visible { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Scale { get; set; }
        public ushort? Width { get; set; }
        public ushort? Height { get; set; }
        public Vector3? Color { get; set; }
        public float? Alpha { get; set; }
        public int? OriginX { get; set; }
        public int? OriginY { get; set; }
    }
    public static void SetVis(AtkResNode* node,bool show)
    {
        if (show)
        {
            node->Flags |= 0x10;
        } else
        {
            node->Flags &= ~0x10;
        }

        node->Flags_2 |= 0xD;
        return;
    }
    public static void SetPos(AtkResNode* node,float x=0F,float y=0F)
    {
        node->X = x;
        node->Y = y;
        node->Flags_2 |= 0xD;
        return;
    }
    public static void SetScale(AtkResNode* node, float scale)
    {
        node->ScaleX = scale;
        node->ScaleY = scale;
        node->Flags_2 |= 0xD;
        return;
    }

    public static void SetOrigin(AtkResNode* node, int x, int y)
    {
        node->OriginX = x;
        node->OriginY = y;
        node->Flags_2 |= 0xD;
        return;
    }

    public static void SetSize(AtkResNode* node, ushort w, ushort h)
    {
        node->Width = w;
        node->Height = h;
        node->Flags_2 |= 0xD;
        return;
    }
    public static void SetColor(AtkResNode* node, Vector3 color)
    {
        node->Color.R = (byte)(color.X * 255f);
        node->Color.G = (byte)(color.Y * 255f);
        node->Color.B = (byte)(color.Z * 255f);
        node->Flags_2 |= 0xD;
        return;
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
        return;
    }
    public static void SetAlpha(AtkResNode* node, float a)
    {
        node->Color.A = (byte)(a * 255f);
        node->Flags_2 |= 0xD;
        return;
    }
    public static void SetVarious(AtkResNode* node, NodeProps props)
    {
        if (props.X != null) { node->X = (float)props.X; }
        if (props.Y != null) { node->Y = (float)props.Y; }
        if (props.Width != null) { node->Width = (ushort)props.Width; }
        if (props.Height != null) { node->Height = (ushort)props.Height; }
        if (props.Scale != null) { SetScale(node, (float)props.Scale); }
        if (props.Visible != null) { SetVis(node, (bool)props.Visible); }
        if (props.Color != null) { SetColor(node, (Vector3)props.Color); }
        if (props.Alpha != null) { SetAlpha(node, (float)props.Alpha); }
        if (props.OriginX != null && props.OriginY != null) { SetOrigin(node, (int)props.OriginX, (int)props.OriginY); }
        node->Flags_2 |= 0xD;
        return;
    }



}