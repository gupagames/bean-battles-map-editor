using GG.BeanBattles.MapEditor;
using Steamworks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BeanBattlesMapEditorSteamUploader
{
    public static class SteamWorkshopManager
    {
        private static CallResult<SubmitItemUpdateResult_t> _submitItemUpdate;
        private static CallResult<CreateItemResult_t> _createItemResult;

        private static string MAP_JSON = "map.json";
        private static string MAP_BUNDLE = "map.bundle";
        private static string MAP_PREVIEW = "preview.png";

        public static async Task<bool> PublishMapAsync(string rootPath)
        {
            if (rootPath == null) return false;

            string jsonPath = Path.Combine(rootPath, MAP_JSON);
            string bundlePath = Path.Combine(rootPath, MAP_BUNDLE);
            string previewPath = Path.Combine(rootPath, MAP_PREVIEW);

            if (!File.Exists(jsonPath)) { Console.WriteLine($"Missing json: {jsonPath}"); return false; }
            if (!File.Exists(bundlePath)) { Console.WriteLine($"Missing bundle: {bundlePath}"); return false; }
            if (!File.Exists(previewPath)) { Console.WriteLine($"Missing preview: {previewPath}"); return false; }

            string json = File.ReadAllText(jsonPath);
            EditorMapMetaData metaData = Newtonsoft.Json.JsonConvert.DeserializeObject<EditorMapMetaData>(json);

            bool creatingNewItem = string.IsNullOrEmpty(metaData.SteamItemId);

            PublishedFileId_t workshopId;

            if (creatingNewItem)
            {
                Console.WriteLine("Creating Workshop Item...");
                workshopId = await CreateWorkshopItemAsync();

                if (workshopId.m_PublishedFileId == 0)
                { Console.WriteLine("Failed to create workshop item."); return false; }

                metaData.SteamItemId = workshopId.m_PublishedFileId.ToString();
                metaData.SteamAuthorId = SteamUser.GetSteamID().m_SteamID.ToString();

                string updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(metaData, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonPath, updatedJson);

                Console.WriteLine("Created Workshop Item");
            }
            else
            {
                Console.WriteLine("Updating existing Workshop Item");
                workshopId = new PublishedFileId_t(ulong.Parse(metaData.SteamItemId));
            }

            Console.WriteLine("Updating Workshop Item...");
            bool success = await UploadWorkshopItemAsync(workshopId, rootPath, previewPath, metaData);

            if (success) Console.WriteLine($"Workshop upload complete: {metaData.MapName}");
            else Console.WriteLine("Workshop upload failed.");

            return success;
        }

        private static async Task<PublishedFileId_t> CreateWorkshopItemAsync()
        {
            var tcs = new TaskCompletionSource<PublishedFileId_t>();

            SteamAPICall_t handle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);

            _createItemResult = CallResult<CreateItemResult_t>.Create();

            _createItemResult.Set(handle, (result, failure) =>
            {
                if (failure || result.m_eResult != EResult.k_EResultOK)
                {
                    Console.WriteLine($"CreateItem failed: {result.m_eResult}");
                    tcs.SetResult(new PublishedFileId_t());
                    return;
                }

                tcs.SetResult(result.m_nPublishedFileId);
            });

            return await tcs.Task;
        }

        private static async Task<bool> UploadWorkshopItemAsync(PublishedFileId_t fileId, string rootPath, string previewPath, EditorMapMetaData metaData)
        {
            var tcs = new TaskCompletionSource<bool>();

            UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), fileId);

            string cleanPath = Path.GetFullPath(rootPath);
            string cleanPreview = Path.GetFullPath(previewPath);

            SteamUGC.SetItemTitle(updateHandle, metaData.MapName);
            SteamUGC.SetItemDescription(updateHandle, metaData.Description);
            SteamUGC.SetItemPreview(updateHandle, cleanPreview);
            SteamUGC.SetItemContent(updateHandle, cleanPath);
            SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);

            SteamAPICall_t submitHandle = SteamUGC.SubmitItemUpdate(updateHandle, "Map updated");

            _submitItemUpdate = CallResult<SubmitItemUpdateResult_t>.Create();

            _submitItemUpdate.Set(submitHandle, (result, failure) =>
            {
                if (failure || result.m_eResult != EResult.k_EResultOK)
                {
                    Console.WriteLine($"SubmitItemUpdate failed: {result.m_eResult}");
                    Console.WriteLine($"Needs legal agreement: {result.m_bUserNeedsToAcceptWorkshopLegalAgreement}");
                    tcs.SetResult(false);
                    return;
                }
                tcs.SetResult(true);
            });

            return await tcs.Task;
        }
    }
}