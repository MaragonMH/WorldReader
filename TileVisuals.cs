using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldReader
{
    public class TileVisuals : FrameworkElement
    {
        private readonly VisualCollection _children = null;
        private TileMap tileMap = null;
        public TileVisuals(TileMap tileMap)
        {
            _children = new VisualCollection(this);
            this.tileMap = tileMap;

            new TileVisual(this);

            Debug.WriteLine("TileVisuals displayed");
        }

        ~TileVisuals()
        {
            this.tileMap = null;
        }

        public class TileVisual : DrawingVisual
        {
            const int tileWidth = 16;
            const int tileHeight = 16;

            public TileVisual(TileVisuals parent)
            {
                // Draw a tile
                var dc = RenderOpen();
                Rect rect = new Rect(new Point(0, 0), new Size(parent.tileMap.worldImage.PixelWidth, parent.tileMap.worldImage.PixelHeight));
                dc.DrawImage(parent.tileMap.worldImage, rect);
                dc.Close();

                // Add the tile as a child to the canvas parent.
                parent._children.Add(this);
            }
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

    }
}
