using System.Threading;
using UnityEngine;
using Verse;

namespace SaveModlistAsCollection;

[StaticConstructorOnStartup]
public class Dialog_ModCollections : Window
{
    private const int HeaderHeight = 50;
    private const int RowHeight = 60;
    private static Vector2 scrollPosition;
    private static readonly Color alternateBackground = new(0.3f, 0.3f, 0.3f, 0.5f);

    public Dialog_ModCollections()
    {
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new(700f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        Text.Font = GameFont.Medium;

        listingStandard.Label("SMAC.Collections".Translate());

        Text.Font = GameFont.Small;
        listingStandard.End();

        var borderRect = inRect;
        borderRect.y += HeaderHeight;
        borderRect.height -= HeaderHeight;
        var scrollContentRect = inRect;
        scrollContentRect.height = SaveModlistAsCollectionMod.UsersCollections.Count * (RowHeight + 1);

        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);

        var alternate = false;

        foreach (var collection in SaveModlistAsCollectionMod.UsersCollections)
        {
            var rowRectFull = scrollListing.GetRect(RowHeight);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRectFull, alternateBackground);
            }

            var rowRectLeft = rowRectFull.ContractedBy(5f).LeftPart(0.9f);
            var rowRectRight = rowRectFull.ContractedBy(5f).RightPart(0.1f);

            Widgets.Label(rowRectLeft.TopHalf(), collection.CollectionName);
            Widgets.Label(rowRectLeft.BottomHalf(), "SMAC.Items".Translate(collection.CollectionItems));
            TooltipHandler.TipRegion(rowRectLeft, collection.CollectionDescription);
            Widgets.DrawHighlightIfMouseover(rowRectLeft);
            if (Widgets.ButtonInvisible(rowRectLeft))
            {
                Application.OpenURL(collection.CollectionUrl);
            }

            if (Widgets.ButtonText(rowRectRight, "Save".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "SMAC.warning".Translate(collection.CollectionItems),
                    delegate
                    {
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            SaveModlistAsCollectionMod.UpdateCollection(collection);
                        }).Start();
                        Find.WindowStack.Add(new CollectionUpdateStatus_Window());
                    }));
            }
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }
}