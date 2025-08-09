using System.Collections.Generic;
using System.Threading;
using Steamworks;
using Verse;
using Verse.Steam;

namespace SaveModlistAsCollection;

public static class SteamCollectionsFetcher
{
    private const int RimworldAppINT = 294100;
    private static CallResult<SteamUGCQueryCompleted_t> onSteamUgcQueryCompletedCallResult;
    private static SteamUGCQueryCompleted_t collectionQueryResult;

    public static List<CollectionInfo> GetUserCollections()
    {
        var collections = new List<CollectionInfo>();

        if (!SteamManager.Initialized)
        {
            Log.Error("Steam is not initialized.");
            return collections;
        }

        collectionQueryResult.m_eResult = EResult.k_EResultNone;

        var queryHandle = SteamUGC.CreateQueryUserUGCRequest(
            SteamUser.GetSteamID().GetAccountID(), // User's Steam account ID
            EUserUGCList.k_EUserUGCList_Published, // Query for published items
            EUGCMatchingUGCType.k_EUGCMatchingUGCType_Collections, // Filter for collections
            EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, // Sort by title ascending,
            AppId_t.Invalid, // Consumer App ID
            new AppId_t(RimworldAppINT), // App ID
            1 // Page number
        );
        SteamUGC.SetReturnChildren(queryHandle, true);
        onSteamUgcQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
        var result = SteamUGC.SendQueryUGCRequest(queryHandle);
        onSteamUgcQueryCompletedCallResult.Set(result);
        while (collectionQueryResult.m_eResult == EResult.k_EResultNone)
        {
            Thread.Sleep(500);
            SteamAPI.RunCallbacks();
        }

        if (collectionQueryResult.m_eResult != EResult.k_EResultOK)
        {
            Log.Error($"Failed to fetch collections: {collectionQueryResult.m_eResult}");
            return collections;
        }

        for (uint i = 0; i < collectionQueryResult.m_unNumResultsReturned; i++)
        {
            SteamUGC.GetQueryUGCResult(queryHandle, i, out var details);
            if (details.m_eResult != EResult.k_EResultOK)
            {
                continue;
            }

            var collectionInfo = new CollectionInfo
            {
                CollectionName = details.m_rgchTitle,
                CollectionDescription = details.m_rgchDescription,
                CollectionId = details.m_nPublishedFileId,
                CollectionUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={details.m_nPublishedFileId}",
                CollectionItems = details.m_unNumChildren
            };

            collections.Add(collectionInfo);
        }

        SteamUGC.ReleaseQueryUGCRequest(queryHandle); // Clean up the query handle

        return collections;
    }

    private static void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
    {
        collectionQueryResult = pCallback;
    }
}