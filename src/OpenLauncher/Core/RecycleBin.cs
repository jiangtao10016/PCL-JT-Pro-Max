using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenLauncher.Core
{
    /// <summary>删除到回收站（给用户反悔的机会）。用 SHFileOperation 的 P/Invoke，不依赖 Microsoft.VisualBasic。</summary>
    public static class RecycleBin
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_NOERRORUI = 0x0400;
        private const ushort FOF_SILENT = 0x0004;

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        /// <summary>把文件或文件夹送到回收站；失败时退回直接删除。</summary>
        public static void Delete(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return;

            var fos = new SHFILEOPSTRUCT
            {
                wFunc = FO_DELETE,
                // SHFileOperation 要求以两个 \0 结尾的路径列表
                pFrom = path + "\0",
                fFlags = (ushort)(FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT),
            };

            int rc = SHFileOperation(ref fos);
            if (rc != 0)
            {
                // 回收站不可用（如网络盘）时退回硬删除
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    else if (Directory.Exists(path)) Directory.Delete(path, true);
                }
                catch { /* 忽略：交给上层处理 */ }
            }
        }
    }
}
