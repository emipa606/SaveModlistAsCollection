using System.Linq;
using UnityEngine;
using Verse;

namespace SaveModlistAsCollection;

[StaticConstructorOnStartup]
public class CollectionUpdateStatus_Window : Window
{
    public CollectionUpdateStatus_Window()
    {
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public override void Close(bool doCloseSound = true)
    {
        if (SaveModlistAsCollectionMod.Updating)
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("SMAC.reallyAbort".Translate(),
                delegate
                {
                    SaveModlistAsCollectionMod.Updating = false;
                    Close(doCloseSound);
                }));
            return;
        }

        base.Close(doCloseSound);
        Find.WindowStack.TryRemove(typeof(Dialog_ModCollections), false);
    }

    public override void DoWindowContents(Rect inRect)
    {
        if (SaveModlistAsCollectionMod.StatusMessages == null || !SaveModlistAsCollectionMod.StatusMessages.Any())
        {
            return;
        }

        var statusMessages = SaveModlistAsCollectionMod.StatusMessages.ToList();
        statusMessages.Reverse();
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        Text.Font = GameFont.Medium;
        listingStandard.Label("SMAC.updateStatus".Translate());
        Text.Font = GameFont.Small;
        listingStandard.Gap();
        listingStandard.End();
        var outRect = inRect;
        outRect.yMin += listingStandard.CurHeight;
        var viewRect = outRect;
        viewRect.height = (statusMessages.Count + 1) * 30f;
        if (!SaveModlistAsCollectionMod.Updating)
        {
            viewRect.height += 100f; // Add space for the close button and status icon
        }

        viewRect.width -= 16f;
        Widgets.BeginScrollView(outRect, ref SaveModlistAsCollectionMod.ScrollPosition, viewRect);
        var innerListing = new Listing_Standard();
        innerListing.Begin(viewRect);
        if (SaveModlistAsCollectionMod.Updating)
        {
            innerListing.Label(SaveModlistAsCollectionMod.ActivityMonitor ? ". . ." : " . . ");
        }
        else
        {
            var imageRect = innerListing.GetRect(50f);
            var lastStatus = statusMessages.Last();
            Widgets.DrawTextureFitted(imageRect,
                lastStatus == "SMAC.failed".Translate() ? Widgets.CheckboxOffTex : Widgets.CheckboxOnTex, 1f);

            innerListing.GapLine();
            if (innerListing.ButtonText("Close".Translate(), widthPct: 0.5f))
            {
                Close();
            }
        }

        innerListing.Gap();

        foreach (var statusMessage in statusMessages)
        {
            innerListing.Label(statusMessage);
        }

        innerListing.End();
        Widgets.EndScrollView();
    }
}