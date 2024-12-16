namespace Test
{
    internal class DetectionItem
    {
        /// <summary>
        /// 检测目标的名称或类型
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 检测目标的置信度
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// 检测目标的位置信息（您可以根据需要定义，例如边界框）
        /// </summary>
        public BoundingBox Box { get; set; }

        public DetectionItem(string name, float confidence, BoundingBox box)
        {
            Name = name;
            Confidence = confidence;
            Box = box;
        }
    }

    internal class BoundingBox
    {
        /// <summary>
        /// 左上角X坐标
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// 左上角Y坐标
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }

        public BoundingBox(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
