using System.IO;
using System.Text;

namespace Crawl.Helpers
{
    public static class FileHelper
    {
        public static void WriteFile(string path, string data)
        {
            FileStream fs = new FileStream(path, FileMode.Append);
            StreamWriter sWriter = new StreamWriter(fs, Encoding.UTF8);
            sWriter.WriteLine(data);
            sWriter.Flush();
            fs.Close();
        }
    }
}
