using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageVerifier.MVAProxy;

namespace ImageVerifier
{
    public class ImInstDisplay
    {
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        string liveURL;

        public string LiveURL
        {
            get { return liveURL; }
            set { liveURL = value; }
        }

        private Guid originalFileGuid;
        private Guid imageID;
       // Locale[] locales = null;

        /// <summary>
        /// Gets/Sets the Guid of the Image
        /// </summary>
        public Guid ImageID
        {
            get { return imageID; }
            set { imageID = value; }
        }

        public Guid OriginalFileGuid
        {
            get { return originalFileGuid; }
            set { originalFileGuid = value; }
        }

    }
}

