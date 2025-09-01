using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mlie;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse;

namespace SaveModlistAsCollection;

[StaticConstructorOnStartup]
internal class SaveModlistAsCollectionMod : Mod
{
    private const string NewCollectionUrl = "https://steamcommunity.com/workshop/editcollection/?appid=294100";
    public static Vector2 ScrollPosition;
    public static List<string> StatusMessages;
    public static bool ActivityMonitor;
    private static UGCQueryHandle_t mUgcQueryHandle;
    private static CallResult<SteamUGCQueryCompleted_t> onSteamUgcQueryCompletedCallResult;
    private static SteamUGCQueryCompleted_t collectionQueryResult;
    private static PublishedFileId_t lastModInCollection;
    private static RemoveUGCDependencyResult_t removeUgcDependencyResult;
    private static CallResult<RemoveUGCDependencyResult_t> onRemoveUgcDependencyCompletedCallResult;
    private static AddUGCDependencyResult_t addUgcDependencyResult;
    private static CallResult<AddUGCDependencyResult_t> onAddUgcDependencyCompletedCallResult;
    public static List<CollectionInfo> UsersCollections;

    public static bool Updating;

    private static string currentVersion;


    private static ModMetaData thisMod;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public SaveModlistAsCollectionMod(ModContentPack content) : base(content)
    {
        thisMod = content.ModMetaData;
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Save Modlist As Collection";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);

        if (listingStandard.ButtonTextLabeled("SMAC.new".Translate(), "SMAC.create".Translate()))
        {
            Application.OpenURL(NewCollectionUrl);
        }

        if (listingStandard.ButtonTextLabeled("SMAC.SaveModlist".Translate(), "Save".Translate()))
        {
            UsersCollections = SteamCollectionsFetcher.GetUserCollections();
            if (!UsersCollections.Any())
            {
                Messages.Message("SMAC.noCollections".Translate(), MessageTypeDefOf.RejectInput);
            }
            else
            {
                Find.WindowStack.Add(new Dialog_ModCollections());
            }
        }

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("SMAC.modVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public static void UpdateCollection(CollectionInfo collection)
    {
        if (Updating)
        {
            return;
        }

        Updating = true;
        StatusMessages = [];

        try
        {
            // Fetch all currently active mods that are published on steam
            var activeMods = ModsConfig.ActiveModsInLoadOrder
                .Where(mod => !mod.IsCoreMod && mod != thisMod && mod.GetPublishedFileId() != PublishedFileId_t.Invalid)
                .ToList();

            switch (activeMods.Count)
            {
                case 0:
                    StatusMessages.Add("SMAC.noActiveMods".Translate());
                    StatusMessages.Add("SMAC.failed".Translate());
                    Updating = false;
                    return;
                case > 1000:
                    StatusMessages.Add("SMAC.tooManyMods".Translate());
                    StatusMessages.Add("SMAC.failed".Translate());
                    Updating = false;
                    return;
            }

            StatusMessages.Add("SMAC.foundMods".Translate(activeMods.Count));
            lastModInCollection = PublishedFileId_t.Invalid;
            collectionQueryResult = new SteamUGCQueryCompleted_t();
            onSteamUgcQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
            onRemoveUgcDependencyCompletedCallResult =
                CallResult<RemoveUGCDependencyResult_t>.Create(OnRemoveUGCDependencyCompleted);
            onAddUgcDependencyCompletedCallResult =
                CallResult<AddUGCDependencyResult_t>.Create(OnAddUGCDependencyCompleted);

            // Remove all existing items from the collection
            PublishedFileId_t[] publishedFileIDs = [collection.CollectionId];
            mUgcQueryHandle =
                SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIDs, (uint)publishedFileIDs.Length);
            SteamUGC.SetReturnChildren(mUgcQueryHandle, true);
            var createQueryUgcDetailsRequest = SteamUGC.SendQueryUGCRequest(mUgcQueryHandle);
            onSteamUgcQueryCompletedCallResult.Set(createQueryUgcDetailsRequest);
            StatusMessages.Add("SMAC.fetchingExisting".Translate());
            while (collectionQueryResult.m_eResult == EResult.k_EResultNone)
            {
                Thread.Sleep(500);
                SteamAPI.RunCallbacks();
                ActivityMonitor = !ActivityMonitor;
            }

            var removeOne = true;
            if (collection.CollectionItems > 0)
            {
                var publishedFileIDArray = new PublishedFileId_t[collection.CollectionItems];
                var success = SteamUGC.GetQueryUGCChildren(collectionQueryResult.m_handle, 0, publishedFileIDArray,
                    (uint)publishedFileIDArray.Length);
                if (!success)
                {
                    StatusMessages.Add("SMAC.failed".Translate());
                    Updating = false;
                    return;
                }

                StatusMessages.Add("SMAC.removingExisting".Translate(collection.CollectionItems));
                lastModInCollection = publishedFileIDArray[^1];

                mUgcQueryHandle =
                    SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIDArray, (uint)publishedFileIDArray.Length);
                createQueryUgcDetailsRequest = SteamUGC.SendQueryUGCRequest(mUgcQueryHandle);
                onSteamUgcQueryCompletedCallResult.Set(createQueryUgcDetailsRequest);
                collectionQueryResult = new SteamUGCQueryCompleted_t();
                while (collectionQueryResult.m_eResult == EResult.k_EResultNone)
                {
                    Thread.Sleep(500);
                    SteamAPI.RunCallbacks();
                    ActivityMonitor = !ActivityMonitor;
                }

                for (uint index = 0; index < publishedFileIDArray.Length; index++)
                {
                    var publishedFileIdT = publishedFileIDArray[index];
                    if (SteamUGC.GetQueryUGCResult(collectionQueryResult.m_handle, index, out var details) &&
                        details.m_eFileType != EWorkshopFileType.k_EWorkshopFileTypeCommunity)
                    {
                        Log.Message(
                            $"[SaveModlistAsCollection]: {details.m_rgchTitle} is not a mod, skipping");
                        if (lastModInCollection == publishedFileIdT)
                        {
                            removeOne = false;
                        }

                        continue;
                    }

                    if (index == publishedFileIDArray.Length - 1)
                    {
                        continue;
                    }

                    removeUgcDependencyResult = new RemoveUGCDependencyResult_t();
                    var removeDependencyHandle = SteamUGC.RemoveDependency(collection.CollectionId, publishedFileIdT);
                    onRemoveUgcDependencyCompletedCallResult.Set(removeDependencyHandle);
                    while (removeUgcDependencyResult.m_eResult == EResult.k_EResultNone)
                    {
                        Thread.Sleep(5);
                        SteamAPI.RunCallbacks();
                        ActivityMonitor = !ActivityMonitor;
                    }

                    if (removeUgcDependencyResult.m_eResult == EResult.k_EResultOK)
                    {
                        continue;
                    }

                    StatusMessages.Add("SMAC.removalFailed".Translate(publishedFileIdT.m_PublishedFileId.ToString(),
                        removeUgcDependencyResult.m_eResult.ToString()));
                    StatusMessages.Add("SMAC.failed".Translate());
                    Updating = false;
                    return;
                }
            }

            StatusMessages.Add("SMAC.adding".Translate(activeMods.Count));
            var indexToStartFrom = 0;
            if (collection.CollectionItems > 0 && lastModInCollection.m_PublishedFileId ==
                activeMods[0].GetPublishedFileId().m_PublishedFileId)
            {
                indexToStartFrom = 1;
            }

            for (var index = indexToStartFrom; index < activeMods.Count; index++)
            {
                var mod = activeMods[index];
                var id = mod.GetPublishedFileId().m_PublishedFileId;
                var modFileId = new PublishedFileId_t(id);
                addUgcDependencyResult = new AddUGCDependencyResult_t();
                var addDependencyHandle = SteamUGC.AddDependency(collection.CollectionId, modFileId);
                onAddUgcDependencyCompletedCallResult.Set(addDependencyHandle);
                while (addUgcDependencyResult.m_eResult == EResult.k_EResultNone)
                {
                    Thread.Sleep(5);
                    SteamAPI.RunCallbacks();
                    ActivityMonitor = !ActivityMonitor;
                }

                // Removing the previous left mod if it was not already the first
                if (removeOne && collection.CollectionItems > 0 && index == 0 &&
                    lastModInCollection != PublishedFileId_t.Invalid)
                {
                    SteamUGC.RemoveDependency(collection.CollectionId, lastModInCollection);
                }
            }

            StatusMessages.Add("SMAC.collectionUpdated".Translate(collection.CollectionName));
            Updating = false;
        }
        catch (Exception e)
        {
            StatusMessages.Add(e.Message);
            StatusMessages.Add("SMAC.failed".Translate());
            Updating = false;
        }
    }

    private static void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
    {
        collectionQueryResult = pCallback;
    }

    private static void OnRemoveUGCDependencyCompleted(RemoveUGCDependencyResult_t pCallback, bool bIOFailure)
    {
        removeUgcDependencyResult = pCallback;
    }

    private static void OnAddUGCDependencyCompleted(AddUGCDependencyResult_t pCallback, bool bIOFailure)
    {
        addUgcDependencyResult = pCallback;
    }
}