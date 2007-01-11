#region BSD License Header

/*
 * Copyright (c) 2006, Oisin Grehan @ Nivot Inc (www.nivot.org)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
 * Neither the name of Nivot Incorporated nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

// MSDN Magazine January 2006 ; http://msdn.microsoft.com/msdnmag/issues/06/01/NETMatters/
// Credit: Stephen Toub - stoub@microsoft.com

using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Nivot.PowerShell.SharePoint.Commands
{
	internal class NativeMethods
	{
		#region DllImports

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(string lpFileName,
		                                                FileAccess dwDesiredAccess, FileShare dwShareMode,
		                                                IntPtr lpSecurityAttributes,
		                                                FileMode dwCreationDisposition, Int32 dwFlagsAndAttributes,
		                                                IntPtr hTemplateFile);

		[DllImport("kernel32.dll")]
		[return : MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupRead(SafeFileHandle hFile, IntPtr lpBuffer,
		                                      uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead,
		                                      [MarshalAs(UnmanagedType.Bool)] bool bAbort,
		                                      [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity,
		                                      ref IntPtr lpContext);

		[DllImport("kernel32.dll")]
		[return : MarshalAs(UnmanagedType.Bool)]
		private static extern bool BackupSeek(SafeFileHandle hFile,
		                                      uint dwLowBytesToSeek, uint dwHighBytesToSeek,
		                                      out uint lpdwLowByteSeeked, out uint lpdwHighByteSeeked,
		                                      ref IntPtr lpContext);

		#endregion

		internal static FileStream CreateFileStream(string path, FileAccess access, FileMode mode, FileShare share)
		{
			SafeFileHandle handle = CreateFile(path, access, share, IntPtr.Zero, mode, 0, IntPtr.Zero);
			if (handle.IsInvalid)
			{
				throw new IOException(String.Format("Could not open file stream '{0}'.", path));
			}
			return new FileStream(handle, access);
		}

		internal static string ReadAllText(string path)
		{
			using (StreamReader reader = new StreamReader(CreateFileStream(path, FileAccess.Read, FileMode.Open, FileShare.Read))
				)
			{
				return reader.ReadToEnd();
			}
		}

		internal static IEnumerable<StreamInfo> GetStreams(FileInfo file)
		{
			const int bufferSize = 4096;

			using (FileStream fs = file.OpenRead())
			{
				IntPtr context = IntPtr.Zero;
				IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

				try
				{
					while (true)
					{
						uint numRead;

						if (!BackupRead(fs.SafeFileHandle, buffer,
						                (uint) Marshal.SizeOf(typeof (Win32StreamID)),
						                out numRead, false, true, ref context))
						{
							throw new Win32Exception();
						}

						if (numRead > 0)
						{
							Win32StreamID streamID = (Win32StreamID) Marshal.PtrToStructure(
							                                         	buffer, typeof (Win32StreamID));
							string name = null;

							if (streamID.dwStreamNameSize > 0)
							{
								if (!BackupRead(fs.SafeFileHandle, buffer, (uint) Math.Min(bufferSize, streamID.dwStreamNameSize),
								                out numRead, false, true, ref context)) throw new Win32Exception();
								name = Marshal.PtrToStringUni(buffer, (int) numRead/2);
							}

							yield return new StreamInfo(name, streamID.dwStreamId, streamID.Size);

							if (streamID.Size > 0)
							{
								uint lo, hi;
								BackupSeek(fs.SafeFileHandle, uint.MaxValue, int.MaxValue, out lo, out hi, ref context);
							}
						}
						else
						{
							break;
						}
					}
				}
				finally
				{
					Marshal.FreeHGlobal(buffer);
					uint numRead;

					if (!BackupRead(fs.SafeFileHandle, IntPtr.Zero, 0, out numRead, true, false, ref context))
					{
						throw new Win32Exception();
					}
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Win32StreamID
	{
		public StreamType dwStreamId;
		public int dwStreamAttributes;
		public long Size;
		public int dwStreamNameSize;
		// WCHAR cStreamName[1]; 
	}

	public enum StreamType
	{
		Data = 1,
		ExternalData = 2,
		SecurityData = 3,
		AlternateData = 4,
		Link = 5,
		PropertyData = 6,
		ObjectID = 7,
		ReparseData = 8,
		SparseDock = 9
	}

	public struct StreamInfo
	{
		public StreamInfo(string name, StreamType type, long size)
		{
			Name = name;
			Type = type;
			Size = size;
		}

		public readonly string Name;
		public readonly StreamType Type;
		public readonly long Size;
	}

/*
 	public sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid {
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		private SafeFindHandle() : base(true) { }

		protected override bool ReleaseHandle() {
			return FindClose(this.handle);
		}

		[DllImport("kernel32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern bool FindClose(IntPtr handle);
	}

	public class FileStreamSearcher2003 {
		private const int ERROR_HANDLE_EOF = 38;
		private enum StreamInfoLevels { FindStreamInfoStandard = 0 }

		[DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern SafeFindHandle FindFirstStreamW(
			string lpFileName,
			StreamInfoLevels InfoLevel,
			[In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData,
			uint dwFlags);

		[DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FindNextStreamW(
			SafeFindHandle hndFindFile,
			[In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_STREAM_DATA lpFindStreamData);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private class WIN32_FIND_STREAM_DATA {
			public long StreamSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
			public string cStreamName;
		}

		public static IEnumerable<string> GetStreams(FileInfo file) {
			if (file == null) throw new ArgumentNullException("file");
			WIN32_FIND_STREAM_DATA findStreamData = new WIN32_FIND_STREAM_DATA();

			SafeFindHandle handle = FindFirstStreamW(file.FullName, StreamInfoLevels.FindStreamInfoStandard,
				findStreamData, 0);
			if (handle.IsInvalid) throw new Win32Exception();
			try {
				do { yield return findStreamData.cStreamName; }
				while (FindNextStreamW(handle, findStreamData));
				int lastError = Marshal.GetLastWin32Error();
				if (lastError != ERROR_HANDLE_EOF) throw new Win32Exception(lastError);
			} finally { handle.Dispose(); }
		}
	}
 */
}