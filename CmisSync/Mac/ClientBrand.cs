namespace CmisSync
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib;

    public class ClientBrand : ClientBrandBase
    {
        public override List<string> GetPathList ()
        {
            List<string> pathList = new List<string> ();
            pathList.Add ("/DataSpaceSync/common/about.png");
            pathList.Add ("/DataSpaceSync/common/about@2x.png");
            pathList.Add ("/DataSpaceSync/common/side-splash.png");
            pathList.Add ("/DataSpaceSync/common/side-splash@2x.png");
            pathList.Add ("/DataSpaceSync/mac/folder.icns");
            return pathList;
        }
    }
}

