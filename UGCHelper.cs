/*
Copyright(C)2020 by graham chow <graham_chow@yahoo.com>

Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

/// <summary>
/// This static class contains a collection of helper function mainly for writing and reading content data
/// No error handling exists to improve readability! 
/// </summary>
public static class UGCHelper
{
    public static readonly string ContentFileName = "content.txt";
    public static string GetContentDirectory(string title)
    {
        return Application.persistentDataPath + Path.DirectorySeparatorChar + title;
    }

    static string GetContentFileName(string title)
    {
        return GetContentDirectory(title) + Path.DirectorySeparatorChar + ContentFileName;
    }

    public static void WriteContent(ulong fileId, string title, DateTime timestamp, ulong steamUserId, string content)
    {
        // using a non ideal separater character just for simplicity
        Debug.Assert(content.Contains("#") == false);
        Debug.Assert(title.Contains("#") == false);
        string directory = GetContentDirectory(title);
        if (Directory.Exists(directory) == false)
            Directory.CreateDirectory(directory);
        string encoded = $"{fileId}#{title}#{timestamp.Ticks}#{steamUserId}#{content}";
        Debug.Log(encoded);
        File.WriteAllText(GetContentFileName(title), encoded);
    }

    public static ulong ReadContent(string title, out DateTime timestamp, out ulong steamUserId, out string content)
    {
        string directory = GetContentDirectory(title);
        if (Directory.Exists(directory) == false)
        {
            content = null;
            steamUserId = 0;
            timestamp = DateTime.MinValue;
            return 0;
        }
        return ReadContentFromFile(GetContentFileName(title), out _, out timestamp, out steamUserId, out content);
    }

    public static ulong ReadContentFromFile(string filename, out string title, out DateTime timestamp, out ulong steamUserId, out string content)
    {
        string encoded = File.ReadAllText(filename);
        string [] body = encoded.Split('#');
        steamUserId = System.Convert.ToUInt64(body[3]);
        title = body[1];
        timestamp = new DateTime(Convert.ToInt64(body[2]));
        content = body[4];
        return System.Convert.ToUInt64(body[0]);
    }

}
