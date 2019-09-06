namespace Dinah.Core
{
    public static class Exe
    {
        /// <summary>
        /// <para>Location of Dinah.Core.dll</para>
        /// In almost all cases: USE THIS ONE instead of FileLocationRunning()
        /// </summary>
        public static string FileLocationOnDisk => System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");

        /// <summary>
        /// <para>DO NOT typically use this</para>
        /// Where the executing assembly is currently located, which may or may not be where the assembly is located when not executing. In the case of shadow copying assemblies, you will get a path in a temp directory
        /// </summary>
        public static string FileLocationRunning => System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("file:///", "");
    }
}
