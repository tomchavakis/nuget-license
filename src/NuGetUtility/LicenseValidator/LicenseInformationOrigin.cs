namespace NuGetUtility.LicenseValidator
{
    public enum LicenseInformationOrigin
    {
        /// <summary>
        /// The license information was provided by the package maintainer using a license expression
        /// </summary>
        Expression,

        /// <summary>
        /// The license information was provided by the package maintainer using a license url
        /// that was then matched against a set of known license url's
        /// </summary>
        Url,

        /// <summary>
        /// The license has an unknown origin. This is always used in conjunction with a licensing error
        /// which will give more information
        /// </summary>
        Unknown,

        /// <summary>
        /// The license for this package was ignored via the ignore list provided
        /// </summary>
        Ignored,

        /// <summary>
        /// The license for this package was given by a custom override
        /// </summary>
        Overwrite
    }
}
