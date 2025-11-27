using Domain.DTOS.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public interface IPolicyService
    {
        Task<RequestResponse<bool>> AddPermissionToRoleAsync(
          string roleId,
          string permission,
          CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> RemovePermissionFromRoleAsync(
            string roleId,
            string permission,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<List<ClaimDTO>>> GetPermissionsByRoleAsync(
            string roleId,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<List<string>>> GetAllPermissionsFromUserClaimsAsync(CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> ClearAllPermissionsFromRoleAsync(
            string roleId,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> RoleHasPermissionAsync(
            string roleId,
            string permission,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> CopyPermissionsFromRoleAsync(
            string sourceRoleId,
            string targetRoleId,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<bool>> UpdateRolePermissionsAsync(
            string roleId,
            List<string> permissions,
            CancellationToken cancellationToken = default);

        Task<RequestResponse<RolePermissionsStatistics>> GetRolePermissionsStatisticsAsync(
            CancellationToken cancellationToken = default);

        Task InitializePoliciesAsync();
    }
}
