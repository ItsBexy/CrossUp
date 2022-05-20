using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;
using Dalamud.Logging;

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
        if (show)
        {
            node->Flags |= 0x10;
        } else
        {
            node->Flags &= ~0x10;
        }

        node->Flags_2 |= 0xD;
    }

    private static void SetPos(AtkResNode* node,float x=0F,float y=0F)
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

    private static void SetOrigin(AtkResNode* node, int x, int y)
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

    private static void SetAlpha(AtkResNode* node, float a)
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

    public class ByLookup // grab and edit things using name and default props from Reference.cs
    {
        public static void RelativePos(CrossUp.NodeRef props, float offX, float offY)
        {
            SetPos(props.UnitBase->UldManager.NodeList[props.Id], props.Position.X + offX, props.Position.Y + offY);
        }
        public static void AbsoluteSize(CrossUp.NodeRef props, ushort? width = null, ushort? height = null)
        {
            width ??= (ushort)props.Size.X;
            height ??= (ushort)props.Size.Y;
            SetSize(props.UnitBase->UldManager.NodeList[props.Id], (ushort)width, (ushort)height);
        }
        public static void AbsolutePos(CrossUp.NodeRef props, float? x = null, float? y = null)
        {
            x ??= props.Position.X;
            y ??= props.Position.Y;
            SetPos(props.UnitBase->UldManager.NodeList[props.Id], (float)x, (float)y);
        }

        public static void SetVis(CrossUp.NodeRef props,bool show)
        {
            NodeEdit.SetVis(props.UnitBase->UldManager.NodeList[props.Id],show);
        }
    };

}