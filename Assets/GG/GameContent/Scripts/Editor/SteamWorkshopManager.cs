using GG.BeanBattles.MapEditor;
using Steamworks;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GG.BeanBattles
{
    public struct SteamWorkshopUploadResult
    {
        public bool Success;
        public string SteamItemId;
        public string SteamAuthorId;
    }

    public static class SteamWorkshopManager
    {
        private static CallResult<SubmitItemUpdateResult_t> _submitItemUpdate;
        private static CallResult<CreateItemResult_t> _createItemResult;

        private static bool _steamInitialized = false;

        private static string MAP_JSON = "map.json";
        private static string MAP_BUNDLE = "map.bundle";
        private static string MAP_PREVIEW = "preview.png";

        public static SteamWorkshopUploadResult PublishMap(string rootPath)
        {
            SteamWorkshopUploadResult result = new SteamWorkshopUploadResult();

            if (!_steamInitialized)
            {
                if (!SteamAPI.Init()) { Debug.LogError("Steam init failed."); return result; }
                _steamInitialized = true;
            }

            if (rootPath == null) { Debug.LogError("rootPath is null."); return result; }

            string jsonPath = Path.Combine(rootPath, MAP_JSON);
            string bundlePath = Path.Combine(rootPath, MAP_BUNDLE);
            string previewPath = Path.Combine(rootPath, MAP_PREVIEW);

            if (!File.Exists(jsonPath)) { Debug.LogError($"Missing json: {jsonPath}"); return result; }
            if (!File.Exists(bundlePath)) { Debug.LogError($"Missing bundle: {bundlePath}"); return result; }
            if (!File.Exists(previewPath)) { Debug.LogError($"Missing preview: {previewPath}"); return result; }

            string json = File.ReadAllText(jsonPath);
            EditorMapMetaData metaData = JsonUtility.FromJson<EditorMapMetaData>(json);

            PublishedFileId_t workshopId = new PublishedFileId_t();

            if (string.IsNullOrEmpty(metaData.SteamItemId))
            {
                Debug.Log("Creating Workshop Item...");

                bool? createDone = null;

                SteamAPICall_t createHandle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);

                _createItemResult = CallResult<CreateItemResult_t>.Create((r, failure) =>
                {
                    if (failure || r.m_eResult != EResult.k_EResultOK)
                    {
                        Debug.LogError($"CreateItem failed: {r.m_eResult}");
                        createDone = false;
                        return;
                    }
                    workshopId = r.m_nPublishedFileId;
                    createDone = true;
                });
                _createItemResult.Set(createHandle);
                if (!WaitUntilDone(ref createDone, "Creating workshop item...", 2f))

                if (createDone == false) { Debug.LogError("Failed to create workshop item."); return result; }

                metaData.SteamItemId = workshopId.m_PublishedFileId.ToString();
                metaData.SteamAuthorId = SteamUser.GetSteamID().m_SteamID.ToString();
                File.WriteAllText(jsonPath, JsonUtility.ToJson(metaData));

                Debug.Log("Created Workshop Item: " + metaData.SteamItemId);
            }
            else
            {
                workshopId = new PublishedFileId_t(ulong.Parse(metaData.SteamItemId));
                Debug.Log("Updating existing Workshop Item: " + workshopId);
            }

            Debug.Log("Updating Workshop Item...");

            bool? updateDone = null;

            UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), workshopId);
            SteamUGC.SetItemTitle(updateHandle, metaData.MapName);
            SteamUGC.SetItemDescription(updateHandle, metaData.Description);
            SteamUGC.SetItemPreview(updateHandle, Path.GetFullPath(previewPath));
            SteamUGC.SetItemContent(updateHandle, Path.GetFullPath(rootPath));
            SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);

            SteamAPICall_t submitHandle = SteamUGC.SubmitItemUpdate(updateHandle, "Map updated");

            _submitItemUpdate = CallResult<SubmitItemUpdateResult_t>.Create((r, failure) =>
            {
                if (failure || r.m_eResult != EResult.k_EResultOK)
                {
                    Debug.LogError($"SubmitItemUpdate failed: {r.m_eResult}");
                    Debug.LogError($"Needs legal agreement: {r.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
                    updateDone = false;
                    return;
                }
                updateDone = true;
            });

            _submitItemUpdate.Set(submitHandle);
            if (!WaitUntilDone(ref updateDone, "Updating workshop item...", 5f))

            if (updateDone == false) { Debug.LogError("Workshop update failed."); return result; }

            Debug.Log($"Workshop update complete: {metaData.MapName}");

            result.Success = true;
            result.SteamItemId = workshopId.m_PublishedFileId.ToString();
            result.SteamAuthorId = SteamUser.GetSteamID().m_SteamID.ToString();
            return result;
        }

        private static bool WaitUntilDone(ref bool? done, string progressLabel, float usualSeconds, float timeoutSeconds = 60f)
        {
            float elapsed = 0f;
            while (done == null)
            {
                SteamAPI.RunCallbacks();
                System.Threading.Thread.Sleep(100);
                elapsed += 0.1f;

                // usualSeconds first 90%, rest of timeout maps
                // we can't get actual progress
                float progress;
                if (elapsed < usualSeconds) progress = (elapsed / usualSeconds) * 0.9f;
                else progress = 0.9f + ((elapsed - usualSeconds) / (timeoutSeconds - usualSeconds)) * 0.1f;

                EditorUtility.DisplayProgressBar("Steam Workshop", progressLabel, progress);

                if (elapsed >= timeoutSeconds)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError($"Steam operation timed out after {timeoutSeconds}s: {progressLabel}");
                    return false;
                }
            }

            EditorUtility.ClearProgressBar();
            return done == true;
        }
    }
}