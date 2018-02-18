using SPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using VkNet.Model;
using System.Threading;

namespace SPA.Engine
{
    public class VKProvider
    {
        private VkApi api = new VkApi();

        private ILogger m_logger;

        public VKProvider(ulong appId, string login, string password, ILogger logger)
        {
            m_logger = logger;

            try
            {
                api.Authorize(new ApiAuthParams
                {
                    ApplicationId = appId,
                    Login = login,
                    Password = password,
                    Settings = Settings.All
                });
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }
        }

        //public Person GetPerson(long userId)
        //{
        //    return GetPersonWithReposts(api.Users.Get(userId, ProfileFields.BirthDate | ProfileFields.Sex | ProfileFields.StandInLife));
        //}

        public Person GetPerson(string screenName, int period)
        {
            try
            {
                var users = api.Users.Get(
                    new List<string>() { screenName },
                    ProfileFields.BirthDate | ProfileFields.Sex
                    );

                if (users.Count == 0)
                    throw new Exception("user wasn't found");

                return GetPersonWithReposts(users[0], period);
            }
            catch (VkNet.Exception.InvalidUserIdException)
            {
                return null;
            }
        }

        public Person GetPersonByUser(string screenName)
        {
            try
            {
                return GetPerson(
                    api.Users.Get(
                            new List<string>() { screenName },
                            ProfileFields.BirthDate | ProfileFields.Sex
                            )
                            .FirstOrDefault());
            }
            catch (Exception ex)
            {
                m_logger.Log($"[GetPerson] {ex.Message}");
                return null;
            }
        }

        internal IEnumerable<VkNet.Model.Group> GetUserGroups(long userId)
        {
            return api.Groups.Get(new GroupsGetParams
            {
                UserId = userId
            });
        }

        private Person GetPerson(User user)
        {
            var person = new Person
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = (int)user.Sex,
                Religion = user.StandInLife?.Religion
            };

            if (DateTime.TryParse(user.BirthDate, out DateTime bdate))
                person.BirthDate = bdate;

            return person;
        }

        private Person GetPersonWithReposts(User user, int dayCount = 365)
        {
            var person = GetPerson(user);
                
            try
            {
                person.Posts = GetWallPosts(user.Id, true);

                foreach (var group in
                    person.Posts.Where(p =>
                        p.CopyHistory.Count > 0
                        && p.Date.HasValue
                        && DateTime.Today.Subtract(p.Date.Value).TotalDays <= dayCount)

                        .GroupBy(p => p.CopyHistory.First().OwnerId)
                        .Select(group => new
                        {
                            OwnerId = -group.Key,
                            Count = group.Count()
                        }))
                {
                    person.GroupReposts.Add(new GroupCounter
                    {
                        GroupId = group.OwnerId,
                        CounterValue = group.Count
                    });
                }

                //person.Posts.AddRange(posts.Select(q => q.Id));
                //var th = new Thread(StoreLikers);
                //th.Start(posts);
                //StoreLikers(posts);

                //m_logger.Log($"{person.GroupReposts.Count} groups were found");
            }
            catch (Exception ex)
            {
                // m_logger.Log($"ERROR: {ex.Message}");
            }

            //var personGroups = api.Groups.Get(new GroupsGetParams
            //{
            //    UserId = user.Id,
            //    //Extended = true
            //});

            //var folowers = api.Users.GetFollowers(user.Id);

            //var subscriptions = api.Users.GetSubscriptions(user.Id);

            //var qq = api.Groups.GetById("flightradar24", GroupsFields.All).Activity;

            return person;
        }

        public IEnumerable<Post> GetWallPosts(long ownderId, bool getAllPosts = false)
        {
            const int maxPortionSize = 100;

            List<Post> result = new List<Post>();

            WallGetObject wgo;
            ulong receivedPostCount = 0;

            do
            {
                wgo = api.Wall.Get(new WallGetParams
                {
                    OwnerId = ownderId,
                    Extended = true,
                    Offset = receivedPostCount,
                    Count = maxPortionSize
                });

                result.AddRange(wgo.WallPosts);

                receivedPostCount += Convert.ToUInt64(wgo.WallPosts.Count);
            }
            while (wgo.TotalCount > receivedPostCount && getAllPosts);

            m_logger.Log($"{result.Count} posts in group {ownderId}");
            return result;
        }

        public IEnumerable<VK_Like> GetLikers(IEnumerable<Post> posts)
        {
            var result = new List<VK_Like>();

            foreach (var post in posts.Where(p => p.Id.HasValue))
            {
                if (post.CopyHistory.Count > 0)
                {
                    var itemId = (long)post.CopyHistory.First().Id;

                    try
                    {
                        int recordCount = 0;
                        uint offset = 0;
                        do
                        {
                            var ownerId = (long)post.CopyHistory.First().OwnerId;
                            var likesRange =
                            api.Likes.GetList(new LikesGetListParams
                            {
                                ItemId = itemId,
                                OwnerId = ownerId,
                                Type = LikeObjectType.Post,
                                Count = 1000,
                                Offset = offset
                            })
                            .Select(a => new VK_Like
                            {
                                UserId = a,
                                ObjectId = itemId,
                                OwnerId = -ownerId
                            });

                            result.AddRange(likesRange);

                            recordCount = likesRange.Count();
                            offset += 1000;
                        }
                        while (recordCount > 0);
                    }
                    catch (Exception ex)
                    {
                        m_logger.Log($"[GetLikers] {ex.Message}");
                    }
                }
            }
            return result;
        }

        public Models.Group GetGroupInfo(string groupName)
        {
            try
            {
                var groups = api.Groups.GetById(null, groupName, GroupsFields.Activity);
                return
                groups
                .Where(wc => wc.Type != GroupType.Event)
                .Select(g => new Models.Group
                {
                    Id = g.Id,
                    Name = g.Name,
                    Activity = g.Activity
                }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                m_logger.Log($"[GetGroupInfo] {ex.Message}");
                return null;
            }
        }

        public List<Models.Group> GetGroupsInfo(List<string> groupNames)
        {
            var groups = api.Groups.GetById(groupNames, null, GroupsFields.Activity);

            return
                groups.Select(g => new Models.Group
                {
                    Id = g.Id,
                    Name = g.Name,
                    Activity = g.Activity
                }).ToList();

        }

        public bool IsLikedPost(long postId, long userId, long? ownerId)
        {
            return api.Likes.IsLiked(out bool copied, LikeObjectType.Post, postId, userId, ownerId);
        }
    }
}
