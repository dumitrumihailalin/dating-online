using API.Entities;
using API.Extensions;
using API.Intefaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseAPIController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, 
                                ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }
        [HttpPost]
        public async Task<ActionResult> AddLike([FromBody] DTO.LikedDto liked )
        {
            var authUser = await _userRepository.GetUserByUsernameAsync(liked.username);
            var likedUser = await _userRepository.GetUserByUsernameAsync(liked.likedByUsername);
            var sourceUser = await _likesRepository.GetUserWithLikes(authUser.Id);

            var userLike = await _likesRepository.GetUserLike(likedUser.Id, authUser.Id);
            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = authUser.Id,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (sourceUser.Username == liked.likedByUsername) return BadRequest("You cannot like you");

            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate, string username)
        {
            var likedUser = await _userRepository.GetMemberAsync(username);
            var users = await _likesRepository.GetUserLikes(predicate, likedUser.Id);
            return Ok(users);
        }
    }
}
