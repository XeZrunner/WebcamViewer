using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebcamViewer
{
    public class Webcam
    {

        // ===== Basic info ===== //

        #region Basic info
        /// <summary>
        /// The name of the camera.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL of the camera image.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The local location to save the image to. null to ask every time for a location when saving.
        /// </summary>
        public string SaveLocation { get; set; }

        /// <summary>
        /// The number of seconds to refresh the camera image after. 0 to disable refreshing for this camera.
        /// </summary>
        public string RefreshRate { get; set; }

        /// <summary>
        /// The resolution of the image. This is set once the image has been loaded at least once.
        /// </summary>
        public string ImageResolution { get; set; }

        /// <summary>
        /// The file type of the image. Usually JPG.
        /// </summary>
        public ImageFileType ImageType { get; set; }

        #endregion

        // ===== Other info (metadata) ===== //

        #region Other info (metadata)

        /// <summary>
        /// The URL to the page that the camera image is from.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// The owner of the camera.
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// A link to the camera owner's profile somewhere.
        /// </summary>
        public string OwnerUrl { get; set; }

        /// <summary>
        /// The name of the location this camera is located at.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// The coordinations of the location this camera is located at. Used to take you to the location on Google Maps.
        /// </summary>
        public string LocationCoords { get; set; }

        #endregion

        // ===== Enums ===== //

        #region Enums

        /// <summary>
        /// The possible file types of an image.
        /// </summary>
        public enum ImageFileType
        {
            JPG,
            PNG
        }

        #endregion
    }
}
