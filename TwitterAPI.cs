using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterAPI : TwitterPlugin
    {
        public string[] Test()
        {
            return new string[]
            {
                "Hello,", "Test!!!"
            };
        }

        public List<User> GetAllUsers()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> GetFollowing(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)//REF.600611030
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("follow");
            string message = request.getRequestByKey("message");
            string[] path = request.Filename.Split("?");
            if (path[0] == "users")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetAllUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        Twitter.AddUser(user, password);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("403 Forbidden");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.DeleteUser(user);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 Not Found");
                    }
                }
            }
            else if (path[0] == "following")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetFollowing(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    if (Twitter.CheckUser(following))
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 Not Found");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 Not Found");
                    }
                }
            }
            else if (path[0] == "tweets")
            {
                
                if (request.Method == "GET")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following")
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            Twitter twitter = new Twitter(user);
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 Not Found");
                    }
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        Twitter twitter = new Twitter(user);
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 Not Found");
                    }
                }
            }
            response.type = "application/json";
            return response;
        }
    }
}
