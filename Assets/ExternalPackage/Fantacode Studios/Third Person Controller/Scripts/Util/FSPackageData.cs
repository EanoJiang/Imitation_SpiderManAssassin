using System.Collections.Generic;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class FSPackageData
    {
        public static List<FSPackageInfo> packages = new List<FSPackageInfo>()
        {
            new FSPackageInfo()
            {
                packageName = "Parkour And Climbing System",
                iconPath = "Icons/Parkour_icon",
                youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAu8b3zKIyTMKhMK8iHsIaOn",
                documentationLink = "https://fantacode.gitbook.io/parkour-and-climbing-system",
                assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-parkour-and-climbing-system-258182"

                //addOns = new List<FSPackageInfo>()
                //{
                //    new FSPackageInfo()
                //    {
                //        packageName = "Parkour And Climbing AI",
                //        iconPath = "Icons/ParkourAI_icon",
                //        youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAsydRZvhwi-SO41x8ysESMe",
                //        documentationLink = "https://fantacode.gitbook.io/parkour-and-climbing-ai",
                //        assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-parkour-and-climbing-ai-269625"
                //    }
                //}

            },

            new FSPackageInfo()
            {
                packageName = "Parkour And Climbing AI",
                iconPath = "Icons/ParkourAI_icon",
                youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAsydRZvhwi-SO41x8ysESMe",
                documentationLink = "https://fantacode.gitbook.io/parkour-and-climbing-ai",
                assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-parkour-and-climbing-ai-269625"
            },

            new FSPackageInfo()
            {
                packageName = "Melee Combat System",
                iconPath = "Icons/Melee_icon",
                youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAuJ6g8xrL9r2K3vv2SwL1na",
                documentationLink = "https://fantacode.gitbook.io/melee-combat-system",
                assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-melee-combat-system-262354"
            },

            new FSPackageInfo()
            {
                packageName = "Grappling Hook System",
                iconPath = "Icons/Grappling_icon",
                youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAsY-QDOKJdTjBoTNZknwcWL",
                documentationLink = "https://fantacode.gitbook.io/grappling-hook-system",
                assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-grappling-hook-system-293862"
            },

            new FSPackageInfo()
            {
                packageName = "Rope Swinging System",
                iconPath = "Icons/Rope_icon",
                youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAsZg7p6pl9dTGjrB2Stvf-D",
                documentationLink = "https://fantacode.gitbook.io/rope-swinging-system",
                assetLink = "https://assetstore.unity.com/packages/slug/294572"
            }

            //new FSPackageInfo()
            //{
            //    packageName = "Dialogue And Cutscene System",
            //    iconPath = "Icons/Cutscene_icon",
            //    youtubeLink = "https://www.youtube.com/playlist?list=PLnbdyws4rcAt_3RFIDzCJZpfyN51uMRRk",
            //    documentationLink = "https://fantacode.gitbook.io/cutscene-system",
            //    assetLink = "https://assetstore.unity.com/packages/tools/game-toolkits/fs-dialogue-cutscene-system-246346"
            //}
        };

        public static void UnSelectAll(List<FSPackageInfo> info = null)
        {
            if (info == null)
                info = packages;
            foreach (var p in info)
            {
                p.opened = false;
                if (p.addOns.Count > 0)
                    UnSelectAll(p.addOns);
            }
        }
    }

    public class FSPackageInfo
    {
        public string packageName;
        public string youtubeLink;
        public string documentationLink;
        public string assetLink;
        public string iconPath;

        public Texture2D icon;


        public List<FSPackageInfo> addOns = new List<FSPackageInfo>();

        public bool opened;
    }

}