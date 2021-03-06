﻿using System;
using Berry.BLL.BaseManage;
using Berry.Cache;
using Berry.Entity.BaseManage;
using System.Collections.Generic;
using System.Linq;

namespace Berry.App.Cache
{
    /// <summary>
    /// 角色信息缓存
    /// </summary>
    public class RoleCache
    {
        private RoleBLL busines = new RoleBLL();

        /// <summary>
        /// 角色列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RoleEntity> GetList()
        {
            List<RoleEntity> cacheList = CacheFactory.GetCacheInstance().GetListCache<RoleEntity>(busines.CacheKey, out long toatl);
            if (cacheList == null || cacheList.Count == 0)
            {
                cacheList = busines.GetRoleList().ToList();
                CacheFactory.GetCacheInstance().WriteListCache<RoleEntity>(cacheList, busines.CacheKey);
            }
            return cacheList;
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <returns></returns>
        public void RefreshCache(DateTime expireTime)
        {
            bool hasExpire = CacheFactory.GetCacheInstance().HasExpire(busines.CacheKey);
            if (!hasExpire)
            {
                var cacheList = busines.GetRoleList().ToList();
                CacheFactory.GetCacheInstance().WriteListCache<RoleEntity>(cacheList, busines.CacheKey, expireTime);
            }
        }

        /// <summary>
        /// 角色列表
        /// </summary>
        /// <param name="organizeId">机构Id</param>
        /// <returns></returns>
        public IEnumerable<RoleEntity> GetRoleList(string organizeId)
        {
            var data = this.GetList();
            if (!string.IsNullOrEmpty(organizeId))
            {
                data = data.Where(t => t.OrganizeId == organizeId);
            }
            return data;
        }

        /// <summary>
        /// 角色实体
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public RoleEntity GetRoleEntity(string roleId)
        {
            var data = this.GetList();
            if (!string.IsNullOrEmpty(roleId))
            {
                var d = data.Where(t => t.Id == roleId).ToList<RoleEntity>();
                if (d.Count > 0)
                {
                    return d[0];
                }
            }
            return new RoleEntity();
        }
    }
}