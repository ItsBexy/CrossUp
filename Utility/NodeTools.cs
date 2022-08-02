﻿using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Numerics;
using CrossUp;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable InvertIf

namespace NodeTools;

/// <summary>A wrapper that points to an AtkUnitBase*.
///<br/><br/>
/// <b>Child Node Indexer:</b><br/>
/// [int <b>i</b>] - Returns a node by its index in UldManager.NodeList<br/>
/// [uint <b>id</b>]  - Returns a node by its ID
/// </summary>
public sealed unsafe class BaseWrapper
{
    /// <param name="addonName">The internal name of a UI Addon</param>
    public BaseWrapper(string addonName)
    {
        UnitBase = (AtkUnitBase*)Service.GameGui.GetAddonByName(addonName, 1);
        AddonName = addonName;
    }
    public string AddonName { get; }
    public readonly AtkUnitBase* UnitBase;
    public static bool BaseCheck(AtkUnitBase* unitBase) => unitBase != null && unitBase->UldManager.NodeListSize > 0 && (unitBase->X > 0 || unitBase->Y > 0);
    public bool Exists => BaseCheck(UnitBase) || BaseCheck((AtkUnitBase*)Service.GameGui.GetAddonByName(AddonName, 1));
    public AtkResNode** NodeList => UnitBase->UldManager.NodeList;
    public ushort NodeListSize => UnitBase->UldManager.NodeListSize;
    public ushort NodeListCount => UnitBase->UldManager.NodeListCount;
    public short X { get => UnitBase->X; set => UnitBase->X = value; }
    public short Y { get => UnitBase->Y; set => UnitBase->Y = value; }
    public float Scale { get => UnitBase->Scale; set => UnitBase->Scale = value; }
    public NodeWrapper this[uint id] => new(UnitBase->GetNodeById(id));
    public NodeWrapper this[int i] => new(NodeListSize > i ? NodeList[i] : null);
    public BaseWrapper SetPos(short x, short y)
    {
        UnitBase->SetPosition(x, y);
        return this;
    }
    public BaseWrapper SetPos(Vector2 position) => SetPos((short)position.X, (short)position.Y);
    public BaseWrapper SetPos(short? x = null, short? y = null)
    {
        x ??= X;
        y ??= Y;
        return SetPos((short)x, (short)y);
    }

    public static implicit operator AtkUnitBase*(BaseWrapper bw) => bw.UnitBase;
    public static implicit operator string(BaseWrapper bw) => bw.AddonName;
}

/// <summary>A wrapper that points to an AtkResNode*, alongside optional defaults for that node's properties, and chainable methods for editing the node
/// <br/><br/>
/// <b>Child Node Indexer:</b><br/>
/// [int <b>i</b>] - Returns a child node by its index in Component->UldManager.NodeList<br/>
/// [uint <b>id</b>]  - Returns a child node by its ID
/// </summary>
public sealed unsafe class NodeWrapper
{
    /// <param name="node">Any AtkResNode* or NodeWrapper</param>
    /// <param name="pos">The node's default position</param>
    /// <param name="size">The node's default size</param>
    public NodeWrapper(AtkResNode* node, Vector2? pos = null, Vector2? size = null)
    {
        Node = node;
        if (pos != null) { DefaultPos = (Vector2)pos; }
        if (size != null) { DefaultSize = (Vector2)size; }
    }
    public Vector2? DefaultPos;
    public Vector2 DefaultSize;
    public readonly AtkResNode* Node;

    public static implicit operator AtkResNode*(NodeWrapper wrap) => wrap.Node;
    public static implicit operator NodeWrapper(AtkResNode* node) => new(node);

    public NodeWrapper this[uint id]
    {
        get
        {
            try
            {
                AtkComponentNode* comp;
                return Node != null ? (comp = Node->GetAsAtkComponentNode()) != null ?
                        comp->Component->UldManager.SearchNodeById(id) :
                        Warning($"No Child node found for NodeWrapper with ID {id}\n{new System.Diagnostics.StackTrace()}") :
                    Warning($"Node is null and has no children \n{new System.Diagnostics.StackTrace()}");
            }
            catch (Exception) { return Warning($"Error retrieving child node with ID {id}\n{new System.Diagnostics.StackTrace()}"); }
        }
    }

    public NodeWrapper this[int i]
    {
        get
        {
            try
            {
                AtkComponentNode* comp;
                AtkUldManager uld;
                return Node != null ? (comp = Node->GetAsAtkComponentNode()) != null && (uld = comp->Component->UldManager).NodeListSize >= i ?
                        uld.NodeList[i] :
                        Warning($"No Child node found for NodeWrapper at index {i}\n{new System.Diagnostics.StackTrace()}") :
                    Warning($"Node is null and has no children \n{new System.Diagnostics.StackTrace()}");
            }
            catch (Exception) { return Warning($"Error retrieving child node at index {i}\n{new System.Diagnostics.StackTrace()}"); }
        }
    }

