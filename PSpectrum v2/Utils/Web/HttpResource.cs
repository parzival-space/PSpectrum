using System;
using System.Text;

namespace PSpectrum.Utils.Web
{
    /// <summary>
    /// Represents a resource.
    /// </summary>
    public class HttpResource
    {
        public byte[] Data;
        public Encoding Encoding;
        public string Type;

        /// <summary>
        /// Creates a new text resource.
        /// </summary>
        /// <param name="data">Your data. Duh!</param>
        /// <param name="type">Something like 'text/html'</param>
        public HttpResource(string data, string type)
        {
            this.Type = type;
            this.Encoding = Encoding.UTF8;
            this.Data = Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// Creates a basic resource.
        /// </summary>
        /// <param name="data">Your binary data.</param>
        /// <param name="type">The content type. Something like 'image/png'.</param>
        public HttpResource(byte[] data, string type)
        {
            this.Type = type;
            this.Data = data;
            this.Encoding = null;
        }

        /// <summary>
        /// Creates a new image reource.
        /// </summary>
        /// <param name="data">Your binary data.</param>
        /// <param name="type">The image type. Something like PNG or JPEG.</param>
        /// <returns>The resource object.</returns>
        public static HttpResource CreateImage(byte[] image, string type)
        {
            return new HttpResource(image, "image/" + Enum.GetName(type.GetType(), type).ToLower());
        }

        /// <summary>
        /// Creates a new text resource.
        /// </summary>
        /// <param name="data">Your data. Duh!</param>
        /// <param name="type">Something like HTML or CSS.</param>
        /// <returns>The resource object.</returns>
        public static HttpResource CreateText(string text, TextResource type)
        {
            return new HttpResource(text, "text/" + Enum.GetName(type.GetType(), type).ToLower());
        }

        /// <summary>
        /// Represents the types of text resources.
        /// </summary>
        public enum TextResource
        {
            CSS,
            HTML,
            JAVASCRIPT,
            XML
        }

        /// <summary>
        /// Represents the types of image resources.
        /// </summary>
        public enum ImageResource
        {
            PNG,
            JPEG,
            GIF,
            WEBP,
            APNG
        }
    }
}