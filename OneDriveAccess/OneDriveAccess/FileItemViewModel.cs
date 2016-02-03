using Microsoft.OneDrive.Sdk;

namespace OneDriveAccess
{
    public class FileItemViewModel
    {
        public enum SpecialTypes
        {
            Normal = 0,
            LocalRoot,
            Shared,
            Shares,
            Share,
            OneLevelUp
        }

        private readonly string[] SpecialTypeNames = new string[]
        {
            "Normal",
            "Root",
            "Shared",
            "Shares",
            "Share",
            ".."
        };


        public FileItemViewModel(Item item)
        {
            SpecialType = SpecialTypes.Normal;
            Item = item;
        }
        
        public FileItemViewModel (Share share)
        {
            SpecialType = SpecialTypes.Share;
            Share = share;
        }

        public FileItemViewModel(SpecialTypes type)
        {
            SpecialType = type;
            Name = SpecialTypeNames[(int)type];
        }

        public FileItemViewModel(SpecialTypes type, Item item)
        {
            SpecialType = type;
            Item = item;
            Name = SpecialTypeNames[(int)type];
        }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Path { get; internal set; }

        public SpecialTypes SpecialType { get; }

        public Item Item { get; }

        public Item ParentItem { get; set; }

        public Share Share { get; }
    }
}
