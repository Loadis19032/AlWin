using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace AlWin
{
    public class Renderer : Overlay
    {
        public Vector2 screenSize = new Vector2(1920, 1080);

        public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        private bool enableESP = false;
        //public bool enableVisibilityCheck = false;
        public bool enableName = false;
        public bool enableLine = false;
        public bool enableBone = false;
        public bool enableHealth = false;

        public bool enableOnlyEnemy = false;

        public bool enableAimBot = false;
        public bool enableOnTeam = false;
        public float aimFov = 50;

        public bool RadarHack = false;

        public int fov = 60;

        public bool InfinityMoney = false;

        private Vector4 enemyColor = new Vector4(1, 0, 0, 1);
        private Vector4 teamColor = new Vector4(0, 1, 0, 1);
        //public Vector4 hiddenColor = new Vector4(0, 0, 0, 1);
        private Vector4 nameColor = new Vector4(1, 1, 1, 1);
        private Vector4 boneColor = new Vector4(1, 1, 1, 1);
        private Vector4 circleColor = new Vector4(1, 1, 1, 1);   

        float boneThickness = 4;

        ImDrawListPtr drawList;

        protected override void Render()
        {
            ImGui.Begin("ALWin v0.1");

            ImGui.Text("Visuals");
            ImGui.Separator();

            ImGui.Checkbox("ESP Enable", ref enableESP);
            // ImGui.Checkbox("Visible check", ref enableVisibilityCheck);

            ImGui.Checkbox("ESP Health", ref enableHealth);
            ImGui.Checkbox("ESP Line", ref enableLine);
            ImGui.Checkbox("ESP Name", ref enableName);
            ImGui.Checkbox("ESP Bone", ref enableBone);
            ImGui.Checkbox("ESP Only Enemy", ref enableOnlyEnemy);

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Team Color"))
                ImGui.ColorPicker4("##teamcolor", ref teamColor);

            if (ImGui.CollapsingHeader("Enemy Color"))
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);

            if (ImGui.CollapsingHeader("bone Color"))
                ImGui.ColorPicker4("##bonecolor", ref boneColor);

            ImGui.Separator();
            ImGui.Text("AimBot");
            ImGui.Separator();

            ImGui.Checkbox("AimBot Enable", ref enableAimBot);
            ImGui.Checkbox("AimBot On Team", ref enableOnTeam);
            //ImGui.SliderFloat("AimBot Fov", ref aimFov, 10, 300);

            ImGui.Text("AimBot HOTKEY 'x'");

            //if (ImGui.CollapsingHeader("FOV circle color"))
             //   ImGui.ColorPicker4("##circlecolor", ref circleColor);

            ImGui.Separator();
            ImGui.Text("Utils");
            ImGui.Separator();

            ImGui.SliderInt("FOV (no work)", ref fov, 58, 140);
            //ImGui.Checkbox("Radar Hack", ref RadarHack);

            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();
            
            if (enableAimBot)
            {
                //drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), aimFov, ImGui.ColorConvertFloat4ToU32(circleColor));
            }

            if (enableESP)
            {
                foreach(var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawHealthBar(entity);
                        DrawBox(entity);
                        DrawLine(entity);
                        DrawName(entity, 20);
                        DrawBones(entity);
                    }
                }
            }
        }

        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }

        private void DrawBones(Entity entity)
        {
            if (enableBone)
            {
                if (enableOnlyEnemy)
                {
                    if (localPlayer.team != entity.team)
                    {
                        uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

                        float currentBoneThickness = boneThickness / entity.distance;

                        drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
                        drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
                        drawList.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
                    }
                }
                else
                {
                    uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

                    float currentBoneThickness = boneThickness / entity.distance;

                    drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
                    drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
                    drawList.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
                }

            }
        }

        private void DrawHealthBar(Entity entity)
        {
            if (enableHealth)
            {
                if (enableOnlyEnemy)
                {
                    if (localPlayer.team != entity.team)
                    {
                        float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

                        float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
                        float boxRight = entity.position2D.X + entityHeight / 3;

                        float barPercentWidth = 0.05f;
                        float barPixelWidth = barPercentWidth * (boxRight - boxLeft);

                        float barHeight = entityHeight * (entity.health / 100f);

                        Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
                        Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);

                        Vector4 barColor = new Vector4(0, 1, 0, 1);

                        drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
                    }
                }
                else
                {
                    float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

                    float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
                    float boxRight = entity.position2D.X + entityHeight / 3;

                    float barPercentWidth = 0.05f;
                    float barPixelWidth = barPercentWidth * (boxRight - boxLeft);

                    float barHeight = entityHeight * (entity.health / 100f);

                    Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
                    Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);

                    Vector4 barColor = new Vector4(0, 1, 0, 1);

                    drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
                }
            }
        }

        private void DrawName(Entity entity, int yOffset)
        {
            if (enableName)
            {
                if (enableOnlyEnemy)
                {
                    if (localPlayer.team != entity.team)
                    {
                        Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
                        drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
                    }
                }
                else
                {
                    Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
                    drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
                }

            }
            
        }

        private void DrawBox(Entity entity)
        {
            
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            if (enableOnlyEnemy)
            {
                if (localPlayer.team != entity.team) { 
                    drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(enemyColor));
                }
            }
            else
            {
                Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;
                drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
            }
                



        }

        private void DrawLine(Entity entity)
        {
            if (enableLine)
            {
                if (enableOnlyEnemy)
                {
                    if (localPlayer.team != entity.team)
                    {
                        drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(enemyColor));
                    }
                }
                else
                {
                    Vector4 lineColor = localPlayer.team == entity.team ? teamColor: enemyColor;
                    drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
                }

            }
            
        }



        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer()
        {
            lock(entityLock)
            {
                return localPlayer;
            }
        }

        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
