using System;
using System.IO;

namespace ManagerUtilities.BackUp
{
    namespace DriveQuickstart
    {
        internal class Program
        {
            private static void Main(string[] args)
            {
                var service = new GoogleService();
                try
                {
                    var path = args[0];
                    if (!File.Exists(path))
                        throw new FileLoadException();

                    var fileName = Path.GetFileName(path);

                    using var sr = new StreamReader(path);
                    var message = service.UploadFile(sr.BaseStream, fileName, "application/zip");

                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception" + ex.Message);
                }
            }
        }
    }
}
