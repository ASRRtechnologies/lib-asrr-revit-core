namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    public class GlbHeader
    {
        public byte[] Length { get; set; }

        public byte[] Magic()
        {
            return new byte[] { 0x67, 0x6C, 0x54, 0x46 };
        }

        public byte[] Version()
        {
            return new byte[] { 0x02, 0x00, 0x00, 0x00 };
        }
    }

    public class GlbJson
    {
        public byte[] Length { get; set; }

        public byte[] ChunkData { get; set; }

        public byte[] ChunkType()
        {
            return new byte[] { 0x4a, 0x53, 0x4f, 0x4e };
        }
    }

    public class GlbBin
    {
        public byte[] Length { get; set; }

        public byte[] ChunkData { get; set; }

        public byte[] ChunkType()
        {
            return new byte[] { 0x42, 0x49, 0x4e, 0x00 };
        }
    }
}