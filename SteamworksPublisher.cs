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
using Steamworks;



/// <summary>
/// This class is used to publish user generated content.
/// The content is just simple text, but it could be any collection of files.
/// no error handling exists to improve readability!
/// </summary>
public class SteamworksPublisher : MonoBehaviour
{
    // UI
    public InputField TitleInputField;
    public InputField ContentInputField;

    // Steam callbacks
    #region Steam callbacks
    private CallResult<CreateItemResult_t> _createItemResult;
    private CallResult<SubmitItemUpdateResult_t> _submitItemUpdateResult;
    //private CallResult<DeleteItemResult_t> _deleteItemResult;
    //private CallResult<RemoteStorageUnsubscribePublishedFileResult_t> _unsubscribeItemResult;

    private void OnEnable()
    {
        _createItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
        _submitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
        //_deleteItemResult = CallResult<DeleteItemResult_t>.Create(OnDeleteItemResult);
        //_unsubscribeItemResult = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create(OnUnsubscribedItemResult);
    }

    private void OnDisable()
    {
        if (_createItemResult != null)
        {
            _createItemResult.Dispose();
            _createItemResult = null;
        }
        if (_submitItemUpdateResult != null)
        {
            _submitItemUpdateResult.Dispose();
            _submitItemUpdateResult = null;
        }
        //if (_deleteItemResult != null)
        //{
        //    _deleteItemResult.Dispose();
        //    _deleteItemResult = null;
        //}
        //if (_unsubscribeItemResult != null)
        //{
        //    _unsubscribeItemResult.Dispose();
        //    _unsubscribeItemResult = null;
        //}
    }

    void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
    {
        Debug.Log($"CreateItem {pCallback.m_eResult}");
        if (pCallback.m_eResult == EResult.k_EResultOK)
        {
            // if the results are OK, then just update the content
            string title = TitleInputField.text;
            UGCHelper.WriteContent(pCallback.m_nPublishedFileId.m_PublishedFileId, title, DateTime.UtcNow, SteamUser.GetSteamID().m_SteamID, ContentInputField.text);
            UpdateItem(pCallback.m_nPublishedFileId, title, UGCHelper.GetContentDirectory(title));
        }
    }

    void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
    {
        Debug.Log($"SubmitItemUpdate {pCallback.m_eResult}");
    }

    //void OnDeleteItemResult(DeleteItemResult_t pCallback, bool bIOFailure)
    //{
    //    Debug.Log($"DeleteItem {pCallback.m_eResult}");
    //}

    //void OnUnsubscribedItemResult(RemoteStorageUnsubscribePublishedFileResult_t pCallback, bool bIOFailure)
    //{
    //    Debug.Log($"UnsubscribedItem {pCallback.m_eResult}");
    //}

    #endregion

    void CreateItem()
    {
        // just create an item (file id)
        SteamAPICall_t handle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        _createItemResult.Set(handle);
    }

    //public void DeleteItem(string title)
    //{
    //    ulong file_id = UGCHelper.ReadContent(title, out _, out _, out _);
    //    if (file_id != 0)
    //    {
    //        PublishedFileId_t sfileId = new PublishedFileId_t(file_id);
    //        SteamAPICall_t handle = SteamUGC.DeleteItem(sfileId);
    //        _deleteItemResult.Set(handle);
    //    }
    //}

    void UpdateItem(PublishedFileId_t fileId, string title, string contentDirectory)
    {
        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), fileId);
        // there are heaps of attributes to set here, like preview image and description
        SteamUGC.SetItemTitle(handle, title);
        SteamUGC.SetItemContent(handle, contentDirectory);

        SteamAPICall_t update_handle = SteamUGC.SubmitItemUpdate(handle, "a super dooper amazing next update :)");
        _submitItemUpdateResult.Set(update_handle);
    }


    // pushing the publish button should call this
    public void OnPublish()
    {
        string title = TitleInputField.text;
        ulong file_id = UGCHelper.ReadContent(title, out _, out _, out _);
        if (file_id == 0)   // if there is no file id, then create one
            CreateItem(); // after creating, the callback will update item.
        else
        {
            // file id exists, just update item
            UGCHelper.WriteContent(file_id, title, DateTime.UtcNow, SteamUser.GetSteamID().m_SteamID, ContentInputField.text);
            UpdateItem(new PublishedFileId_t(file_id), title, UGCHelper.GetContentDirectory(title));
        }
    }
}
