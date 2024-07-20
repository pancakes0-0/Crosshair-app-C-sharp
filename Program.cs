using System;
using System.Numerics;
using ImGuiNET;
using ClickableTransparentOverlay;
using System.Runtime.InteropServices;

namespace MSDApplication
{
    public class MSDOverlay : Overlay
    {
        private Vector4 _accentColor = new Vector4(0.2f, 0.6f, 1.0f, 1.0f);
        private Vector4 _dotColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private float _dotSize = 5.0f;
        private int _dotShape = 0; // 0: Circle, 1: Cross, 2: Triangle, 3: Square
        private bool _isFilled = true;
        private bool _showOutline = false;
        private float _outlineThickness = 1.0f;
        private Vector4 _outlineColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        private bool _showOverlay = true;

        public MSDOverlay()
        {
            // Set the initial font in the constructor
            VSync = true;
        }

        protected override void Render()
        {
            if (ImGui.IsKeyPressed(ImGuiKey.Insert))
            {
                _showOverlay = !_showOverlay;
            }

            if (!_showOverlay) return;

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 0.9f));
            ImGui.PushStyleColor(ImGuiCol.TitleBg, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.Button, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _accentColor * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, _accentColor * 0.8f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(300, 0));

            if (ImGui.Begin("MSD Settings", ImGuiWindowFlags.None))
            {
                if (ImGui.ColorEdit4("Accent Color", ref _accentColor))
                {
                    UpdateAccentColor();
                }

                ImGui.Separator();

                ImGui.Text("Dot Settings");
                ImGui.ColorEdit4("Dot Color", ref _dotColor);
                ImGui.SliderFloat("Dot Size", ref _dotSize, 1.0f, 20.0f);
                string[] shapes = { "Circle", "Cross", "Triangle", "Square" };
                ImGui.Combo("Dot Shape", ref _dotShape, shapes, shapes.Length);
                ImGui.Checkbox("Filled", ref _isFilled);
                ImGui.Checkbox("Show Outline", ref _showOutline);

                if (_showOutline)
                {
                    ImGui.SliderFloat("Outline Thickness", ref _outlineThickness, 1.0f, 5.0f);
                    ImGui.ColorEdit4("Outline Color", ref _outlineColor);
                }

                ImGui.Separator();


                ImGui.Text("Press Insert to hide/show overlay");
            }
            ImGui.End();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor(6);

            // Draw the middle screen dot
            var io = ImGui.GetIO();
            var pos = new Vector2(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f);
            var drawList = ImGui.GetForegroundDrawList();

            DrawCustomShape(drawList, pos, _dotShape, _dotSize, _dotColor, _outlineColor, _showOutline, _outlineThickness, _isFilled);
        }

        private void DrawCustomShape(ImDrawListPtr drawList, Vector2 pos, int shape, float size, Vector4 fillColor, Vector4 outlineColor, bool showOutline, float outlineThickness, bool isFilled)
        {
            uint fillColorU32 = ImGui.ColorConvertFloat4ToU32(fillColor);
            uint outlineColorU32 = ImGui.ColorConvertFloat4ToU32(outlineColor);

            switch (shape)
            {
                case 0: // Circle
                    if (showOutline)
                        drawList.AddCircle(pos, size + outlineThickness, outlineColorU32, 32, outlineThickness);
                    if (isFilled)
                        drawList.AddCircleFilled(pos, size, fillColorU32, 32);
                    else
                        drawList.AddCircle(pos, size, fillColorU32, 32, 1.0f);
                    break;
                case 1: // Cross
                    float halfSize = size * 0.5f;
                    if (showOutline)
                    {
                        drawList.AddLine(new Vector2(pos.X - halfSize - outlineThickness, pos.Y), new Vector2(pos.X + halfSize + outlineThickness, pos.Y), outlineColorU32, outlineThickness * 3);
                        drawList.AddLine(new Vector2(pos.X, pos.Y - halfSize - outlineThickness), new Vector2(pos.X, pos.Y + halfSize + outlineThickness), outlineColorU32, outlineThickness * 3);
                    }
                    drawList.AddLine(new Vector2(pos.X - halfSize, pos.Y), new Vector2(pos.X + halfSize, pos.Y), fillColorU32, isFilled ? 2.0f : 1.0f);
                    drawList.AddLine(new Vector2(pos.X, pos.Y - halfSize), new Vector2(pos.X, pos.Y + halfSize), fillColorU32, isFilled ? 2.0f : 1.0f);
                    break;
                case 2: // Triangle
                    Vector2 p1 = new Vector2(pos.X, pos.Y - size);
                    Vector2 p2 = new Vector2(pos.X - size * 0.866f, pos.Y + size * 0.5f);
                    Vector2 p3 = new Vector2(pos.X + size * 0.866f, pos.Y + size * 0.5f);
                    if (showOutline)
                        drawList.AddTriangle(p1, p2, p3, outlineColorU32, outlineThickness);
                    if (isFilled)
                        drawList.AddTriangleFilled(p1, p2, p3, fillColorU32);
                    else
                        drawList.AddTriangle(p1, p2, p3, fillColorU32, 1.0f);
                    break;
                case 3: // Square
                    Vector2 min = new Vector2(pos.X - size, pos.Y - size);
                    Vector2 max = new Vector2(pos.X + size, pos.Y + size);
                    if (showOutline)
                        drawList.AddRect(min - new Vector2(outlineThickness), max + new Vector2(outlineThickness), outlineColorU32, 0, ImDrawFlags.None, outlineThickness);
                    if (isFilled)
                        drawList.AddRectFilled(min, max, fillColorU32);
                    else
                        drawList.AddRect(min, max, fillColorU32, 0, ImDrawFlags.None, 1.0f);
                    break;
            }
        }

        private void UpdateAccentColor()
        {
            ImGui.PushStyleColor(ImGuiCol.TitleBg, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.Button, _accentColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _accentColor * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, _accentColor * 0.8f);
        }

    }

    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            var overlay = new MSDOverlay();
            overlay.Start();
        }
    }
}