using System.IO;
using System.Windows.Forms;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Utils
{
    internal class FilesHelper
    {
        public static bool AskToSave(ref string filename, string filter, string defaultExt, string initialDirectory = null)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = filter;
                saveDialog.DefaultExt = defaultExt;
                saveDialog.FileName = filename;

                // -- Optional initial directory
                if (initialDirectory != null) saveDialog.InitialDirectory = initialDirectory;

                DialogResult resultDialog = saveDialog.ShowDialog();
                filename = saveDialog.FileName;
                if (resultDialog != DialogResult.OK) return false;
            }

            return !File.Exists(filename) || !FileIsLocked(filename, FileAccess.ReadWrite);
            // MessageWindow.Show("Error", "The file is opened by another process, please close it and try again");
        }

        internal static bool FileIsLocked(string filename, FileAccess file_access)
        {
            if (!File.Exists(filename)) return false;

            // Try to open the file with the indicated access.
            try
            {
                var fs = new FileStream(filename, FileMode.Open, file_access);
                fs.Close();
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}