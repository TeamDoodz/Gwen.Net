﻿using System.Collections.Generic;
using System.IO;

namespace Gwen.Net.Platform;
public class GwenPlatform {
	private static IPlatform? platform = null;

	/// <summary>
	/// Set the current platform.
	/// </summary>
	/// <param name="platform">Platform.</param>
	public static void Init(IPlatform platform) {
		GwenPlatform.platform = platform;
	}

	/// <summary>
	/// Gets text from clipboard.
	/// </summary>
	/// <returns>Clipboard text.</returns>
	public static string GetClipboardText() {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetClipboardText();
	}

	/// <summary>
	/// Sets the clipboard text.
	/// </summary>
	/// <param name="text">Text to set.</param>
	/// <returns>True if succeeded.</returns>
	public static bool SetClipboardText(string text) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.SetClipboardText(text);
	}


	/// <summary>
	/// Gets elapsed time since this class was initalized.
	/// </summary>
	/// <returns>Time interval in seconds.</returns>
	public static float GetTimeInSeconds() {
		System.Diagnostics.Debug.Assert(platform != null);

		return (float)platform.GetTimeInSeconds();
	}

	/// <summary>
	/// Changes the mouse cursor.
	/// </summary>
	/// <param name="cursor">Cursor type.</param>
	public static void SetCursor(Cursor cursor) {
		System.Diagnostics.Debug.Assert(platform != null);

		platform.SetCursor(cursor);
	}

	/// <summary>
	/// Get special folders of the system.
	/// </summary>
	/// <returns>List of folders.</returns>
	public static IEnumerable<ISpecialFolder> GetSpecialFolders() {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetSpecialFolders();
	}

	public static string GetFileName(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetFileName(path);
	}

	public static string? GetDirectoryName(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetDirectoryName(path);
	}

	public static bool FileExists(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.FileExists(path);
	}

	public static bool DirectoryExists(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.DirectoryExists(path);
	}

	public static void CreateDirectory(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		platform.CreateDirectory(path);
	}

	public static string Combine(string path1, string path2) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.Combine(path1, path2);
	}

	public static string Combine(string path1, string path2, string path3) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.Combine(path1, path2, path3);
	}

	public static string Combine(string path1, string path2, string path3, string path4) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.Combine(path1, path2, path3, path4);
	}

	public static string CurrentDirectory {
		get {
			System.Diagnostics.Debug.Assert(platform != null);

			return platform.CurrentDirectory;
		}
	}

	public static IEnumerable<IFileSystemDirectoryInfo> GetDirectories(string path) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetDirectories(path);
	}

	public static IEnumerable<IFileSystemFileInfo> GetFiles(string path, string filter) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetFiles(path, filter);
	}

	public static Stream GetFileStream(string path, bool isWritable) {
		System.Diagnostics.Debug.Assert(platform != null);

		return platform.GetFileStream(path, isWritable);
	}
}