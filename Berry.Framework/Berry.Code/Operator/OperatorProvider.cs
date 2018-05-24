﻿using Berry.Cache;
using Berry.Extension;
using Berry.Util;
using System;

namespace Berry.Code.Operator
{
    public class OperatorProvider : IOperatorProvider
    {
        #region 静态实例

        /// <summary>
        /// 当前提供者
        /// </summary>
        public static IOperatorProvider Provider => new OperatorProvider();

        #endregion 静态实例

        /// <summary>
        /// 秘钥
        /// </summary>
        private string LoginUserKey = "__LoginUserKey";

        /// <summary>
        /// 登陆提供者模式:Session、Cookie
        /// </summary>
        private readonly string _loginProvider = ConfigHelper.GetValue("LoginProvider");

        /// <summary>
        /// 写入登录信息
        /// </summary>
        /// <param name="user">成员信息</param>
        public virtual void AddCurrent(OperatorEntity user)
        {
            try
            {
                if (_loginProvider == "Cookie")
                {
                    //#region 解决cookie时，设置数据权限较多时无法登陆的bug
                    //CacheFactory.GetCacheInstance().WriteCache(user.DataAuthorize, LoginUserKey, user.LoginTime.AddHours(12));
                    //user.DataAuthorize = null;
                    //#endregion

                    CookieHelper.WriteCookie(LoginUserKey, DESEncryptHelper.Encrypt(user.TryToJson()));
                }
                else if (_loginProvider == "Session")
                {
                    SessionHelper.AddSession(LoginUserKey, DESEncryptHelper.Encrypt(user.TryToJson()));
                }
                else if (_loginProvider == "Cache")
                {
                    CacheFactory.GetCacheInstance().WriteCache(DESEncryptHelper.Encrypt(user.TryToJson()), LoginUserKey, user.LoginTime.AddHours(1));
                }

                CacheFactory.GetCacheInstance().WriteCache(user.Token, user.UserId, user.LoginTime.AddHours(12));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        /// <returns></returns>
        public virtual OperatorEntity Current()
        {
            try
            {
                OperatorEntity user = new OperatorEntity
                {
                    UserId = "",
                    UserName = ""
                };
                if (_loginProvider == "Cookie")
                {
                    string json = CookieHelper.GetCookie(LoginUserKey).ToString();
                    if (!string.IsNullOrEmpty(json))
                    {
                        user = DESEncryptHelper.Decrypt(json).JsonToEntity<OperatorEntity>();

                        #region 解决cookie时，设置数据权限较多时无法登陆的bug

                        AuthorizeDataModel dataAuthorize = CacheFactory.GetCacheInstance().GetCache<AuthorizeDataModel>(LoginUserKey);
                        user.DataAuthorize = dataAuthorize;

                        #endregion 解决cookie时，设置数据权限较多时无法登陆的bug
                    }
                }
                else if (_loginProvider == "Session")
                {
                    string json = SessionHelper.GetSession<string>(LoginUserKey).ToString();
                    if (!string.IsNullOrEmpty(json))
                    {
                        user = DESEncryptHelper.Decrypt(json).JsonToEntity<OperatorEntity>();
                    }
                }
                else if (_loginProvider == "Cache")
                {
                    string json = CacheFactory.GetCacheInstance().GetCache<string>(LoginUserKey);
                    if (!string.IsNullOrEmpty(json))
                    {
                        user = DESEncryptHelper.Decrypt(json).JsonToEntity<OperatorEntity>();
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 删除登录信息
        /// </summary>
        public virtual void EmptyCurrent()
        {
            if (_loginProvider == "Cookie")
            {
                CookieHelper.DelCookie(LoginUserKey.Trim());

                #region 解决cookie时，设置数据权限较多时无法登陆的bug

                CacheFactory.GetCacheInstance().RemoveCache(LoginUserKey);

                #endregion 解决cookie时，设置数据权限较多时无法登陆的bug
            }
            else if (_loginProvider == "Session")
            {
                SessionHelper.RemoveSession(LoginUserKey.Trim());
            }
            else if (_loginProvider == "Cache")
            {
                CacheFactory.GetCacheInstance().RemoveCache(LoginUserKey);
            }
        }

        /// <summary>
        /// 是否过期
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOverdue()
        {
            try
            {
                string str = "";
                if (_loginProvider == "Cookie")
                {
                    str = CookieHelper.GetCookie(LoginUserKey);

                    #region 解决cookie时，设置数据权限较多时无法登陆的bug

                    AuthorizeDataModel dataAuthorize = CacheFactory.GetCacheInstance().GetCache<AuthorizeDataModel>(LoginUserKey);

                    if (dataAuthorize == null)
                    {
                        return true;
                    }

                    #endregion 解决cookie时，设置数据权限较多时无法登陆的bug
                }
                else if (_loginProvider == "Session")
                {
                    str = SessionHelper.GetSession<string>(LoginUserKey);
                }
                else if (_loginProvider == "Cache")
                {
                    str = CacheFactory.GetCacheInstance().GetCache<string>(LoginUserKey);
                }

                return string.IsNullOrEmpty(str);
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// 是否已登录
        /// </summary>
        /// <returns></returns>
        public virtual int IsOnLine()
        {
            OperatorEntity user = new OperatorEntity();
            if (_loginProvider == "Cookie")
            {
                user = DESEncryptHelper.Decrypt(CookieHelper.GetCookie(LoginUserKey).ToString()).JsonToEntity<OperatorEntity>();

                #region 解决cookie时，设置数据权限较多时无法登陆的bug

                AuthorizeDataModel dataAuthorize = CacheFactory.GetCacheInstance().GetCache<AuthorizeDataModel>(LoginUserKey);
                user.DataAuthorize = dataAuthorize;

                #endregion 解决cookie时，设置数据权限较多时无法登陆的bug
            }
            else if (_loginProvider == "Session")
            {
                user = DESEncryptHelper.Decrypt(SessionHelper.GetSession<string>(LoginUserKey).ToString()).JsonToEntity<OperatorEntity>();
            }
            else if (_loginProvider == "Cache")
            {
                string json = CacheFactory.GetCacheInstance().GetCache<string>(LoginUserKey);
                if (!string.IsNullOrEmpty(json))
                {
                    user = DESEncryptHelper.Decrypt(json).JsonToEntity<OperatorEntity>();
                }
            }

            object token = CacheFactory.GetCacheInstance().GetCache<string>(user.UserId);
            if (token == null)
            {
                return -1;//过期
            }
            if (user.Token == token.ToString())
            {
                return 1;//正常
            }
            else
            {
                return 0;//已登录
            }
        }

        /// <summary>
        /// 当前Tab页面模块Id
        /// </summary>
        public static string CurrentModuleId
        {
            get
            {
                return CookieHelper.GetCookie("currentmoduleId");
            }
        }
    }
}