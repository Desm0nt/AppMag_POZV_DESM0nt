using AppLib;
using System.Collections.Generic;
using System.IO;

namespace EdgeDetectorsLib
{
    public class DicomFolderManager
    {
        string _loadFolder;
        string _resultFolder;

        private readonly Dictionary<string,List<NewLayer>> _data;
        
        public DicomFolderManager(string loadFolder, string resultFolder)
        {
            _data = new Dictionary<string, List<NewLayer>>();
            _loadFolder = Path.GetFullPath(Path.Combine(loadFolder, @"..\..\..\..\", @"Resources", loadFolder));
            _resultFolder = Path.GetFullPath(Path.Combine(resultFolder, @"..\..\..\..\", @"Resources", resultFolder));
            LoadFiles();

        }

        private void LoadFiles()
        {
            if (Directory.Exists(_loadFolder))
            {
                foreach (string directory in Directory.GetDirectories(_loadFolder, "*", SearchOption.TopDirectoryOnly))
                {

                    var list = new List<NewLayer>();
                    foreach (string file in Directory.GetFiles(_loadFolder))
                    {
                        list.Add(new NewLayer(file));
                    }
                    _data.Add(directory, list);
                }
            }

            //    foreach (string file in Directory.GetFiles(_loadFolder))
            //    {
            //        
            //        
            //        _layers.Add(GetLayer(file));
            //    }

        }

        //protected abstract void Detect();

        private void SaveFiles()
        { }
    }
}
