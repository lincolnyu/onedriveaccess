using Microsoft.OneDrive.Sdk;

namespace OneDriveAccess
{
    public class FileItemViewModel
    {
        public enum SpecialTypes
        {
            Normal = 0,
            LocalRoot,
            SharedRoot
        }

        private readonly string[] SpecialTypeNames = new string[]
        {
            "Normal",
            "Root",
            "Shared"
        };


        public FileItemViewModel(Item item)
        {
            SpecialType = SpecialTypes.Normal;
            Item = item;
        }

        public FileItemViewModel(SpecialTypes type)
        {
            SpecialType = type;
            Name = SpecialTypeNames[(int)type];
        }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Path { get; internal set; }

        public SpecialTypes SpecialType { get; }

        public Item Item { get; }
    }
}
