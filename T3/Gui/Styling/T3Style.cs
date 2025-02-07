﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace T3.Gui
{
    /// <summary>
    /// Defines global style constants and ImGUI overrides
    /// </summary>
    /// <remarks>
    /// Sadly we can't use the original ImGUI-Style struct because the .net wrapper only defines get-accessors.
    /// </remarks>
    public static class T3Style
    {
        public static class Colors
        {
            public static Color ConnectedParameterColor = new Color(0.6f, 0.6f, 1f, 1f);
            public static Color ValueLabelColor = new Color(1, 1, 1, 0.5f);
            public static Color ValueLabelColorHover = new Color(1, 1, 1, 1.2f);
            
            public static Color GraphLineColor = new Color(1, 1, 1, 0.3f);
            public static Color GraphLineColorHover = new Color(1, 1, 1, 0.7f);
            
            public static Color GraphAxisColor = new Color(0, 0, 0, 0.3f);

            public static Color ButtonColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            public static Color ButtonHoverColor = new Color(43, 65, 80, 255);
            public static Color TextMuted = new Color(0.5f);
            public static Color TextDisabled = new Color(0.328f, 0.328f, 0.328f, 1.000f);
            public static Color WarningColor = new Color(203, 19,113, 255);
            
            public static Color WindowBackground = new Color(0.05f, 0.05f,0.05f, 1);
        }
        
        

        public static readonly Color FragmentLineColor = Color.Orange;
        
        public static float ToolBarHeight = 25; 

        public static void Apply()
        {
            if (!_overridesEnabled)
                return;

            var style = ImGui.GetStyle();
            _colorOverrides.Apply(style);
            _styleOverrides.Apply(style);
        }


        public static void DrawUi()
        {
            ImGui.Checkbox("Apply Override", ref _overridesEnabled);
            _styleOverrides.DrawUi();
            _colorOverrides.DrawUi();
        }


        private static ImGuiColorOverrides _colorOverrides = new ImGuiColorOverrides();
        private class ImGuiColorOverrides
        {
            public ImGuiColorOverrides()
            {
                var style = ImGui.GetStyle();
                _colors = new Vector4[style.Colors.Count];
                for (var i = 0; i < style.Colors.Count; i++)
                {
                    _colors[i] = style.Colors[i];
                }

                _colors[(int)ImGuiCol.Text] = new Vector4(1, 1, 1, 0.85f);
                _colors[(int)ImGuiCol.TextDisabled] = Colors.TextDisabled;
                _colors[(int)ImGuiCol.Button] = Colors.ButtonColor;
                _colors[(int)ImGuiCol.ButtonHovered] = Colors.ButtonHoverColor;
                _colors[(int)ImGuiCol.Border] = new Vector4(0, 0.00f, 0.00f, 0.97f);
                _colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
                _colors[(int)ImGuiCol.FrameBg] = new Vector4(0.13f, 0.13f, 0.13f, 0.80f);
                _colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.38f, 0.38f, 0.38f, 0.40f);
                _colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.00f, 0.55f, 0.8f, 1.00f);
                _colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.12f, 0.12f, 0.12f, 0.53f);
                _colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 0.33f);
                _colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.00f, 0.00f, 0.00f, 0.25f);
                _colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1f,0.1f,0.1f, 0.98f);
                _colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.1f,0.1f,0.1f, 0.1f);
                _colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.0f,0.0f,0.0f, 1.0f);
                
            }

            public void Apply(ImGuiStylePtr style)
            {
                for (var i = 0; i < style.Colors.Count; i++)
                {
                    style.Colors[i] = _colors[i];
                }
            }

            public void DrawUi()
            {
                for (var index = 0; index < _colors.Length; index++)
                {
                    var x = (ImGuiCol)index;
                    ImGui.ColorEdit4("" + x, ref _colors[index]);
                }
            }
            private static Vector4[] _colors;
        }

        private static ImGuiStyleOverrides _styleOverrides = new ImGuiStyleOverrides();
        private class ImGuiStyleOverrides
        {
            public Vector2 ItemSpacing = new Vector2(1, 1);
            public Vector2 FramePadding = new Vector2(7, 4);
            public Vector2 ItemInnerSpacing = new Vector2(3, 2);
            public Vector2 WindowPadding = Vector2.Zero;
            public float GrabMinSize = 2;
            public float FrameBorderSize = 0;
            public float WindowRounding = 0;
            public float ChildRounding = 0;
            public float ScrollbarRounding = 2;
            public float FrameRounding = 0f;
             

            public void Apply(ImGuiStylePtr style)
            {
                style.WindowPadding = WindowPadding;
                style.FramePadding = FramePadding;
                style.ItemSpacing = ItemSpacing;
                style.ItemInnerSpacing = ItemInnerSpacing;
                style.GrabMinSize = GrabMinSize;
                style.FrameBorderSize = FrameBorderSize;
                style.WindowRounding = WindowRounding;
                style.ChildRounding = ChildRounding;
                style.ScrollbarRounding = ScrollbarRounding;
                style.FrameRounding = FrameRounding;
                style.DisplayWindowPadding = Vector2.Zero;
                style.DisplaySafeAreaPadding = Vector2.Zero;
                style.ChildBorderSize = 0;
            }

            public void DrawUi()
            {
                ImGui.DragFloat2("FramePadding", ref FramePadding);
                ImGui.DragFloat2("ItemSpacing", ref ItemSpacing);
                ImGui.DragFloat2("ItemInnerSpacing", ref ItemInnerSpacing);

                ImGui.DragFloat("GrabMinSize", ref GrabMinSize);
                ImGui.DragFloat("FrameBorderSize", ref FrameBorderSize);
                ImGui.DragFloat("WindowRounding", ref WindowRounding);
                ImGui.DragFloat("ChildRounding", ref ChildRounding);
                ImGui.DragFloat("ScrollbarRounding", ref ScrollbarRounding);
                ImGui.DragFloat("FrameRounding", ref FrameRounding);
                ImGui.Spacing();
            }
        }

        private static bool _overridesEnabled = true;
    }
}
