using System;
using System.Collections;
using System.Collections.Generic;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Positions;
using JetBrains.Annotations;
using Loki.Bot.Pathfinding;
using Loki.Common;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.EXtensions
{
    public static class ClassExtensions
    {
        public static bool EqualsIgnorecase(this string thisStr, string str)
        {
            return thisStr.Equals(str, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsIgnorecase(this string thisStr, string str)
        {
            return thisStr.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void LogProperties(this object obj)
        {
            var type = obj.GetType();
            var typeName = type.Name;
            foreach (var p in obj.GetType().GetProperties())
            {
                GlobalLog.Info($"[{typeName}] {p.Name}: {p.GetValue(obj) ?? "null"}");
            }
        }

        public static Item FindItemByPos(this Inventory inventory, Vector2i pos)
        {
            return inventory.Items.Find(i => i.LocationTopLeft == pos);
        }

        public static Item FindItemById(this Inventory inventory, int id)
        {
            return inventory.Items.Find(i => i.LocalId == id);
        }

        public static Rarity RarityLite(this Item item)
        {
            var mods = item.Components.ModsComponent;
            return mods == null ? Rarity.Normal : mods.Rarity;
        }

        public static WorldPosition WorldPosition(this NetworkObject obj)
        {
            return new WorldPosition(obj.Position);
        }

        public static WalkablePosition WalkablePosition(this NetworkObject obj, int step = 10, int radius = 30)
        {
            return new WalkablePosition(obj.Name, obj.Position, step, radius);
        }

        public static bool PathExists(this NetworkObject obj)
        {
            return ExilePather.PathExistsBetween(LokiPoe.MyPosition, obj.Position);
        }

        public static float PathDistance(this NetworkObject obj)
        {
            return ExilePather.PathDistance(LokiPoe.MyPosition, obj.Position);
        }

        public static TownNpc AsTownNpc(this NetworkObject npc)
        {
            return new TownNpc(npc);
        }

        public static bool IsPlayerPortal(this Portal p)
        {
            if (!p.IsTargetable)
                return false;

            var m = p.Metadata;
            return m == "Metadata/MiscellaneousObjects/PlayerPortal" ||
                   m == "Metadata/MiscellaneousObjects/MapReturnPortal";
        }

        public static bool LeadsTo(this Portal p, AreaInfo area)
        {
            var dest = p.Components.PortalComponent?.Area;
            return dest != null && dest == area;
        }

        public static bool LeadsTo(this Portal p, Func<DatWorldAreaWrapper, bool> match)
        {
            var dest = p.Components.PortalComponent?.Area;
            return dest != null && match(dest);
        }

        public static bool LeadsTo(this AreaTransition a, AreaInfo area)
        {
            if (a.TransitionType == TransitionTypes.Local)
                return false;

            var dest = a.Components.AreaTransitionComponent?.Destination;
            return dest != null && dest == area;
        }

        public static bool LeadsTo(this AreaTransition a, Func<DatWorldAreaWrapper, bool> match)
        {
            if (a.TransitionType == TransitionTypes.Local)
                return false;

            var dest = a.Components.AreaTransitionComponent?.Destination;
            return dest != null && match(dest);
        }

        public static T Fresh<T>(this T obj) where T : NetworkObject
        {
            return LokiPoe.ObjectManager.Objects.Find(o => o.Id == obj.Id) as T;
        }

        [CanBeNull]
        public static T Closest<T>(this IEnumerable<T> collection) where T : NetworkObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                if (closest == null || element.DistanceSqr < closest.DistanceSqr)
                    closest = element;
            }
            return closest;
        }

        [CanBeNull]
        public static T Closest<T>(this IEnumerable collection) where T : NetworkObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                var typed = element as T;
                if (typed != null)
                {
                    if (closest == null || typed.DistanceSqr < closest.DistanceSqr)
                        closest = typed;
                }
            }
            return closest;
        }

        [CanBeNull]
        public static T Closest<T>(this IEnumerable<T> collection, Func<T, bool> match) where T : NetworkObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                if (match(element))
                {
                    if (closest == null || element.DistanceSqr < closest.DistanceSqr)
                        closest = element;
                }
            }
            return closest;
        }

        [CanBeNull]
        public static T Closest<T>(this IEnumerable collection, Func<T, bool> match) where T : NetworkObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                var typed = element as T;
                if (typed != null && match(typed))
                {
                    if (closest == null || typed.DistanceSqr < closest.DistanceSqr)
                        closest = typed;
                }
            }
            return closest;
        }

        [CanBeNull]
        public static T FirstOrDefault<T>(this IEnumerable collection) where T : class
        {
            foreach (var element in collection)
            {
                var t = element as T;
                if (t != null)
                    return t;
            }
            return null;
        }

        [CanBeNull]
        public static T FirstOrDefault<T>(this IEnumerable collection, Func<T, bool> match) where T : class
        {
            foreach (var element in collection)
            {
                var t = element as T;
                if (t != null && match(t))
                    return t;
            }
            return null;
        }

        public static bool Any<T>(this IEnumerable collection, Func<T, bool> match) where T : class
        {
            foreach (var element in collection)
            {
                var t = element as T;
                if (t != null && match(t))
                    return true;
            }
            return false;
        }

        public static IEnumerable<T> Where<T>(this IEnumerable collection, Func<T, bool> match) where T : class
        {
            foreach (var element in collection)
            {
                var t = element as T;
                if (t != null && match(t))
                    yield return t;
            }
        }

        public static IEnumerable<T> Valid<T>(this IEnumerable<T> collection) where T : CachedObject
        {
            foreach (var element in collection)
            {
                if (!element.Ignored && !element.Unwalkable)
                    yield return element;
            }
        }

        public static IEnumerable<T> Valid<T>(this IEnumerable<T> collection, Func<T, bool> match) where T : CachedObject
        {
            foreach (var element in collection)
            {
                if (!element.Ignored && !element.Unwalkable && match(element))
                    yield return element;
            }
        }

        [CanBeNull]
        public static T ClosestValid<T>(this IEnumerable<T> collection) where T : CachedObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                if (!element.Ignored && !element.Unwalkable)
                {
                    if (closest == null || element.Position.DistanceSqr < closest.Position.DistanceSqr)
                        closest = element;
                }
            }
            return closest;
        }

        [CanBeNull]
        public static T ClosestValid<T>(this IEnumerable<T> collection, Func<T, bool> match) where T : CachedObject
        {
            T closest = null;
            foreach (var element in collection)
            {
                if (!element.Ignored && !element.Unwalkable && match(element))
                {
                    if (closest == null || element.Position.DistanceSqr < closest.Position.DistanceSqr)
                        closest = element;
                }
            }
            return closest;
        }
    }
}