using SPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPA.Engine
{
    public static class Main
    {
        public static IEnumerable<Categorie> GetUserFavoriteCategories(
            VKProvider vkProvider,
            SocialContext context,
            RepostCategoriesRequest repostCategoriesRequest)
        {
            var result = new List<Categorie>();

            var person = vkProvider.GetPerson(repostCategoriesRequest.UserName, repostCategoriesRequest.AnalysisPeriod);

            if (person == null)
                return null;

            //var keyGroups = context.vk_keygroups.ToList();
            //FilterRepostGroups(person.GroupReposts, keyGroups);

            foreach (var gc in person.GroupReposts.Where(q => q.GroupId > 0))
            {
                var Activity = "";

                Activity = vkProvider.GetGroupInfo(gc.GroupId.ToString()).Activity;
                
                result.Add(new Categorie {
                    Name = Activity,
                    ActionCount = gc.CounterValue
                });
            }

            //var nn = vkProvider.GetLikers(person.Posts);
            //StoreLikes(context, nn);

            var groupedResult =
            result
                .GroupBy(p => p.Name)
                .Select(group => new Categorie
                {
                    Name = group.Key,
                    ActionCount = group.Sum(n => n.ActionCount)
                }).ToList();
             
            return groupedResult;
        }

        public static IEnumerable<Categorie> GetUserLikeCategories(
            VKProvider vkProvider,
            //SocialContext context,
            long userId)
        {
            var result = new List<Categorie>();

            foreach (var group in vkProvider.GetUserGroups(userId))
            {
                var actionCount = 0; var progress = 0;
                var groupPosts = vkProvider.GetWallPosts(-group.Id, false);
                foreach (var post in groupPosts.Where(x => x.Id.HasValue))
                {
                    if (vkProvider.IsLikedPost((long)post.Id, userId, -group.Id))
                        actionCount++;

                    progress++;
                }

                if (actionCount > 0)
                    result.Add(new Categorie
                    {
                        Name = vkProvider.GetGroupInfo(group.Id.ToString()).Activity,
                        ActionCount = actionCount
                    });
            }

            //foreach (var group in
            //    context.Vk_like
            //        .Where(q => q.UserId == userId)
            //        .GroupBy(g => g.OwnerId)
            //        .Select(n => new GroupCounter
            //        {
            //            GroupId = n.Key,
            //            CounterValue = n.Count()
            //        }))
            //{
            //    result.Add(new Categorie
            //    {
            //        ActionCount = group.CounterValue,
            //        Name = vkProvider.GetGroupInfo(group.GroupId.ToString()).Activity
            //    });
            //}

            return result;
        }

        public static Person GetPerson(
            VKProvider vkProvider,
            string userName
            )
        {
            return vkProvider.GetPersonByUser(userName);
        }

        private static void StoreLikes(SocialContext context, IEnumerable<VK_Like> likes)
        {
            foreach (var key in likes
                .GroupBy(p => new { p.ObjectId, p.OwnerId } )
                .Select(group => group.Key).ToList())
            {
                var existing = context.Vk_like.Where(c => c.ObjectId == key.ObjectId).ToList();
                var deltaUsers = likes
                    .Where(x => x.ObjectId == key.ObjectId && !existing.Any(y => x.UserId == y.UserId))
                    .Select(n => n.UserId)
                    .Distinct();

                context.Vk_like.AddRange(deltaUsers.Select(x => new VK_Like
                    {
                        ObjectId = key.ObjectId,
                        OwnerId = key.OwnerId,
                        UserId = x
                    }));
            }

            context.SaveChangesAsync();
        }

        private static void FilterRepostGroups(List<GroupCounter> repostGroups, List<VK_KeyGroups> definedGroups)
        {
            if (repostGroups.Count == 0)
                return;

            repostGroups.RemoveAll(x => !definedGroups.Exists(df => df.Id == x.GroupId));


        }
    }
}
