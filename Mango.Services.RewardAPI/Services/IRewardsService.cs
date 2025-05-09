using Mango.Services.RewardAPI.Message;

namespace Mango.Services.RewardAPI.Services
{
    public interface IRewardsService
    {
        Task UpdateRewards(RewardsMessage rewardsMessage);
    }
}
