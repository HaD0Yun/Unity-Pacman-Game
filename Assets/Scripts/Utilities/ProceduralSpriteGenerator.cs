using UnityEngine;
using System.Collections.Generic;

namespace Pacman.Utilities
{
    /// <summary>
    /// Utility class for generating all game sprites procedurally.
    /// No external sprite assets required.
    /// </summary>
    public static class ProceduralSpriteGenerator
    {
        // Cache generated textures for cleanup
        private static List<Texture2D> _generatedTextures = new List<Texture2D>();

        /// <summary>
        /// Creates a solid colored circle sprite (for dots, etc.)
        /// </summary>
        public static Sprite CreateCircle(int diameter, Color color)
        {
            Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[diameter * diameter];
            Color transparent = new Color(0, 0, 0, 0);
            
            float radius = diameter / 2f;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    pixels[y * diameter + x] = distance <= radius ? color : transparent;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, diameter, diameter),
                new Vector2(0.5f, 0.5f),
                diameter
            );
        }

        /// <summary>
        /// Creates a Pacman sprite with optional mouth opening
        /// </summary>
        public static Sprite CreatePacman(int diameter, Color color, float mouthAngle = 45f, Vector2 direction = default)
        {
            if (direction == default) direction = Vector2.right;
            
            Texture2D texture = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[diameter * diameter];
            Color transparent = new Color(0, 0, 0, 0);
            
            float radius = diameter / 2f;
            Vector2 center = new Vector2(radius, radius);
            
            // Calculate facing angle
            float facingAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float halfMouth = mouthAngle / 2f;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius)
                    {
                        // Check if in mouth area
                        Vector2 dirFromCenter = (pos - center).normalized;
                        float angle = Mathf.Atan2(dirFromCenter.y, dirFromCenter.x) * Mathf.Rad2Deg;
                        float angleDiff = Mathf.DeltaAngle(facingAngle, angle);
                        
                        bool inMouth = Mathf.Abs(angleDiff) < halfMouth;
                        pixels[y * diameter + x] = inMouth ? transparent : color;
                    }
                    else
                    {
                        pixels[y * diameter + x] = transparent;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, diameter, diameter),
                new Vector2(0.5f, 0.5f),
                diameter
            );
        }

        /// <summary>
        /// Creates a ghost sprite with body and eyes
        /// </summary>
        public static Sprite CreateGhost(int width, int height, Color bodyColor, Vector2 eyeDirection = default)
        {
            if (eyeDirection == default) eyeDirection = Vector2.up;
            
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color transparent = new Color(0, 0, 0, 0);
            Color white = Color.white;
            Color blue = new Color(0.2f, 0.2f, 0.8f, 1f); // Pupil color
            
            float halfWidth = width / 2f;
            float bodyRadius = width * 0.45f;
            int waveHeight = height / 6;
            int bodyTop = height - (int)(width * 0.5f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    Color pixelColor = transparent;

                    // Head (top semicircle)
                    if (y >= bodyTop)
                    {
                        Vector2 headCenter = new Vector2(halfWidth, bodyTop);
                        float distToHead = Vector2.Distance(pos, headCenter);
                        if (distToHead <= bodyRadius)
                        {
                            pixelColor = bodyColor;
                        }
                    }
                    // Body (rectangle with wavy bottom)
                    else if (y >= waveHeight)
                    {
                        if (x >= (width - bodyRadius * 2) / 2 && x < (width + bodyRadius * 2) / 2)
                        {
                            pixelColor = bodyColor;
                        }
                    }
                    // Wavy bottom
                    else
                    {
                        // Create 3 waves
                        int waveCount = 3;
                        float waveWidth = width / (float)waveCount;
                        int waveIndex = (int)(x / waveWidth);
                        float waveX = (x % waveWidth) / waveWidth;
                        
                        // Sine wave for each segment
                        float waveY = Mathf.Sin(waveX * Mathf.PI) * waveHeight;
                        
                        if (y < waveY && x >= (width - bodyRadius * 2) / 2 && x < (width + bodyRadius * 2) / 2)
                        {
                            pixelColor = bodyColor;
                        }
                    }

                    pixels[y * width + x] = pixelColor;
                }
            }

            // Draw eyes
            int eyeRadius = width / 6;
            int pupilRadius = eyeRadius / 2;
            int eyeY = bodyTop + (int)(bodyRadius * 0.3f);
            int leftEyeX = (int)(halfWidth - bodyRadius * 0.35f);
            int rightEyeX = (int)(halfWidth + bodyRadius * 0.35f);

            // Eye direction offset for pupils
            Vector2 pupilOffset = eyeDirection.normalized * (eyeRadius - pupilRadius) * 0.5f;

            DrawCircleOnPixels(pixels, width, height, leftEyeX, eyeY, eyeRadius, white);
            DrawCircleOnPixels(pixels, width, height, rightEyeX, eyeY, eyeRadius, white);
            
            // Pupils
            DrawCircleOnPixels(pixels, width, height, 
                leftEyeX + (int)pupilOffset.x, eyeY + (int)pupilOffset.y, pupilRadius, blue);
            DrawCircleOnPixels(pixels, width, height, 
                rightEyeX + (int)pupilOffset.x, eyeY + (int)pupilOffset.y, pupilRadius, blue);

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                width
            );
        }

        /// <summary>
        /// Creates a frightened ghost sprite (blue with wavy mouth)
        /// </summary>
        public static Sprite CreateFrightenedGhost(int width, int height, Color bodyColor, bool isBlinking = false)
        {
            Color actualColor = isBlinking ? Color.white : bodyColor;
            
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color transparent = new Color(0, 0, 0, 0);
            Color faceColor = isBlinking ? new Color(1f, 0.5f, 0.5f, 1f) : new Color(1f, 0.8f, 0.7f, 1f);
            
            float halfWidth = width / 2f;
            float bodyRadius = width * 0.45f;
            int waveHeight = height / 6;
            int bodyTop = height - (int)(width * 0.5f);

            // Draw body (same as normal ghost)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pos = new Vector2(x + 0.5f, y + 0.5f);
                    Color pixelColor = transparent;

                    if (y >= bodyTop)
                    {
                        Vector2 headCenter = new Vector2(halfWidth, bodyTop);
                        float distToHead = Vector2.Distance(pos, headCenter);
                        if (distToHead <= bodyRadius)
                        {
                            pixelColor = actualColor;
                        }
                    }
                    else if (y >= waveHeight)
                    {
                        if (x >= (width - bodyRadius * 2) / 2 && x < (width + bodyRadius * 2) / 2)
                        {
                            pixelColor = actualColor;
                        }
                    }
                    else
                    {
                        float waveWidth = width / 3f;
                        float waveX = (x % waveWidth) / waveWidth;
                        float waveY = Mathf.Sin(waveX * Mathf.PI) * waveHeight;
                        
                        if (y < waveY && x >= (width - bodyRadius * 2) / 2 && x < (width + bodyRadius * 2) / 2)
                        {
                            pixelColor = actualColor;
                        }
                    }

                    pixels[y * width + x] = pixelColor;
                }
            }

            // Draw simple eyes (small squares)
            int eyeSize = width / 8;
            int eyeY = bodyTop + (int)(bodyRadius * 0.3f);
            int leftEyeX = (int)(halfWidth - bodyRadius * 0.3f);
            int rightEyeX = (int)(halfWidth + bodyRadius * 0.3f);

            DrawRectOnPixels(pixels, width, height, leftEyeX - eyeSize/2, eyeY - eyeSize/2, eyeSize, eyeSize, faceColor);
            DrawRectOnPixels(pixels, width, height, rightEyeX - eyeSize/2, eyeY - eyeSize/2, eyeSize, eyeSize, faceColor);

            // Draw wavy mouth
            int mouthY = bodyTop - (int)(bodyRadius * 0.2f);
            int mouthWidth = (int)(bodyRadius * 1.2f);
            int mouthStartX = (int)(halfWidth - mouthWidth / 2);
            
            for (int x = 0; x < mouthWidth; x++)
            {
                float t = x / (float)mouthWidth;
                int waveOffset = (int)(Mathf.Sin(t * Mathf.PI * 4) * 2);
                int px = mouthStartX + x;
                int py = mouthY + waveOffset;
                
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    pixels[py * width + px] = faceColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                width
            );
        }

        /// <summary>
        /// Creates ghost eyes only sprite (for eaten state)
        /// </summary>
        public static Sprite CreateGhostEyes(int width, int height, Vector2 direction = default)
        {
            if (direction == default) direction = Vector2.up;
            
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            Color transparent = new Color(0, 0, 0, 0);
            Color white = Color.white;
            Color blue = new Color(0.2f, 0.2f, 0.8f, 1f);

            // Fill transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = transparent;

            int eyeRadius = width / 4;
            int pupilRadius = eyeRadius / 2;
            int eyeY = height / 2;
            int leftEyeX = width / 3;
            int rightEyeX = width * 2 / 3;

            Vector2 pupilOffset = direction.normalized * (eyeRadius - pupilRadius) * 0.6f;

            DrawCircleOnPixels(pixels, width, height, leftEyeX, eyeY, eyeRadius, white);
            DrawCircleOnPixels(pixels, width, height, rightEyeX, eyeY, eyeRadius, white);
            
            DrawCircleOnPixels(pixels, width, height, 
                leftEyeX + (int)pupilOffset.x, eyeY + (int)pupilOffset.y, pupilRadius, blue);
            DrawCircleOnPixels(pixels, width, height, 
                rightEyeX + (int)pupilOffset.x, eyeY + (int)pupilOffset.y, pupilRadius, blue);

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                width
            );
        }

        /// <summary>
        /// Creates a solid wall tile
        /// </summary>
        public static Sprite CreateWallTile(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        /// <summary>
        /// Creates a wall tile with border (outlined style)
        /// </summary>
        public static Sprite CreateOutlinedWallTile(int size, Color fillColor, Color borderColor, int borderWidth = 2)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < borderWidth || x >= size - borderWidth || 
                                   y < borderWidth || y >= size - borderWidth;
                    pixels[y * size + x] = isBorder ? borderColor : fillColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        /// <summary>
        /// Creates a rounded corner wall tile
        /// </summary>
        public static Sprite CreateRoundedWallTile(int size, Color color, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            Color transparent = new Color(0, 0, 0, 0);
            
            int cornerRadius = size / 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool shouldDraw = true;

                    // Check corners
                    if (topLeft && x < cornerRadius && y >= size - cornerRadius)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, size - cornerRadius));
                        shouldDraw = dist <= cornerRadius;
                    }
                    if (topRight && x >= size - cornerRadius && y >= size - cornerRadius)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, size - cornerRadius));
                        shouldDraw = dist <= cornerRadius;
                    }
                    if (bottomLeft && x < cornerRadius && y < cornerRadius)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius - 1));
                        shouldDraw = dist <= cornerRadius;
                    }
                    if (bottomRight && x >= size - cornerRadius && y < cornerRadius)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, cornerRadius - 1));
                        shouldDraw = dist <= cornerRadius;
                    }

                    pixels[y * size + x] = shouldDraw ? color : transparent;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            
            _generatedTextures.Add(texture);

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        /// <summary>
        /// Creates a power pellet sprite (larger, pulsing dot)
        /// </summary>
        public static Sprite CreatePowerPellet(int diameter, Color color)
        {
            return CreateCircle(diameter, color);
        }

        // === HELPER METHODS ===

        private static void DrawCircleOnPixels(Color[] pixels, int width, int height, int cx, int cy, int radius, Color color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist <= radius)
                        {
                            pixels[y * width + x] = color;
                        }
                    }
                }
            }
        }

        private static void DrawRectOnPixels(Color[] pixels, int width, int height, int startX, int startY, int rectWidth, int rectHeight, Color color)
        {
            for (int y = startY; y < startY + rectHeight; y++)
            {
                for (int x = startX; x < startX + rectWidth; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        /// <summary>
        /// Cleans up all generated textures (call on scene unload)
        /// </summary>
        public static void CleanupTextures()
        {
            foreach (var texture in _generatedTextures)
            {
                if (texture != null)
                {
                    Object.Destroy(texture);
                }
            }
            _generatedTextures.Clear();
        }
    }
}
