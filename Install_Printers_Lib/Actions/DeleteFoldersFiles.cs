using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Install_Printers_Lib_core.Actions
{
    public class DeleteFoldersFiles
    {
        private string _deletePath;

        private List<string> _deletePaths;

        private bool _folder;

        public DeleteFoldersFiles(string deletePath, bool folder)
        {
            _deletePath = deletePath;

            _folder = folder;
        }

        public DeleteFoldersFiles(List<string> deletePaths, bool folder)
        {
            _deletePaths = deletePaths;

            _folder = folder;
        }

        private void DeleteFile()
        {
            try
            {
                File.Delete(_deletePath);
            }
            catch
            {
                throw new Exception("Ошибка удаления. Проверьте правильность введенного пути.");
            }
        }

        private void DeleteFiles()
        {
            foreach (var p in _deletePaths)
            {
                try
                {
                    File.Delete(p);
                }
                catch
                {
                    throw new Exception("Ошибка удаления. Проверьте правильность введенного пути.");
                }
            }
        }

        private void DeleteFolder()
        {
            try
            {
                Directory.Delete(_deletePath, true);
            }
            catch
            {
                throw new Exception("Ошибка удаления. Проверьте правильность введенного пути.");
            }
        }

        private bool DeleteFolders()
        {
            foreach (var p in _deletePaths)
            {
                try
                {
                    Directory.Delete(p, true);
                }
                catch
                {
                    throw new Exception("Ошибка удаления. Проверьте правильность введенного пути.");
                }
            }

            return true;
        }

        public bool StartDelete()
        {
            if (!_folder && _deletePaths == null)
            {
                try
                {
                    DeleteFile();

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else if(!_folder && _deletePaths != null)
            {
                try
                {
                    DeleteFiles();

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else if(_folder && _deletePaths == null)
            {
                try
                {
                    DeleteFolder();

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            else
            {
                try
                {
                    DeleteFolders();

                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
