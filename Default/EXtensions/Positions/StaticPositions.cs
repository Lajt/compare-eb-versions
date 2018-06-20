using Loki.Bot;
using Loki.Common;

namespace Default.EXtensions.Positions
{
    public static class StaticPositions
    {
        public static readonly WalkablePosition StashPosAct1 = new WalkablePosition("Stash", new Vector2i(313, 279));
        public static readonly WalkablePosition StashPosAct2 = new WalkablePosition("Stash", new Vector2i(194, 206));
        public static readonly WalkablePosition StashPosAct3 = new WalkablePosition("Stash", new Vector2i(200, 310));
        public static readonly WalkablePosition StashPosAct4 = new WalkablePosition("Stash", new Vector2i(203, 520));
        public static readonly WalkablePosition StashPosAct5 = new WalkablePosition("Stash", new Vector2i(339, 335));
        public static readonly WalkablePosition StashPosAct6 = new WalkablePosition("Stash", new Vector2i(313, 417));
        public static readonly WalkablePosition StashPosAct7 = new WalkablePosition("Stash", new Vector2i(370, 631));
        public static readonly WalkablePosition StashPosAct8 = StashPosAct3;
        public static readonly WalkablePosition StashPosAct9 = StashPosAct4;
        public static readonly WalkablePosition StashPosAct10 = new WalkablePosition("Stash", new Vector2i(532, 298));
        public static readonly WalkablePosition StashPosAct11 = new WalkablePosition("Stash", new Vector2i(734, 1065));

        public static readonly WalkablePosition WaypointPosAct1 = new WalkablePosition("Waypoint", new Vector2i(256, 169));
        public static readonly WalkablePosition WaypointPosAct2 = new WalkablePosition("Waypoint", new Vector2i(188, 116));
        public static readonly WalkablePosition WaypointPosAct3 = new WalkablePosition("Waypoint", new Vector2i(219, 211));
        public static readonly WalkablePosition WaypointPosAct4 = new WalkablePosition("Waypoint", new Vector2i(281, 492));
        public static readonly WalkablePosition WaypointPosAct5 = new WalkablePosition("Waypoint", new Vector2i(317, 386));
        public static readonly WalkablePosition WaypointPosAct6 = new WalkablePosition("Waypoint", new Vector2i(256, 307));
        public static readonly WalkablePosition WaypointPosAct7 = new WalkablePosition("Waypoint", new Vector2i(330, 529));
        public static readonly WalkablePosition WaypointPosAct8 = WaypointPosAct3;
        public static readonly WalkablePosition WaypointPosAct9 = WaypointPosAct4;
        public static readonly WalkablePosition WaypointPosAct10 = new WalkablePosition("Waypoint", new Vector2i(586, 333));
        public static readonly WalkablePosition WaypointPosAct11 = new WalkablePosition("Waypoint", new Vector2i(724, 1115));

        public static readonly WalkablePosition CommonPortalSpotAct1 = new WalkablePosition("common portal spot", new Vector2i(200, 245));
        public static readonly WalkablePosition CommonPortalSpotAct2 = new WalkablePosition("common portal spot", new Vector2i(220, 168));
        public static readonly WalkablePosition CommonPortalSpotAct3 = new WalkablePosition("common portal spot", new Vector2i(230, 230));
        public static readonly WalkablePosition CommonPortalSpotAct4 = new WalkablePosition("common portal spot", new Vector2i(257, 500));
        public static readonly WalkablePosition CommonPortalSpotAct5 = new WalkablePosition("common portal spot", new Vector2i(356, 288));
        public static readonly WalkablePosition CommonPortalSpotAct6 = new WalkablePosition("common portal spot", new Vector2i(210, 385));
        public static readonly WalkablePosition CommonPortalSpotAct7 = new WalkablePosition("common portal spot", new Vector2i(315, 530));
        public static readonly WalkablePosition CommonPortalSpotAct8 = CommonPortalSpotAct3;
        public static readonly WalkablePosition CommonPortalSpotAct9 = CommonPortalSpotAct4;
        public static readonly WalkablePosition CommonPortalSpotAct10 = new WalkablePosition("common portal spot", new Vector2i(400, 285));
        public static readonly WalkablePosition CommonPortalSpotAct11 = new WalkablePosition("common portal spot", new Vector2i(695, 1200));

        public static WalkablePosition GetStashPosByAct()
        {
            switch (World.CurrentArea.Act)
            {
                case 11: return StashPosAct11;
                case 10: return StashPosAct10;
                case 9: return StashPosAct9;
                case 8: return StashPosAct8;
                case 7: return StashPosAct7;
                case 6: return StashPosAct6;
                case 5: return StashPosAct5;
                case 4: return StashPosAct4;
                case 3: return StashPosAct3;
                case 2: return StashPosAct2;
                case 1: return StashPosAct1;
            }
            GlobalLog.Error($"[GetStashPosByAct] Unknown act: {World.CurrentArea.Act}.");
            BotManager.Stop();
            return null;
        }


        public static WalkablePosition GetWaypointPosByAct()
        {
            switch (World.CurrentArea.Act)
            {
                case 11: return WaypointPosAct11;
                case 10: return WaypointPosAct10;
                case 9: return WaypointPosAct9;
                case 8: return WaypointPosAct8;
                case 7: return WaypointPosAct7;
                case 6: return WaypointPosAct6;
                case 5: return WaypointPosAct5;
                case 4: return WaypointPosAct4;
                case 3: return WaypointPosAct3;
                case 2: return WaypointPosAct2;
                case 1: return WaypointPosAct1;
            }
            GlobalLog.Error($"[GetWaypointPosByAct] Unknown act: {World.CurrentArea.Act}.");
            BotManager.Stop();
            return null;
        }

        public static WalkablePosition GetCommonPortalSpotByAct()
        {
            switch (World.CurrentArea.Act)
            {
                case 11: return CommonPortalSpotAct11;
                case 10: return CommonPortalSpotAct10;
                case 9: return CommonPortalSpotAct9;
                case 8: return CommonPortalSpotAct8;
                case 7: return CommonPortalSpotAct7;
                case 6: return CommonPortalSpotAct6;
                case 5: return CommonPortalSpotAct5;
                case 4: return CommonPortalSpotAct4;
                case 3: return CommonPortalSpotAct3;
                case 2: return CommonPortalSpotAct2;
                case 1: return CommonPortalSpotAct1;
            }
            GlobalLog.Error($"[GetCommonPortalSpotByAct] Unknown act: {World.CurrentArea.Act}.");
            BotManager.Stop();
            return null;
        }
    }
}