    /// <summary>Logs a warning but still returns the NodeWrapper</summary>
    public NodeWrapper Warning(string message)
    {
        PluginLog.LogWarning(message);
        return this;
    }

    /// <summary>A set of properties that can be applied to a node all at once</summary>
    public struct PropertySet
    {
        public bool? Visible { get; init; }
        public float? X { get; init; }
        public float? Y { get; init; }
        public Vector2? Position { get; init; }
        public float? Scale { get; init; }
        public ushort? Width { get; init; }
        public ushort? Height { get; init; }
        public float? Alpha { get; init; }
        public Vector2? Origin { get; init; }
        public Vector3? Color { get; init; }
        public Vector3? Multiply { get; init; }
    }
    public int ChildCount() => Node == null ? 0 : Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;
    public NodeWrapper SetVis(bool show)
    {
        if (Node != null)
        {
            if (show) Node->Flags |= 0x10;
            else Node->Flags &= ~0x10;
            Node->Flags_2 |= 0xD;
        }
        return this;
    }

    public NodeWrapper ChildVis(bool show)
    {
        if (Node != null)
        {
            var count = (int)Node->GetAsAtkComponentNode()->Component->UldManager.NodeListSize;
            for (var i = 0; i < count; i++) this[i].SetVis(show);
        }
        return this;
    }

    public NodeWrapper SetScale(float scale = 1f)
    {
        if (Node != null)
        {
            Node->ScaleX = scale;
            Node->ScaleY = scale;
            Node->Flags_2 |= 0xD;
        }
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
        if (Node != null)
        {
            Node->Color.R = (byte)(color.X * 255f);
            Node->Color.G = (byte)(color.Y * 255f);
            Node->Color.B = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
        }
        return this;
    }

    public NodeWrapper SetMultiply(Vector3 color)
    {
        if (Node != null)
        {
            Node->MultiplyRed = (byte)(color.X * 255f);
            Node->MultiplyGreen = (byte)(color.Y * 255f);
            Node->MultiplyBlue = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
        }
        return this;
    }

    public NodeWrapper SetAlpha(byte a)
    {
        if (Node != null)
        {
            Node->Color.A = a;
            Node->Flags_2 |= 0xD;
        }
        return this;
    }

    public NodeWrapper SetBlend(uint b)
    {
        AtkNineGridNode* nineGrid;
        if (Node != null && (nineGrid = Node->GetAsAtkNineGridNode()) != null) nineGrid->BlendMode = b;
        return this;
    }

    public NodeWrapper SetOrigin(Vector2 origin) => Node == null ? this : SetOrigin(origin.X, origin.Y);
    public NodeWrapper SetOrigin(float x, float y)
    {
        if (Node != null)
        {
            Node->OriginX = x;
            Node->OriginY = y;
            Node->Flags_2 |= 0xD;
        }
        return this;
    }

    public NodeWrapper SetPos(Vector2 pos)
    {
        if (Node != null) Node->SetPositionFloat(pos.X, pos.Y);
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
        if (DefaultPos != null && Node != null) Node->SetPositionFloat(((Vector2)DefaultPos).X + x, ((Vector2)DefaultPos).Y + y);
        return this;
    }

    public NodeWrapper SetTextColor(Vector3 color,Vector3 glow)
    {
        if (Node != null)
        {
            var tnode = Node->GetAsAtkTextNode();
            tnode->EdgeColor.R = (byte)(glow.X * 255f);
            tnode->EdgeColor.G = (byte)(glow.Y * 255f);
            tnode->EdgeColor.B = (byte)(glow.Z * 255f);
            tnode->TextColor.R = (byte)(color.X * 255f);
            tnode->TextColor.G = (byte)(color.Y * 255f);
            tnode->TextColor.B = (byte)(color.Z * 255f);
            Node->Flags_2 |= 0xD;
        }

        return this;
    }

    public NodeWrapper SetProps(PropertySet props)
    {
        if (Node != null)
        {
            if (props.Position != null) SetPos(props.Position);
            if (props.Width != null) Node->Width = (ushort)props.Width;
            if (props.Height != null) Node->Height = (ushort)props.Height;
            if (props.Scale != null) SetScale((float)props.Scale);
            if (props.Visible != null) SetVis((bool)props.Visible);
            if (props.Color != null) SetColor((Vector3)props.Color);
            if (props.Multiply != null) SetMultiply((Vector3)props.Multiply);
            if (props.Alpha != null) SetAlpha((byte)props.Alpha);
            if (props.Origin != null) SetOrigin((Vector2)props.Origin);

            Node->Flags_2 |= 0xD;
        }

        return this;
    }
}

