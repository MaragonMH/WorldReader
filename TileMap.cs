using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldReader
{
    public class TileMap
    {
        public const int tileWidth = 16;
        public const int tileHeight = 16;
        const int dpi = 96; // Result in 1:1
        private readonly byte[] emptyPixel = { 255, 255, 255, 0 };

        public WriteableBitmap worldImage = null;
        private BitmapFrame tileMapImage = null;
        private List<CroppedBitmap> tileMapTileImages = null;
        public int width;
        public int height;
        private int worldWidth;
        private int worldHeight;

        public TileMap(FileStream fileStream, int worldWidth, int worldHeight)
        {
            tileMapTileImages = new List<CroppedBitmap>();
            tileMapImage = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;

            width = tileMapImage.PixelWidth / tileWidth;
            height = tileMapImage.PixelHeight / tileHeight;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Int32Rect rect = new Int32Rect(w * tileWidth, h * tileHeight, tileWidth, tileHeight);
                    tileMapTileImages.Add(new CroppedBitmap(tileMapImage, rect));
                }
            }

            worldImage = new WriteableBitmap(worldWidth * tileWidth, worldHeight * tileHeight, dpi, dpi, tileMapImage.Format, null);
            Debug.WriteLine("TileMap read");
        }

        ~TileMap()
        {
            this.worldImage = null;
            this.tileMapImage = null;
            this.tileMapTileImages = null;
    }

        public void setTile(int worldWidth, int worldHeight, int index, bool overlay = false)
        {
            int pixelWidth = (worldImage.Format.BitsPerPixel+7) / 8;
            int normalizedIndex = index & (int)~WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.MaskFlip;
            bool flipHorizontally = (((uint)index & (uint)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipHorizontally) != 0);
            bool flipVertically = (((uint)index & (uint)WorldDatastructur.TileMapGroup.Tile.CollisionTile.TileFlags.FlagFlipVertically) != 0);
            if (tileMapTileImages != null && tileMapTileImages.ElementAtOrDefault(normalizedIndex) != null)
            {
                byte[] pixels = BitmapSourceToArray(tileMapTileImages[normalizedIndex], new Int32Rect(0, 0, tileWidth, tileHeight));

                // Flip Tile if neccessary
                if(flipHorizontally) pixels = FlipHorizontally(pixels, pixelWidth, tileWidth, tileHeight);
                if(flipVertically) pixels = FlipVertically(pixels, pixelWidth, tileWidth, tileHeight);

                Int32Rect rect = new Int32Rect(worldWidth * tileWidth, worldHeight * tileHeight, tileWidth, tileHeight);
                // keep all pixels which are only behind new transparent pixels
                if (overlay)
                {
                    byte[] originalPixels = BitmapSourceToArray(worldImage, rect);
                    for (int i = 0; i < pixels.Count(); i += 4)
                    {
                        bool pixelIsAlpha = pixels[i] == 255 && pixels[i + 1] == 255 && pixels[i + 2] == 255 && pixels[i + 3] == 0;
                        // bool orignalPixelIsAlpha = originalPixels[i] == 255 && originalPixels[i + 1] == 255 && originalPixels[i + 2] == 255 && originalPixels[i + 3] == 0;
                        if (pixelIsAlpha) Buffer.BlockCopy(originalPixels, i, pixels, i, 4);
                    }
                }
                worldImage.WritePixels(rect, pixels, pixels.Count() / tileHeight, 0);
            }
        }

        public void unsetAllTiles()
        {
            byte[] pixels = new byte[tileHeight * tileWidth * worldImage.Format.BitsPerPixel];
            for (int i = 0; i < tileHeight * tileWidth; i += 4)
            {
                Buffer.BlockCopy(emptyPixel, 0, pixels, i, 4);
            }
            for (int w = 0; w < worldWidth; w++)
            {
                for (int h = 0; h < worldHeight; h++)
                {
                    Int32Rect rect = new Int32Rect(w * tileWidth, h * tileHeight, tileWidth, tileHeight);
                    worldImage.WritePixels(rect, pixels, pixels.Count() / tileHeight, 0);
                }
            }
        }

        private byte[] BitmapSourceToArray(BitmapSource bitmapSource, Int32Rect rect)
        {
            // Stride = (width) x (bytes per pixel)
            int stride = (int)rect.Width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[(int)rect.Height * stride];

            bitmapSource.CopyPixels(rect, pixels, stride, 0);

            return pixels;
        }

        private byte[] FlipVertically(byte[] pixels, int pixelWidth, int width, int height)
        {
            byte[] result = new byte[pixels.Count()];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Buffer.BlockCopy(pixels, (h * width + w) * pixelWidth, result, ((height - h - 1) * width + w) * pixelWidth, pixelWidth);
                }
            }
            return result;
        }

        private byte[] FlipHorizontally(byte[] pixels, int pixelWidth, int width, int height)
        {
            byte[] result = new byte[pixels.Count()];
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    Buffer.BlockCopy(pixels, (h * width + w) * pixelWidth, result, (h * width + (width - w - 1)) * pixelWidth, pixelWidth);
                }
            }
            return result;
        }

        private byte[] compositePixels(byte[] backgroundPixel, byte[] foregroundPixel)
        {
            return new byte[4];
        }

    }
}
