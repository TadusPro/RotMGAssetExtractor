namespace RotMGAssetExtractor.UnityExtractor.resextractor
{
    public class TextAsset
    {
        public static readonly string[] NonXmlFiles = {
            "manifest_xml", "COPYING", "Errors", "ExplainUnzip", "cloth_bazaar", "Cursors", "Dialogs", "Keyboard",
            "LICENSE", "LineBreaking Following Characters", "LineBreaking Leading Characters", "manifest_json", "spritesheetf",
            "iso_4217", "data", "manifest", "BillingMode"
        };

        private DataReader reader;
        public string Name { get; private set; }
        public byte[] Script { get; private set; }

        public TextAsset(ObjectReader o)
        {
            this.reader = o.Reader;
            reader.SetPosition(o.ByteStart);

            Name = reader.ReadAlignedString();

            int bytes = reader.ReadInt();
            Script = reader.ReadBytes(bytes);
        }
    }
}