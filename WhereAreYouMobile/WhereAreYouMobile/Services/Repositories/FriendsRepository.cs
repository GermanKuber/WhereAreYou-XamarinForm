﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhereAreYouMobile.Abstractions;
using WhereAreYouMobile.Abstractions.Repositories;
using WhereAreYouMobile.Data;
using WhereAreYouMobile.Services.Repositories;
using Xamarin.Forms;
[assembly: Dependency(typeof(FriendsRepository))]
namespace WhereAreYouMobile.Services.Repositories
{
    public class FriendsRepository : IFriendsRepository
    {

        private readonly IDataService _dataService;
        private readonly ILoggerService _loggerService;

        public FriendsRepository()
        {
            _dataService = DependencyService.Get<IDataService>();
            _loggerService = DependencyService.Get<ILoggerService>();
        }

        public async Task<IEnumerable<UserProfile>> GetAllFriendsByIdAsync(string id)
        {
            try
            {
                //obtengo todos los id de mis amigos
                //TODO: debo de trabajar con un cache
                var relations = await this._dataService.Friends.Where(x => x.IdUser == id || x.IdFriend == id).ToEnumerableAsync();
       
                var idList = relations.Select(x => x.IdFriend).ToList();
                if (relations != null && relations.Any())
                {


                    var profilesFriends = await _dataService.UserProfileTable
                            .Where(x => idList.Contains(x.Id)).ToEnumerableAsync();
                    return profilesFriends;
                }
                return new List<UserProfile>();
            }
            catch (Exception e)
            {
                await _loggerService.LogErrorAsync(e);
                throw;
            }
        }

        public async Task DeleteFriendAsync(string idUser, string idFriend)
        {
            try
            {
                //obtengo todos los id de mis amigos
                //TODO: debo de trabar con un cache
                var relationShip = (await this._dataService.Friends.Where(x => (x.IdUser == idUser && x.IdFriend == idFriend)
                                                                            ||
                                                                            (x.IdUser == idFriend && x.IdFriend == idUser)).ToEnumerableAsync())?.SingleOrDefault();
              
             
                if (relationShip != null)
                {
                    await this._dataService.Friends.DeleteAsync(relationShip);
                }

            }
            catch (Exception e)
            {
                await _loggerService.LogErrorAsync(e);
                throw;
            }
        }
        /// <summary>
        /// Devuelve una amistad, si cualquiera de los dos usuarios es amigo del otro
        /// </summary>
        /// <param name="idUserMain"></param>
        /// <param name="idFriendUser"></param>
        /// <returns></returns>
        public async Task<Friend> GetFriendByBothAsync(string idUserMain, string idFriendUser)
        {
            var friend =
            (await this._dataService.Friends.Where(
                x => (x.IdUser == idUserMain && x.IdFriend == idFriendUser)
                     || (x.IdUser == idFriendUser && x.IdFriend == idUserMain)).ToEnumerableAsync())?.SingleOrDefault();
            return friend;
        }

        /// <summary>
        /// Si el Id no esta null o empty, intenta actualizar, si esta null o empty inserta
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task SaveAsync(Friend item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.Id))
                    await _dataService.Friends.InsertAsync(item);

                else
                    await _dataService.Friends.UpdateAsync(item);
            }
            catch (Exception e)
            {
                await _loggerService.LogErrorAsync(e);
                throw;
            }
        }
    }
}
