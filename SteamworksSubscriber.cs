/*
Copyright(C)2020 by graham chow <graham_chow@yahoo.com>

Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby granted.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
*/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Steamworks;

// no error handling exists to improve readability!

public class SteamworksSubscriber : MonoBehaviour
{
    public Dropdown CollectionDropdown;
    public Text CollectionText;

    #region Steam callbacks
    private Callback<DownloadItemResult_t> _downloadItemResult;

    private void OnEnable()
    {
        _downloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemCallback);
    }

    private void OnDisable()
    {
        if (_downloadItemResult != null)
        {
            _downloadItemResult.Dispose();
            _downloadItemResult = null;
        }
    }

    void OnDownloadItemCallback(DownloadItemResult_t result)
    {
        Debug.Log($"DownloadedItem {result.m_eResult}");
        ProcessSubscription(result.m_nPublishedFileId);
    }
    #endregion

    #region helpers


    void Start()
    {
        UpdateDropDown();
        OnCollectionChange();
    }

    public void OnCollectionChange()
    {
        if (CollectionDropdown.value >= CollectionDropdown.options.Count)
            CollectionText.text = "";
        else
        {
            string title = CollectionDropdown.options[CollectionDropdown.value].text;
            UGCHelper.ReadContent(title, out _, out _, out string content);
            CollectionText.text = content;
        }
    }

    void UpdateDropDown()
    {
        List<Dropdown.OptionData> od = new List<Dropdown.OptionData>();
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);
        foreach(DirectoryInfo content in di.EnumerateDirectories())
        {
            od.Add(new Dropdown.OptionData(content.Name));
        }
        CollectionDropdown.options = od;
    }

    #endregion

    public PublishedFileId_t[] GetSubscribedContent()
    {
        PublishedFileId_t[] all = new PublishedFileId_t[SteamUGC.GetNumSubscribedItems()];
        SteamUGC.GetSubscribedItems(all, (uint)all.Length);
        return all;
    }

    public void ProcessSubscription(PublishedFileId_t fileId)
    {
        if ((SteamUGC.GetItemState(fileId) & (uint)EItemState.k_EItemStateInstalled) != 0)
        {
            if (SteamUGC.GetItemInstallInfo(fileId, out _, out string folder, 1024, out _))
            {
                Debug.Log($"cache folder location {folder}");
                // read the content from Steam's cache of the content
                string cached_contents_filename = folder + Path.DirectorySeparatorChar + UGCHelper.ContentFileName;
                UGCHelper.ReadContentFromFile(cached_contents_filename, out string cached_title, out DateTime cached_timestamp, out ulong cached_steam_user_id, out _);
                string directory = Application.persistentDataPath + Path.DirectorySeparatorChar + cached_title;
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }
                string contents_filename = directory + Path.DirectorySeparatorChar + UGCHelper.ContentFileName;
                if (File.Exists(contents_filename))
                {
                    UGCHelper.ReadContentFromFile(contents_filename, out string title, out DateTime timestamp, out ulong steam_user_id, out _);
                    {
                        if (timestamp < cached_timestamp)
                        {
                            File.Delete(contents_filename);
                            File.Copy(cached_contents_filename, contents_filename);
                        }
                    }
                }
                else
                    File.Copy(cached_contents_filename, contents_filename);
                UpdateDropDown();
                OnCollectionChange();
            }
        }
    }
    public void OnSubscription()
    {
        PublishedFileId_t[] subscribed_content = GetSubscribedContent();

        // iterate through all the subscribed content
        foreach (PublishedFileId_t file_id in subscribed_content)
        {
            uint item_state = SteamUGC.GetItemState(file_id);
            if ((item_state & (uint)EItemState.k_EItemStateNeedsUpdate) != 0)
            {
                // make sure it is downloaded
                SteamUGC.DownloadItem(file_id, false);
            }
            else
            {
                // force a download
                SteamUGC.DownloadItem(file_id, false);
                //ProcessSubscription(file_id);
            }
        }
        UpdateDropDown();
        OnCollectionChange();
    }

}
